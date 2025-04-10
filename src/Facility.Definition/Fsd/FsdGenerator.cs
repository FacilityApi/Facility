using System.Text.RegularExpressions;
using Facility.Definition.CodeGen;

namespace Facility.Definition.Fsd;

/// <summary>
/// Generates an FSD file for a service definition.
/// </summary>
public sealed class FsdGenerator : CodeGenerator
{
	/// <summary>
	/// Generates an FSD file for a service definition.
	/// </summary>
	/// <param name="parser">The service parser.</param>
	/// <param name="settings">The settings.</param>
	/// <returns>The number of updated files.</returns>
	public static int GenerateFsd(ServiceParser parser, FsdGeneratorSettings settings) =>
		FileGenerator.GenerateFiles(parser, new FsdGenerator { GeneratorName = nameof(FsdGenerator) }, settings);

	/// <summary>
	/// Generates an FSD file for a service definition.
	/// </summary>
	/// <param name="settings">The settings.</param>
	/// <returns>The number of updated files.</returns>
	[Obsolete("Use the overload that takes a parser.")]
	public static int GenerateFsd(FsdGeneratorSettings settings) =>
		FileGenerator.GenerateFiles(new FsdGenerator { GeneratorName = nameof(FsdGenerator) }, settings);

	/// <summary>
	/// True to generate a file-scoped service (instead of using braces).
	/// </summary>
	public bool FileScopedService { get; set; }

	/// <summary>
	/// Generates an FSD file for a service definition.
	/// </summary>
	public override CodeGenOutput GenerateOutput(ServiceInfo service)
	{
		var output = CreateFile($"{service.Name}.fsd", code =>
		{
			if (!string.IsNullOrWhiteSpace(GeneratorName))
			{
				code.WriteLine("// " + CodeGenUtility.GetCodeGenComment(GeneratorName!));
				code.WriteLine();
			}

			var remarks = new List<string>();
			if (service.Remarks.Count != 0)
				remarks.AddRange(new[] { "", $"# {service.Name}", "" }.Concat(service.Remarks));

			WriteSummaryAndAttributes(code, service);
			code.WriteLine($"service {service.Name}{(FileScopedService ? ";" : "")}");
			using (FileScopedService ? null : code.Block())
			{
				foreach (var member in service.Members)
				{
					if (FileScopedService)
						code.WriteLine();
					else
						code.WriteLineSkipOnce();

					if (member is ServiceMethodInfo method)
					{
						WriteSummaryAndAttributes(code, method);
						code.WriteLine($"{method.Kind.GetKeyword()} {method.Name}");
						using (code.Block("{", "}:"))
							WriteFields(code, method.RequestFields);
						using (code.Block())
							WriteFields(code, method.ResponseFields);

						if (method.Remarks.Count != 0)
							remarks.AddRange(new[] { "", $"# {method.Name}", "" }.Concat(method.Remarks));
					}
					else if (member is ServiceDtoInfo dto)
					{
						WriteSummaryAndAttributes(code, dto);
						code.WriteLine($"data {dto.Name}");
						using (code.Block())
							WriteFields(code, dto.Fields);

						if (dto.Remarks.Count != 0)
							remarks.AddRange(new[] { "", $"# {dto.Name}", "" }.Concat(dto.Remarks));
					}
					else if (member is ServiceEnumInfo enumInfo)
					{
						WriteSummaryAndAttributes(code, enumInfo);
						code.WriteLine($"enum {enumInfo.Name}");
						using (code.Block())
							WriteEnumValues(code, enumInfo.Values);

						if (enumInfo.Remarks.Count != 0)
							remarks.AddRange(new[] { "", $"# {enumInfo.Name}", "" }.Concat(enumInfo.Remarks));
					}
					else if (member is ServiceErrorSetInfo errorSet)
					{
						WriteSummaryAndAttributes(code, errorSet);
						code.WriteLine($"errors {errorSet.Name}");
						using (code.Block())
							WriteErrors(code, errorSet.Errors);

						if (errorSet.Remarks.Count != 0)
							remarks.AddRange(new[] { "", $"# {errorSet.Name}", "" }.Concat(errorSet.Remarks));
					}
					else if (member is ServiceExternalDtoInfo externalDto)
					{
						WriteSummaryAndAttributes(code, externalDto);
						code.WriteLine($"extern data {externalDto.Name};");

						if (externalDto.Remarks.Count != 0)
							remarks.AddRange(new[] { "", $"# {externalDto.Name}", "" }.Concat(externalDto.Remarks));
					}
					else if (member is ServiceExternalEnumInfo externalEnum)
					{
						WriteSummaryAndAttributes(code, externalEnum);
						code.WriteLine($"extern enum {externalEnum.Name};");

						if (externalEnum.Remarks.Count != 0)
							remarks.AddRange(new[] { "", $"# {externalEnum.Name}", "" }.Concat(externalEnum.Remarks));
					}
					else
					{
						throw new InvalidOperationException("Unexpected member type.");
					}
				}
			}

			foreach (var remark in remarks)
				code.WriteLine(remark);
		});
		return new CodeGenOutput(output);
	}

	/// <summary>
	/// Applies generator-specific settings.
	/// </summary>
	public override void ApplySettings(FileGeneratorSettings settings)
	{
		var fsdSettings = (FsdGeneratorSettings) settings;
		FileScopedService = fsdSettings.FileScopedService;
	}

	/// <summary>
	/// The generator writes output to a single file.
	/// </summary>
	public override bool SupportsSingleOutput => true;

	private static void WriteSummaryAndAttributes<T>(CodeWriter code, T info)
		where T : ServiceElementWithAttributesInfo, IServiceHasSummary
	{
		WriteSummary(code, info.Summary);
		WriteAttributes(code, info.Attributes);
	}

	private static void WriteSummary(CodeWriter code, string summary)
	{
		if (!string.IsNullOrEmpty(summary))
			code.WriteLine($"/// {summary}");
	}

	private static void WriteAttributes(CodeWriter code, IEnumerable<ServiceAttributeInfo> attributes)
	{
		foreach (var attribute in attributes)
			WriteAttribute(code, attribute);
	}

	private static void WriteAttribute(CodeWriter code, ServiceAttributeInfo attribute)
	{
		var parameters = string.Join(", ", attribute.Parameters.Select(RenderAttributeParameter));
		if (parameters.Length != 0)
			parameters = $"({parameters})";
		code.WriteLine($"[{attribute.Name}{parameters}]");
	}

	private static string RenderAttributeParameter(ServiceAttributeParameterInfo parameter) =>
		$"{parameter.Name}: {RenderAttributeParameterValue(parameter)}";

	private static string RenderAttributeParameterValue(ServiceAttributeParameterInfo parameter)
	{
		if (s_unquotedAttributeValueRegex.IsMatch(parameter.Value))
			return parameter.Value;

		return "\"" + s_escapeAttributeValueRegex.Replace(parameter.Value, RenderAttributeValueEscape) + "\"";
	}

	private static string RenderAttributeValueEscape(Match match) =>
		match.Value[0] switch
		{
			'\\' => @"\\",
			'"' => @"\""",
			'\b' => @"\b",
			'\f' => @"\f",
			'\n' => @"\n",
			'\r' => @"\r",
			'\t' => @"\t",
			var ch => $@"\u{(int) ch:x4}",
		};

	private static void WriteFields(CodeWriter code, IEnumerable<ServiceFieldInfo> fields)
	{
		foreach (var field in fields)
		{
			code.WriteLineSkipOnce();
			WriteSummary(code, field.Summary);

			var attributes = field.Attributes;
			if (field.IsRequired)
				attributes = [.. attributes.Where(x => x.Name != "required")];
			WriteAttributes(code, attributes);

			code.WriteLine($"{field.Name}: {field.TypeName}{(field.IsRequired ? "!" : "")};");
		}
	}

	private static void WriteEnumValues(CodeWriter code, IEnumerable<ServiceEnumValueInfo> enumValues)
	{
		foreach (var enumValue in enumValues)
		{
			code.WriteLineSkipOnce();
			WriteSummaryAndAttributes(code, enumValue);
			code.WriteLine($"{enumValue.Name},");
		}
	}

	private static void WriteErrors(CodeWriter code, IEnumerable<ServiceErrorInfo> errors)
	{
		foreach (var error in errors)
		{
			code.WriteLineSkipOnce();
			WriteSummaryAndAttributes(code, error);
			code.WriteLine($"{error.Name},");
		}
	}

	private static readonly Regex s_unquotedAttributeValueRegex = new("^[0-9a-zA-Z.+_-]+$");
	private static readonly Regex s_escapeAttributeValueRegex = new(@"[\\""\u0000-\u001F]");
}

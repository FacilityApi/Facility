using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Facility.Definition.CodeGen;

namespace Facility.Definition.Fsd
{
	/// <summary>
	/// Generates an FSD file for a service definition.
	/// </summary>
	public sealed class FsdGenerator : CodeGenerator
	{
		/// <summary>
		/// Generates an FSD file for a service definition.
		/// </summary>
		protected override CodeGenOutput GenerateOutputCore(ServiceInfo service)
		{
			var output = CreateNamedText(service.Name + ".fsd", code =>
			{
				if (!string.IsNullOrWhiteSpace(GeneratorName))
				{
					code.WriteLine("// " + CodeGenUtility.GetCodeGenComment(GeneratorName));
					code.WriteLine();
				}

				var remarks = new List<string>();
				if (service.Remarks.Count != 0)
					remarks.AddRange(new[] { "", $"# {service.Name}", "" }.Concat(service.Remarks));

				WriteSummaryAndAttributes(code, service);
				code.WriteLine($"service {service.Name}");
				using (code.Block())
				{
					foreach (var member in service.Members)
					{
						var method = member as ServiceMethodInfo;
						if (method != null)
						{
							code.WriteLineSkipOnce();
							WriteSummaryAndAttributes(code, method);
							code.WriteLine($"method {method.Name}");
							using (code.Block("{", "}:"))
								WriteFields(code, method.RequestFields);
							using (code.Block())
								WriteFields(code, method.ResponseFields);

							if (method.Remarks.Count != 0)
								remarks.AddRange(new[] { "", $"# {method.Name}", "" }.Concat(method.Remarks));
						}

						var dto = member as ServiceDtoInfo;
						if (dto != null)
						{
							code.WriteLineSkipOnce();
							WriteSummaryAndAttributes(code, dto);
							code.WriteLine($"data {dto.Name}");
							using (code.Block())
								WriteFields(code, dto.Fields);

							if (dto.Remarks.Count != 0)
								remarks.AddRange(new[] { "", $"# {dto.Name}", "" }.Concat(dto.Remarks));
						}

						var enumInfo = member as ServiceEnumInfo;
						if (enumInfo != null)
						{
							code.WriteLineSkipOnce();
							WriteSummaryAndAttributes(code, enumInfo);
							code.WriteLine($"enum {enumInfo.Name}");
							using (code.Block())
								WriteEnumValues(code, enumInfo.Values);

							if (enumInfo.Remarks.Count != 0)
								remarks.AddRange(new[] { "", $"# {enumInfo.Name}", "" }.Concat(enumInfo.Remarks));
						}

						var errorSet = member as ServiceErrorSetInfo;
						if (errorSet != null)
						{
							code.WriteLineSkipOnce();
							WriteSummaryAndAttributes(code, errorSet);
							code.WriteLine($"errors {errorSet.Name}");
							using (code.Block())
								WriteErrors(code, errorSet.Errors);

							if (errorSet.Remarks.Count != 0)
								remarks.AddRange(new[] { "", $"# {errorSet.Name}", "" }.Concat(errorSet.Remarks));
						}
					}
				}

				foreach (string remark in remarks)
					code.WriteLine(remark);
			});
			return new CodeGenOutput(output);
		}

		private void WriteSummaryAndAttributes(CodeWriter code, IServiceElementInfo info)
		{
			WriteSummary(code, info.Summary);
			WriteAttributes(code, info.Attributes);
		}

		private void WriteSummary(CodeWriter code, string summary)
		{
			if (!string.IsNullOrEmpty(summary))
				code.WriteLine($"/// {summary}");
		}

		private void WriteAttributes(CodeWriter code, IEnumerable<ServiceAttributeInfo> attributes)
		{
			foreach (var attribute in attributes)
				WriteAttribute(code, attribute);
		}

		private void WriteAttribute(CodeWriter code, ServiceAttributeInfo attribute)
		{
			string parameters = string.Join(", ", attribute.Parameters.Select(RenderAttributeParameter));
			if (parameters.Length != 0)
				parameters = $"({parameters})";
			code.WriteLine($"[{attribute.Name}{parameters}]");
		}

		private string RenderAttributeParameter(ServiceAttributeParameterInfo parameter)
		{
			return $"{parameter.Name}: {RenderAttributeParameterValue(parameter)}";
		}

		private string RenderAttributeParameterValue(ServiceAttributeParameterInfo parameter)
		{
			if (s_unquotedAttributeValueRegex.IsMatch(parameter.Value))
				return parameter.Value;

			return "\"" + s_escapeAttributeValueRegex.Replace(parameter.Value, RenderAttributeValueEscape) + "\"";
		}

		private string RenderAttributeValueEscape(Match match)
		{
			char ch = match.Value[0];
			switch (ch)
			{
			case '\\':
				return @"\\";
			case '"':
				return @"\""";
			case '\b':
				return @"\b";
			case '\f':
				return @"\f";
			case '\n':
				return @"\n";
			case '\r':
				return @"\r";
			case '\t':
				return @"\t";
			default:
				return $@"\u{(int) ch:x4}";
			}
		}

		private void WriteFields(CodeWriter code, IEnumerable<ServiceFieldInfo> fields)
		{
			foreach (var field in fields)
			{
				code.WriteLineSkipOnce();
				WriteSummaryAndAttributes(code, field);
				code.WriteLine($"{field.Name}: {field.TypeName};");
			}
		}

		private void WriteEnumValues(CodeWriter code, IEnumerable<ServiceEnumValueInfo> enumValues)
		{
			foreach (var enumValue in enumValues)
			{
				code.WriteLineSkipOnce();
				WriteSummaryAndAttributes(code, enumValue);
				code.WriteLine($"{enumValue.Name},");
			}
		}

		private void WriteErrors(CodeWriter code, IEnumerable<ServiceErrorInfo> errors)
		{
			foreach (var error in errors)
			{
				code.WriteLineSkipOnce();
				WriteSummaryAndAttributes(code, error);
				code.WriteLine($"{error.Name},");
			}
		}

		static readonly Regex s_unquotedAttributeValueRegex = new Regex(@"^[0-9a-zA-Z.+_-]+$");
		static readonly Regex s_escapeAttributeValueRegex = new Regex(@"[\\""\u0000-\u001F]");
	}
}

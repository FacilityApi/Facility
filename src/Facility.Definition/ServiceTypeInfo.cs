namespace Facility.Definition;

/// <summary>
/// A service type.
/// </summary>
public sealed class ServiceTypeInfo
{
	/// <summary>
	/// Create a primitive type of the specified kind.
	/// </summary>
	public static ServiceTypeInfo CreatePrimitive(ServiceTypeKind kind)
	{
		if (kind is ServiceTypeKind.Dto or ServiceTypeKind.Enum or ServiceTypeKind.ExternalDto or ServiceTypeKind.ExternalEnum or ServiceTypeKind.Result or ServiceTypeKind.Array or ServiceTypeKind.Map or ServiceTypeKind.Nullable)
			throw new ArgumentOutOfRangeException(nameof(kind), "Kind must be primitive.");
		return new ServiceTypeInfo(kind);
	}

	/// <summary>
	/// Create a DTO type.
	/// </summary>
	public static ServiceTypeInfo CreateDto(ServiceDtoInfo dto) => new(ServiceTypeKind.Dto, dto: dto);

	/// <summary>
	/// Create an enumerated type.
	/// </summary>
	public static ServiceTypeInfo CreateEnum(ServiceEnumInfo @enum) => new(ServiceTypeKind.Enum, @enum: @enum);

	/// <summary>
	/// Create an external DTO type.
	/// </summary>
	public static ServiceTypeInfo CreateExternalDto(ServiceExternalDtoInfo externalDto) => new(ServiceTypeKind.ExternalDto, externalDto: externalDto);

	/// <summary>
	/// Create an external enumerated type.
	/// </summary>
	public static ServiceTypeInfo CreateExternalEnum(ServiceExternalEnumInfo externalEnum) => new(ServiceTypeKind.ExternalEnum, externalEnum: externalEnum);

	/// <summary>
	/// Create a service result type.
	/// </summary>
	public static ServiceTypeInfo CreateResult(ServiceTypeInfo valueType) => new(ServiceTypeKind.Result, valueType: valueType);

	/// <summary>
	/// Create an array type.
	/// </summary>
	public static ServiceTypeInfo CreateArray(ServiceTypeInfo valueType) => new(ServiceTypeKind.Array, valueType: valueType);

	/// <summary>
	/// Create a map type.
	/// </summary>
	public static ServiceTypeInfo CreateMap(ServiceTypeInfo valueType) => new(ServiceTypeKind.Map, valueType: valueType);

	/// <summary>
	/// Create a nullable type.
	/// </summary>
	public static ServiceTypeInfo CreateNullable(ServiceTypeInfo valueType) => new(ServiceTypeKind.Nullable, valueType: valueType);

	/// <summary>
	/// The kind of type.
	/// </summary>
	public ServiceTypeKind Kind { get; }

	/// <summary>
	/// The DTO (when Kind is Dto).
	/// </summary>
	public ServiceDtoInfo? Dto { get; }

	/// <summary>
	/// The enumerated type (when Kind is Enum).
	/// </summary>
	public ServiceEnumInfo? Enum { get; }

	/// <summary>
	/// The external DTO (when Kind is ExternalDto).
	/// </summary>
	public ServiceExternalDtoInfo? ExternalDto { get; }

	/// <summary>
	/// The external enumerated type (when Kind is ExternalEnum).
	/// </summary>
	public ServiceExternalEnumInfo? ExternalEnum { get; }

	/// <summary>
	/// The value type (when Kind is Result, Array, or Map).
	/// </summary>
	public ServiceTypeInfo? ValueType { get; }

	/// <summary>
	/// The string form of the service type.
	/// </summary>
	public override string ToString()
	{
		return Kind switch
		{
			ServiceTypeKind.Dto => Dto!.Name,
			ServiceTypeKind.Enum => Enum!.Name,
			ServiceTypeKind.ExternalDto => ExternalDto!.Name,
			ServiceTypeKind.ExternalEnum => ExternalEnum!.Name,
			ServiceTypeKind.Result => $"result<{ValueType}>",
			ServiceTypeKind.Array => $"{ValueType}[]",
			ServiceTypeKind.Map => $"map<{ValueType}>",
			ServiceTypeKind.Nullable => $"nullable<{ValueType}>",
			_ => s_primitives.Where(x => x.Kind == Kind).Select(x => x.Name).Single(),
		};
	}

	internal static ServiceTypeInfo? TryParse(string text, Func<string, ServiceMemberInfo?> findMember)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var primitive = s_primitives.FirstOrDefault(x => text == x.Name);
		if (primitive.Name is not null)
			return CreatePrimitive(primitive.Kind);

		var resultValueType = TryPrefixSuffix(text, "result<", ">");
		if (resultValueType is not null)
		{
			var valueType = TryParse(resultValueType, findMember);
			return valueType is null ? null : CreateResult(valueType);
		}

		var arrayValueType = TryPrefixSuffix(text, "", "[]");
		if (arrayValueType is not null)
		{
			var valueType = TryParse(arrayValueType, findMember);
			return valueType is null ? null : CreateArray(valueType);
		}

		var mapValueType = TryPrefixSuffix(text, "map<", ">");
		if (mapValueType is not null)
		{
			var valueType = TryParse(mapValueType, findMember);
			return valueType is null ? null : CreateMap(valueType);
		}

		var nullableValueType = TryPrefixSuffix(text, "nullable<", ">");
		if (nullableValueType is not null)
		{
			var valueType = TryParse(nullableValueType, findMember);
			return valueType is null || valueType.Kind == ServiceTypeKind.Nullable ? null : CreateNullable(valueType);
		}

		var member = findMember(text);
		if (member is not null)
		{
			if (member is ServiceDtoInfo dto)
				return CreateDto(dto);

			if (member is ServiceEnumInfo @enum)
				return CreateEnum(@enum);

			if (member is ServiceExternalDtoInfo externalDto)
				return CreateExternalDto(externalDto);

			if (member is ServiceExternalEnumInfo externalEnum)
				return CreateExternalEnum(externalEnum);
		}

		return null;
	}

	private ServiceTypeInfo(ServiceTypeKind kind, ServiceDtoInfo? dto = null, ServiceEnumInfo? @enum = null, ServiceExternalDtoInfo? externalDto = null, ServiceExternalEnumInfo? externalEnum = null, ServiceTypeInfo? valueType = null)
	{
		Kind = kind;
		Dto = dto;
		Enum = @enum;
		ExternalDto = externalDto;
		ExternalEnum = externalEnum;
		ValueType = valueType;
	}

	private static string? TryPrefixSuffix(string text, string prefix, string suffix)
	{
		return text.StartsWith(prefix, StringComparison.Ordinal) && text.EndsWith(suffix, StringComparison.Ordinal) ?
			text.Substring(prefix.Length, text.Length - prefix.Length - suffix.Length) : null;
	}

	private static readonly (ServiceTypeKind Kind, string Name)[] s_primitives =
	{
		(ServiceTypeKind.String, "string"),
		(ServiceTypeKind.Boolean, "boolean"),
		(ServiceTypeKind.Double, "double"),
		(ServiceTypeKind.Int32, "int32"),
		(ServiceTypeKind.Int64, "int64"),
		(ServiceTypeKind.Decimal, "decimal"),
		(ServiceTypeKind.Bytes, "bytes"),
		(ServiceTypeKind.Object, "object"),
		(ServiceTypeKind.Error, "error"),
		(ServiceTypeKind.DateTime, "datetime"),
	};
}

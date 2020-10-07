using System;
using System.Linq;

namespace Facility.Definition
{
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
			if (kind == ServiceTypeKind.Dto || kind == ServiceTypeKind.Enum || kind == ServiceTypeKind.Result || kind == ServiceTypeKind.Array || kind == ServiceTypeKind.Map)
				throw new ArgumentOutOfRangeException(nameof(kind), "Kind must be primitive.");
			return new ServiceTypeInfo(kind);
		}

		/// <summary>
		/// Create a DTO type.
		/// </summary>
		public static ServiceTypeInfo CreateDto(ServiceDtoInfo dto) => new ServiceTypeInfo(ServiceTypeKind.Dto, dto: dto);

		/// <summary>
		/// Create an enumerated type.
		/// </summary>
		public static ServiceTypeInfo CreateEnum(ServiceEnumInfo @enum) => new ServiceTypeInfo(ServiceTypeKind.Enum, @enum: @enum);

		/// <summary>
		/// Create a service result type.
		/// </summary>
		public static ServiceTypeInfo CreateResult(ServiceTypeInfo valueType) => new ServiceTypeInfo(ServiceTypeKind.Result, valueType: valueType);

		/// <summary>
		/// Create an array type.
		/// </summary>
		public static ServiceTypeInfo CreateArray(ServiceTypeInfo valueType) => new ServiceTypeInfo(ServiceTypeKind.Array, valueType: valueType);

		/// <summary>
		/// Create a map type.
		/// </summary>
		public static ServiceTypeInfo CreateMap(ServiceTypeInfo valueType) => new ServiceTypeInfo(ServiceTypeKind.Map, valueType: valueType);

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
				ServiceTypeKind.Result => $"result<{ValueType}>",
				ServiceTypeKind.Array => $"{ValueType}[]",
				ServiceTypeKind.Map => $"map<{ValueType}>",
				_ => s_primitives.Where(x => x.Kind == Kind).Select(x => x.Name).Single(),
			};
		}

		internal static ServiceTypeInfo? TryParse(string text, Func<string, ServiceMemberInfo?> findMember)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));

			var primitive = s_primitives.FirstOrDefault(x => text == x.Name);
			if (primitive.Name != null)
				return CreatePrimitive(primitive.Kind);

			var resultValueType = TryPrefixSuffix(text, "result<", ">");
			if (resultValueType != null)
			{
				var valueType = TryParse(resultValueType, findMember);
				return valueType == null ? null : CreateResult(valueType);
			}

			var arrayValueType = TryPrefixSuffix(text, "", "[]");
			if (arrayValueType != null)
			{
				var valueType = TryParse(arrayValueType, findMember);
				return valueType == null ? null : CreateArray(valueType);
			}

			var mapValueType = TryPrefixSuffix(text, "map<", ">");
			if (mapValueType != null)
			{
				var valueType = TryParse(mapValueType, findMember);
				return valueType == null ? null : CreateMap(valueType);
			}

			var member = findMember(text);
			if (member != null)
			{
				if (member is ServiceDtoInfo dto)
					return CreateDto(dto);

				if (member is ServiceEnumInfo @enum)
					return CreateEnum(@enum);
			}

			return null;
		}

		private ServiceTypeInfo(ServiceTypeKind kind, ServiceDtoInfo? dto = null, ServiceEnumInfo? @enum = null, ServiceTypeInfo? valueType = null)
		{
			Kind = kind;
			Dto = dto;
			Enum = @enum;
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
		};
	}
}

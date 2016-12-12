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
		public static ServiceTypeInfo CreateDto(ServiceDtoInfo dto)
		{
			return new ServiceTypeInfo(ServiceTypeKind.Dto, dto: dto);
		}

		/// <summary>
		/// Create an enumerated type.
		/// </summary>
		public static ServiceTypeInfo CreateEnum(ServiceEnumInfo @enum)
		{
			return new ServiceTypeInfo(ServiceTypeKind.Enum, @enum: @enum);
		}

		/// <summary>
		/// Create a service result type.
		/// </summary>
		public static ServiceTypeInfo CreateResult(ServiceTypeInfo valueType, NamedTextPosition position)
		{
			return new ServiceTypeInfo(ServiceTypeKind.Result, valueType: valueType);
		}

		/// <summary>
		/// Create an array type.
		/// </summary>
		public static ServiceTypeInfo CreateArray(ServiceTypeInfo valueType, NamedTextPosition position)
		{
			return new ServiceTypeInfo(ServiceTypeKind.Array, valueType: valueType);
		}

		/// <summary>
		/// Create a map type.
		/// </summary>
		public static ServiceTypeInfo CreateMap(ServiceTypeInfo valueType, NamedTextPosition position)
		{
			return new ServiceTypeInfo(ServiceTypeKind.Map, valueType: valueType);
		}

		/// <summary>
		/// The kind of type.
		/// </summary>
		public ServiceTypeKind Kind { get; }

		/// <summary>
		/// The DTO (when Kind is Dto).
		/// </summary>
		public ServiceDtoInfo Dto { get; }

		/// <summary>
		/// The enumerated type (when Kind is Enum).
		/// </summary>
		public ServiceEnumInfo Enum { get; }

		/// <summary>
		/// The value type (when Kind is Result, Array, or Map).
		/// </summary>
		public ServiceTypeInfo ValueType { get; }

		/// <summary>
		/// The string form of the service type.
		/// </summary>
		public override string ToString()
		{
			switch (Kind)
			{
			case ServiceTypeKind.Dto:
				return Dto.Name;
			case ServiceTypeKind.Enum:
				return Enum.Name;
			case ServiceTypeKind.Result:
				return $"result<{ValueType}>";
			case ServiceTypeKind.Array:
				return $"{ValueType}[]";
			case ServiceTypeKind.Map:
				return $"map<{ValueType}>";
			default:
				return s_primitiveTuples.Where(x => x.Item1 == Kind).Select(x => x.Item2).Single();
			}
		}

		internal static ServiceTypeInfo Parse(string text, Func<string, IServiceMemberInfo> findMember, NamedTextPosition position = null)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));

			var primitiveTuple = s_primitiveTuples.FirstOrDefault(x => text == x.Item2);
			if (primitiveTuple != null)
				return CreatePrimitive(primitiveTuple.Item1);

			string resultValueType = TryPrefixSuffix(text, "result<", ">");
			if (resultValueType != null)
				return CreateResult(Parse(resultValueType, findMember, position), position);

			string arrayValueType = TryPrefixSuffix(text, "", "[]");
			if (arrayValueType != null)
				return CreateArray(Parse(arrayValueType, findMember, position), position);

			string mapValueType = TryPrefixSuffix(text, "map<", ">");
			if (mapValueType != null)
				return CreateMap(Parse(mapValueType, findMember, position), position);

			if (findMember != null)
			{
				var member = findMember(text);

				var dto = member as ServiceDtoInfo;
				if (dto != null)
					return CreateDto(dto);

				var @enum = member as ServiceEnumInfo;
				if (@enum != null)
					return CreateEnum(@enum);
			}

			throw new ServiceDefinitionException($"Unknown field type '{text}'.", position);
		}

		private ServiceTypeInfo(ServiceTypeKind kind, ServiceDtoInfo dto = null, ServiceEnumInfo @enum = null, ServiceTypeInfo valueType = null)
		{
			Kind = kind;
			Dto = dto;
			Enum = @enum;
			ValueType = valueType;
		}

		private static string TryPrefixSuffix(string text, string prefix, string suffix)
		{
			return text.StartsWith(prefix, StringComparison.Ordinal) && text.EndsWith(suffix, StringComparison.Ordinal) ?
				text.Substring(prefix.Length, text.Length - prefix.Length - suffix.Length) : null;
		}

		static readonly Tuple<ServiceTypeKind, string>[] s_primitiveTuples =
		{
			Tuple.Create(ServiceTypeKind.String, "string"),
			Tuple.Create(ServiceTypeKind.Boolean, "boolean"),
			Tuple.Create(ServiceTypeKind.Double, "double"),
			Tuple.Create(ServiceTypeKind.Int32, "int32"),
			Tuple.Create(ServiceTypeKind.Int64, "int64"),
			Tuple.Create(ServiceTypeKind.Bytes, "bytes"),
			Tuple.Create(ServiceTypeKind.Object, "object"),
			Tuple.Create(ServiceTypeKind.Error, "error"),
		};
	}
}

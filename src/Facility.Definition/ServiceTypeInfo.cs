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

		internal static ServiceTypeInfo Parse(string text, Func<string, IServiceMemberInfo> findMember, ServiceTextPosition position = null)
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

		private static ServiceTypeInfo CreatePrimitive(ServiceTypeKind kind)
		{
			if (kind == ServiceTypeKind.Dto || kind == ServiceTypeKind.Enum || kind == ServiceTypeKind.Result || kind == ServiceTypeKind.Array || kind == ServiceTypeKind.Map)
				throw new ArgumentOutOfRangeException(nameof(kind), "Kind must be primitive.");
			return new ServiceTypeInfo(kind);
		}

		private static ServiceTypeInfo CreateDto(ServiceDtoInfo dto)
		{
			return new ServiceTypeInfo(ServiceTypeKind.Dto, dto: dto);
		}

		private static ServiceTypeInfo CreateEnum(ServiceEnumInfo @enum)
		{
			return new ServiceTypeInfo(ServiceTypeKind.Enum, @enum: @enum);
		}

		private static ServiceTypeInfo CreateResult(ServiceTypeInfo valueType, ServiceTextPosition position)
		{
			if (valueType.Kind != ServiceTypeKind.Dto)
				throw new ServiceDefinitionException($"Service result value type '{valueType}' is not a DTO.", position);

			return new ServiceTypeInfo(ServiceTypeKind.Result, valueType: valueType);
		}

		private static ServiceTypeInfo CreateArray(ServiceTypeInfo valueType, ServiceTextPosition position)
		{
			if (valueType.Kind == ServiceTypeKind.Array || valueType.Kind == ServiceTypeKind.Map)
				throw new ServiceDefinitionException($"Array value type '{valueType}' must not be an array or map.", position);

			return new ServiceTypeInfo(ServiceTypeKind.Array, valueType: valueType);
		}

		private static ServiceTypeInfo CreateMap(ServiceTypeInfo valueType, ServiceTextPosition position = null)
		{
			if (valueType.Kind == ServiceTypeKind.Array || valueType.Kind == ServiceTypeKind.Map)
				throw new ServiceDefinitionException($"Map value type '{valueType}' must not be an array or map.", position);

			return new ServiceTypeInfo(ServiceTypeKind.Map, valueType: valueType);
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

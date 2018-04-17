using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Facility.Definition.Swagger
{
	/// <summary>
	/// Helpers for Swagger (OpenAPI) 2.0.
	/// </summary>
	public static class SwaggerUtility
	{
		/// <summary>
		/// The Swagger version.
		/// </summary>
		public static readonly string SwaggerVersion = "2.0";

		/// <summary>
		/// JSON serializer settings for Swagger DTOs.
		/// </summary>
		public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCaseExceptDictionaryKeysContractResolver(),
			DateParseHandling = DateParseHandling.None,
			NullValueHandling = NullValueHandling.Ignore,
			MissingMemberHandling = MissingMemberHandling.Ignore,
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
		};

		internal static IReadOnlyList<T> EmptyIfNull<T>(this IReadOnlyList<T> list) => list ?? new T[0];

		internal static IList<T> EmptyIfNull<T>(this IList<T> list) => list ?? new T[0];

		internal static IReadOnlyDictionary<TKey, TValue> EmptyIfNull<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> list) => list ?? new Dictionary<TKey, TValue>();

		internal static IDictionary<TKey, TValue> EmptyIfNull<TKey, TValue>(this IDictionary<TKey, TValue> list) => list ?? new Dictionary<TKey, TValue>();

		private sealed class CamelCaseExceptDictionaryKeysContractResolver : CamelCasePropertyNamesContractResolver
		{
			protected override string ResolveDictionaryKey(string dictionaryKey) => dictionaryKey;
		}
	}
}

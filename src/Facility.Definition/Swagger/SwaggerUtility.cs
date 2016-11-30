using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Facility.Definition.Swagger
{
	/// <summary>
	/// Helpers for Swagger (Open API 2.0).
	/// </summary>
	public static class SwaggerUtility
	{
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

		private sealed class CamelCaseExceptDictionaryKeysContractResolver : CamelCasePropertyNamesContractResolver
		{
			protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
			{
				JsonDictionaryContract contract = base.CreateDictionaryContract(objectType);
				contract.PropertyNameResolver = propertyName => propertyName;
				return contract;
			}
		}
	}
}

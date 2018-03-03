using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;

#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public class SwaggerResponse
	{
		[JsonProperty("$ref")]
		[YamlMember(Alias = "$ref")]
		public string Ref { get; set; }

		public string Description { get; set; }

		public SwaggerSchema Schema { get; set; }

		public IDictionary<string, SwaggerSchema> Headers { get; set; }

		public JObject Examples { get; set; }

		[JsonProperty("x-identifier")]
		[YamlMember(Alias = "x-identifier")]
		public string Identifier { get; set; }
	}
}

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public class SwaggerResponse
	{
		[JsonProperty("$ref")]
		public string Ref { get; set; }

		public string Description { get; set; }

		public SwaggerSchema Schema { get; set; }

		public IReadOnlyDictionary<string, SwaggerSchema> Headers { get; set; }

		public JObject Examples { get; set; }

		[JsonProperty("x-identifier")]
		public string Identifier { get; set; }
	}
}

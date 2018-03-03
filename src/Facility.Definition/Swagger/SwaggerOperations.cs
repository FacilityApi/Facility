using System.Collections.Generic;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public class SwaggerOperations
	{
		[JsonProperty("$ref")]
		[YamlMember(Alias = "$ref")]
		public string Ref { get; set; }

		public SwaggerOperation Get { get; set; }

		public SwaggerOperation Post { get; set; }

		public SwaggerOperation Put { get; set; }

		public SwaggerOperation Delete { get; set; }

		public SwaggerOperation Options { get; set; }

		public SwaggerOperation Head { get; set; }

		public SwaggerOperation Patch { get; set; }

		public IList<SwaggerParameter> Parameters { get; set; }
	}
}

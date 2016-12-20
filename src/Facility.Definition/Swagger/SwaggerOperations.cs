using System.Collections.Generic;
using Newtonsoft.Json;
#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public class SwaggerOperations
	{
		[JsonProperty("$ref")]
		public string Ref { get; set; }

		public SwaggerOperation Get { get; set; }

		public SwaggerOperation Post { get; set; }

		public SwaggerOperation Put { get; set; }

		public SwaggerOperation Delete { get; set; }

		public SwaggerOperation Options { get; set; }

		public SwaggerOperation Head { get; set; }

		public SwaggerOperation Patch { get; set; }

		public IReadOnlyList<SwaggerParameter> Parameters { get; set; }
	}
}

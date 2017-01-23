using System.Collections.Generic;
#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public class SwaggerSecurityScheme
	{
		public string Type { get; set; }

		public string Description { get; set; }

		public string Name { get; set; }

		public string In { get; set; }

		public string Flow { get; set; }

		public string AuthorizationUrl { get; set; }

		public string TokenUrl { get; set; }

		public IDictionary<string, string> Scopes { get; set; }
	}
}

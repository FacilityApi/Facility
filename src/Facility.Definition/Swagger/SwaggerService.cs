using System.Collections.Generic;
#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public class SwaggerService
	{
		public string Swagger { get; set; }

		public SwaggerInfo Info { get; set; }

		public string Host { get; set; }

		public string BasePath { get; set; }

		public IList<string> Schemes { get; set; }

		public IList<string> Consumes { get; set; }

		public IList<string> Produces { get; set; }

		public IDictionary<string, SwaggerOperations> Paths { get; set; }

		public IDictionary<string, SwaggerSchema> Definitions { get; set; }

		public IDictionary<string, SwaggerParameter> Parameters { get; set; }

		public IDictionary<string, SwaggerResponse> Responses { get; set; }

		public IDictionary<string, SwaggerSecurityScheme> SecurityDefinitions { get; set; }

		public IList<IDictionary<string, IList<string>>> Security { get; set; }

		public IList<SwaggerTag> Tags { get; set; }

		public SwaggerExternalDocumentation ExternalDocs { get; set; }
	}
}

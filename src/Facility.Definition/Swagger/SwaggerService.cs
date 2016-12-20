using System.Collections.Generic;
#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public class SwaggerService
	{
		public SwaggerService()
		{
			Swagger = "2.0";
		}

		public string Swagger { get; set; }

		public SwaggerInfo Info { get; set; }

		public string Host { get; set; }

		public string BasePath { get; set; }

		public IReadOnlyList<string> Schemes { get; set; }

		public IReadOnlyList<string> Consumes { get; set; }

		public IReadOnlyList<string> Produces { get; set; }

		public IReadOnlyDictionary<string, SwaggerOperations> Paths { get; set; }

		public IReadOnlyDictionary<string, SwaggerSchema> Definitions { get; set; }

		public IReadOnlyDictionary<string, SwaggerParameter> Parameters { get; set; }

		public IReadOnlyDictionary<string, SwaggerResponse> Responses { get; set; }

		public IReadOnlyDictionary<string, SwaggerSecurityScheme> SecurityDefinitions { get; set; }

		public IReadOnlyList<IReadOnlyDictionary<string, IReadOnlyList<string>>> Security { get; set; }

		public IReadOnlyList<SwaggerTag> Tags { get; set; }

		public SwaggerExternalDocumentation ExternalDocs { get; set; }
	}
}

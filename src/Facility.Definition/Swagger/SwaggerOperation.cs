using System.Collections.Generic;
#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public class SwaggerOperation
	{
		public IReadOnlyList<string> Tags { get; set; }

		public string Summary { get; set; }

		public string Description { get; set; }

		public SwaggerExternalDocumentation ExternalDocs { get; set; }

		public string OperationId { get; set; }

		public IReadOnlyList<string> Consumes { get; set; }

		public IReadOnlyList<string> Produces { get; set; }

		public IReadOnlyList<SwaggerParameter> Parameters { get; set; }

		public IReadOnlyDictionary<string, SwaggerResponse> Responses { get; set; }

		public IReadOnlyList<string> Schemes { get; set; }

		public bool? Deprecated { get; set; }

		public IReadOnlyList<IReadOnlyDictionary<string, IReadOnlyList<string>>> Security { get; set; }
	}
}

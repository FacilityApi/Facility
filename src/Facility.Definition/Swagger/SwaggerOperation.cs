using System.Collections.Generic;
#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public class SwaggerOperation
	{
		public IList<string> Tags { get; set; }

		public string Summary { get; set; }

		public string Description { get; set; }

		public SwaggerExternalDocumentation ExternalDocs { get; set; }

		public string OperationId { get; set; }

		public IList<string> Consumes { get; set; }

		public IList<string> Produces { get; set; }

		public IList<SwaggerParameter> Parameters { get; set; }

		public IDictionary<string, SwaggerResponse> Responses { get; set; }

		public IList<string> Schemes { get; set; }

		public bool? Deprecated { get; set; }

		public IList<IDictionary<string, IList<string>>> Security { get; set; }
	}
}

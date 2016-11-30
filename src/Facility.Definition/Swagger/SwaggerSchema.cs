using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public class SwaggerSchema
	{
		[JsonProperty("$ref")]
		public string Ref { get; set; } // parameters, schema

		public string In { get; set; } // parameters

		public string Name { get; set; } // parameters

		public string Description { get; set; } // parameters, headers, schema

		public bool? Required { get; set; } // parameters, schema

		public SwaggerSchema Schema { get; set; } // parameters (body)

		public string Title { get; set; } // schema

		public string Type { get; set; } // parameters (non-body), headers, schema

		public string Format { get; set; } // parameters (non-body), headers, schema

		public bool? AllowEmptyValue { get; set; } // parameters (non-body)

		public SwaggerSchema Items { get; set; } // parameters (non-body), headers, schema

		public int? MaxProperties { get; set; } // schema

		public int? MinProperties { get; set; } // schema

		public string CollectionFormat { get; set; } // parameters (non-body), headers

		public JToken Default { get; set; } // parameters (non-body), headers, schema

		public double? Maximum { get; set; } // parameters (non-body), headers, schema

		public bool? ExclusiveMaximum { get; set; } // parameters (non-body), headers, schema

		public double? Minimum { get; set; } // parameters (non-body), headers, schema

		public bool? ExclusiveMinimum { get; set; } // parameters (non-body), headers, schema

		public int? MaxLength { get; set; } // parameters (non-body), headers, schema

		public int? MinLength { get; set; } // parameters (non-body), headers, schema

		public string Pattern { get; set; } // parameters (non-body), headers, schema

		public int? MaxItems { get; set; } // parameters (non-body), headers, schema

		public int? MinItems { get; set; } // parameters (non-body), headers, schema

		public bool? UniqueItems { get; set; } // parameters (non-body), headers, schema

		public IReadOnlyList<JToken> Enum { get; set; } // parameters (non-body), headers, schema

		public double? MultipleOf { get; set; } // parameters (non-body), headers, schema

		public IReadOnlyDictionary<string, SwaggerSchema> Properties { get; set; } // schema

		public IReadOnlyList<SwaggerSchema> AllOf { get; set; } // schema

		public SwaggerSchema AdditionalProperties { get; set; } // schema

		public string Discriminator { get; set; } // schema

		public bool? ReadOnly { get; set; } // schema

		public JObject Xml { get; set; } // schema

		public SwaggerExternalDocumentation ExternalDocs { get; set; }

		public JToken Example { get; set; } // schema
	}
}

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;

#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public class SwaggerSchema : ISwaggerSchema
	{
		[JsonProperty("$ref")]
		[YamlMember(Alias = "$ref")]
		public string Ref { get; set; } // parameters, schema

		public string Description { get; set; } // parameters, headers, schema

		public IList<string> Required { get; set; } // schema

		public string Title { get; set; } // schema

		public string Type { get; set; } // parameters (non-body), headers, schema

		public string Format { get; set; } // parameters (non-body), headers, schema

		public SwaggerSchema Items { get; set; } // parameters (non-body), headers, schema

		public int? MaxProperties { get; set; } // schema

		public int? MinProperties { get; set; } // schema

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

		public IList<JToken> Enum { get; set; } // parameters (non-body), headers, schema

		public double? MultipleOf { get; set; } // parameters (non-body), headers, schema

		public IDictionary<string, SwaggerSchema> Properties { get; set; } // schema

		public IList<SwaggerSchema> AllOf { get; set; } // schema

		public SwaggerSchema AdditionalProperties { get; set; } // schema

		public string Discriminator { get; set; } // schema

		public bool? ReadOnly { get; set; } // schema

		public JObject Xml { get; set; } // schema

		public SwaggerExternalDocumentation ExternalDocs { get; set; } // schema

		public JToken Example { get; set; } // schema

		[JsonProperty("x-identifier")]
		[YamlMember(Alias = "x-identifier")]
		public string Identifier { get; set; } // parameters, headers, schema

		[JsonProperty("x-obsolete")]
		[YamlMember(Alias = "x-obsolete")]
		public bool? Obsolete { get; set; } // parameters, headers, schema

		[JsonProperty("x-remarks")]
		[YamlMember(Alias = "x-remarks")]
		public string Remarks { get; set; } // schema
	}
}

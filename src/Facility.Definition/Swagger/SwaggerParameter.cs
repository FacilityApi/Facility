using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;

#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public class SwaggerParameter : ISwaggerSchema
	{
		[JsonProperty("$ref")]
		[YamlMember(Alias = "$ref")]
		public string Ref { get; set; } // parameters, schema

		public string In { get; set; } // parameters

		public string Name { get; set; } // parameters

		public string Description { get; set; } // parameters, headers, schema

		public bool? Required { get; set; } // parameters

		public SwaggerSchema Schema { get; set; } // parameters (body)

		public string Type { get; set; } // parameters (non-body), headers, schema

		public string Format { get; set; } // parameters (non-body), headers, schema

		public bool? AllowEmptyValue { get; set; } // parameters (non-body)

		public SwaggerSchema Items { get; set; } // parameters (non-body), headers, schema

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

		public IList<JToken> Enum { get; set; } // parameters (non-body), headers, schema

		public double? MultipleOf { get; set; } // parameters (non-body), headers, schema

		[JsonProperty("x-identifier")]
		[YamlMember(Alias = "x-identifier")]
		public string Identifier { get; set; } // parameters, headers, schema

		[JsonProperty("x-obsolete")]
		[YamlMember(Alias = "x-obsolete")]
		public bool? Obsolete { get; set; } // parameters, headers, schema
	}
}

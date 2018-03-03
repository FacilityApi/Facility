using System.Collections.Generic;
using Newtonsoft.Json.Linq;

#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public interface ISwaggerSchema
	{
		string Ref { get; set; } // parameters, schema

		string Description { get; set; } // parameters, headers, schema

		string Type { get; set; } // parameters (non-body), headers, schema

		string Format { get; set; } // parameters (non-body), headers, schema

		SwaggerSchema Items { get; set; } // parameters (non-body), headers, schema

		JToken Default { get; set; } // parameters (non-body), headers, schema

		double? Maximum { get; set; } // parameters (non-body), headers, schema

		bool? ExclusiveMaximum { get; set; } // parameters (non-body), headers, schema

		double? Minimum { get; set; } // parameters (non-body), headers, schema

		bool? ExclusiveMinimum { get; set; } // parameters (non-body), headers, schema

		int? MaxLength { get; set; } // parameters (non-body), headers, schema

		int? MinLength { get; set; } // parameters (non-body), headers, schema

		string Pattern { get; set; } // parameters (non-body), headers, schema

		int? MaxItems { get; set; } // parameters (non-body), headers, schema

		int? MinItems { get; set; } // parameters (non-body), headers, schema

		bool? UniqueItems { get; set; } // parameters (non-body), headers, schema

		IList<JToken> Enum { get; set; } // parameters (non-body), headers, schema

		double? MultipleOf { get; set; } // parameters (non-body), headers, schema

		string Identifier { get; set; } // parameters, headers, schema

		bool? Obsolete { get; set; } // parameters, headers, schema
	}
}

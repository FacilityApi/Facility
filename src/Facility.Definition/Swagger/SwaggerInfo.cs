using Newtonsoft.Json;
using YamlDotNet.Serialization;

#pragma warning disable 1591

namespace Facility.Definition.Swagger
{
	public class SwaggerInfo
	{
		public string Title { get; set; }

		public string Description { get; set; }

		public string TermsOfService { get; set; }

		public SwaggerContact Contact { get; set; }

		public SwaggerLicense License { get; set; }

		public string Version { get; set; }

		[JsonProperty("x-identifier")]
		[YamlMember(Alias = "x-identifier")]
		public string Identifier { get; set; }

		[JsonProperty("x-codegen")]
		[YamlMember(Alias = "x-codegen")]
		public string CodeGen { get; set; }
	}
}

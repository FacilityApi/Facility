using System.Collections.Generic;
using Facility.Console;
using Facility.Definition.CodeGen;
using Facility.Definition.Fsd;
using Facility.Definition.Swagger;

namespace fsdgenfsd
{
	public sealed class FsdGenFsdApp : CodeGeneratorApp
	{
		public static int Main(string[] args)
		{
			return new FsdGenFsdApp().Run(args);
		}

		protected override IReadOnlyList<string> Description => new[]
		{
			"Generates Facility Service Definitions.",
		};

		protected override IReadOnlyList<string> ExtraUsage => new[]
		{
			"   --swagger",
			"      Generates Swagger (OpenAPI) 2.0.",
			"   --yaml",
			"      Generates YAML instead of JSON.",
		};

		protected override CodeGenerator CreateGenerator(ArgsReader args)
		{
			if (args.ReadFlag("swagger"))
				return new SwaggerGenerator { Yaml = args.ReadFlag("yaml") };
			else
				return new FsdGenerator();
		}

		protected override bool SupportsSingleOutput => true;
	}
}

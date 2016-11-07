using System.Collections.Generic;
using Facility.Console;
using Facility.Definition.CodeGen;
using Facility.Definition.Fsd;

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

		protected override CodeGenerator CreateGenerator(ArgsReader args) => new FsdGenerator();

		protected override bool SupportsSingleOutput => true;
	}
}

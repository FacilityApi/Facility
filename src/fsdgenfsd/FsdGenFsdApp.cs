using ArgsReading;
using Facility.CodeGen.Console;
using Facility.Definition.CodeGen;
using Facility.Definition.Fsd;

namespace fsdgenfsd;

public sealed class FsdGenFsdApp : CodeGeneratorApp
{
	public static int Main(string[] args) => new FsdGenFsdApp().Run(args);

	protected override IReadOnlyList<string> Description =>
	[
		"Generates FSD for a Facility Service Definition",
	];

	protected override IReadOnlyList<string> ExtraUsage =>
	[
		"   --file-scoped-service",
		"      Generate file-scoped service.",
	];

	protected override ServiceParser CreateParser() => new FsdParser(new FsdParserSettings { SupportsEvents = true });

	protected override CodeGenerator CreateGenerator() => new FsdGenerator();

	protected override FileGeneratorSettings CreateSettings(ArgsReader args) =>
		new FsdGeneratorSettings
		{
			FileScopedService = args.ReadFlag("file-scoped-service"),
		};
}

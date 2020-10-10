using System;
using System.Collections.Generic;
using System.Reflection;
using ArgsReading;
using Facility.Definition;
using Facility.Definition.CodeGen;
using Facility.Definition.Fsd;

namespace Facility.CodeGen.Console
{
	/// <summary>
	/// Base class for code generator console applications.
	/// </summary>
	public abstract class CodeGeneratorApp
	{
		/// <summary>
		/// Called to execute the code generator application.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		/// <returns>The exit code.</returns>
		public int Run(IReadOnlyList<string> args)
		{
			var generator = CreateGenerator();
			generator.GeneratorName = Assembly.GetEntryAssembly()!.GetName().Name;

			try
			{
				var argsReader = new ArgsReader(args) { LongOptionIgnoreCase = true, LongOptionIgnoreKebabCase = true };
				if (argsReader.ReadHelpFlag())
				{
					foreach (string line in Description)
						System.Console.WriteLine(line);
					System.Console.WriteLine();
					WriteUsage(generator);
					return 0;
				}

				var settings = CreateSettings(argsReader);
				var isVerify = argsReader.ReadVerifyFlag();

				settings.IndentText = generator.RespectsIndentText ? argsReader.ReadIndentOption() : null;
				settings.NewLine = generator.RespectsNewLine ? argsReader.ReadNewLineOption() : null;
				settings.ShouldClean = generator.HasPatternsToClean && argsReader.ReadCleanFlag();
				settings.IsQuiet = argsReader.ReadQuietFlag();
				settings.IsDryRun = isVerify || argsReader.ReadDryRunFlag();
				settings.ExcludeTags = argsReader.ReadExcludeTagOptions();
				settings.IgnoreNewLines = isVerify;

				settings.InputPath = argsReader.ReadArgument();
				if (settings.InputPath == null)
					throw new ArgsReaderException("Missing input path.");

				settings.OutputPath = argsReader.ReadArgument();
				if (settings.OutputPath == null)
					throw new ArgsReaderException("Missing output path.");

				argsReader.VerifyComplete();

				var filesChanged = FileGenerator.GenerateFiles(CreateParser(), generator, settings);

				if (isVerify)
					return filesChanged == 0 ? 0 : 1;

				return 0;
			}
			catch (ServiceDefinitionException exception)
			{
				System.Console.Error.WriteLine(exception);
				foreach (var error in exception.Errors)
					System.Console.Error.WriteLine(error.ToString());
				return 2;
			}
			catch (ArgsReaderException exception)
			{
				System.Console.Error.WriteLine(exception.Message);
				System.Console.Error.WriteLine();
				WriteUsage(generator);
				return 2;
			}
			catch (ApplicationException exception)
			{
				System.Console.Error.WriteLine(exception.Message);
				return 2;
			}
			catch (Exception exception)
			{
				System.Console.Error.WriteLine(exception.ToString());
				return 3;
			}
		}

		/// <summary>
		/// The app description lines for help.
		/// </summary>
		protected abstract IReadOnlyList<string> Description { get; }

		/// <summary>
		/// Any extra usage lines for help.
		/// </summary>
		protected virtual IReadOnlyList<string> ExtraUsage => Array.Empty<string>();

		/// <summary>
		/// Creates the code generator.
		/// </summary>
		protected abstract CodeGenerator CreateGenerator();

		/// <summary>
		/// Creates the file generator settings.
		/// </summary>
		/// <param name="args">Used to support extra arguments.</param>
		/// <returns>The file generator settings.</returns>
		protected abstract FileGeneratorSettings CreateSettings(ArgsReader args);

		/// <summary>
		/// Creates the service parser.
		/// </summary>
		protected virtual ServiceParser CreateParser() => new FsdParser();

		private void WriteUsage(CodeGenerator generator)
		{
			System.Console.WriteLine($"Usage: {generator.GeneratorName} input output [options]");
			System.Console.WriteLine();
			System.Console.WriteLine("   input");
			System.Console.WriteLine("      The path to the input file (- for stdin).");
			System.Console.WriteLine("   output");
			System.Console.WriteLine("      The path to the output directory" + (generator.SupportsSingleOutput ? " or file (- for stdout)." : "."));
			System.Console.WriteLine();

			foreach (var usage in ExtraUsage)
				System.Console.WriteLine(usage);

			if (generator.HasPatternsToClean)
			{
				System.Console.WriteLine("   --clean");
				System.Console.WriteLine("      Deletes previously generated files that are no longer used.");
			}

			if (generator.RespectsIndentText)
			{
				System.Console.WriteLine("   --indent (tab|1|2|3|4|5|6|7|8)");
				System.Console.WriteLine("      The indent used in the output: a tab or a number of spaces.");
			}

			if (generator.RespectsNewLine)
			{
				System.Console.WriteLine("   --newline (auto|lf|crlf)");
				System.Console.WriteLine("      The newline used in the output.");
			}

			System.Console.WriteLine("   --exclude-tag <tag>");
			System.Console.WriteLine("      Excludes service elements with the specified tag.");
			System.Console.WriteLine("   --dry-run");
			System.Console.WriteLine("      Executes the tool without making changes to the file system.");
			System.Console.WriteLine("   --verify");
			System.Console.WriteLine("      Exits with error code 1 if changes to the file system are needed.");
			System.Console.WriteLine("   --quiet");
			System.Console.WriteLine("      Suppresses normal console output.");
		}
	}
}

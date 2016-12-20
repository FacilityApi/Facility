using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Facility.Definition;
using Facility.Definition.CodeGen;
using Facility.Definition.Fsd;
using Facility.Definition.Swagger;

namespace Facility.Console
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
			try
			{
				var argsReader = new ArgsReader(args);
				if (argsReader.ReadHelpFlag())
				{
					foreach (string line in Description)
						System.Console.WriteLine(line);
					System.Console.WriteLine();
					WriteUsage();
					return 0;
				}

				var generator = CreateGenerator(argsReader);
				generator.GeneratorName = s_assemblyName;
				if (SupportsCustomIndent)
					generator.IndentText = argsReader.ReadIndentOption();
				if (SupportsCustomNewLine)
					generator.NewLine = argsReader.ReadNewLineOption();

				string serviceName = argsReader.ReadServiceNameOption();
				bool shouldClean = SupportsClean && argsReader.ReadCleanFlag();
				bool isQuiet = argsReader.ReadQuietFlag();
				bool isVerify = argsReader.ReadVerifyFlag();
				bool isDryRun = argsReader.ReadDryRunFlag();

				string inputPath = argsReader.ReadArgument();
				if (inputPath == null)
					throw new ArgsReaderException("Missing input path.");

				string outputPath = argsReader.ReadArgument();
				if (outputPath == null)
					throw new ArgsReaderException("Missing output path.");

				argsReader.VerifyComplete();

				NamedText input;
				if (inputPath == "-")
				{
					input = new NamedText("", System.Console.In.ReadToEnd());
				}
				else
				{
					if (!File.Exists(inputPath))
						throw new ApplicationException("Input file does not exist: " + inputPath);
					input = new NamedText(Path.GetFileName(inputPath), File.ReadAllText(inputPath));
				}

				ServiceInfo service;
				if (ServiceDefinitionUtility.DetectFormat(input) == ServiceDefinitionFormat.Swagger)
				{
					service = new SwaggerParser { ServiceName = serviceName }.ParseDefinition(input);
				}
				else
				{
					if (serviceName != null)
						throw new ArgsReaderException("--serviceName not supported for FSD input.");
					service = new FsdParser().ParseDefinition(input);
				}

				PrepareGenerator(generator, service, outputPath);
				var output = generator.GenerateOutput(service);

				if (SupportsSingleOutput &&
					!outputPath.EndsWith("/", StringComparison.Ordinal) &&
					!outputPath.EndsWith("\\", StringComparison.Ordinal) &&
					!Directory.Exists(outputPath))
				{
					if (output.NamedTexts.Count > 1)
						throw new InvalidOperationException("Multiple outputs not expected.");

					if (output.NamedTexts.Count == 1)
					{
						var namedText = output.NamedTexts[0];

						if (outputPath == "-")
							System.Console.Write(namedText.Text);
						else if (ShouldWriteByteOrderMark(namedText.Name))
							File.WriteAllText(outputPath, namedText.Text, s_utf8WithBom);
						else
							File.WriteAllText(outputPath, namedText.Text);
					}
				}
				else
				{
					var namedTextsToWrite = new List<NamedText>();
					foreach (var namedText in output.NamedTexts)
					{
						string existingFilePath = Path.Combine(outputPath, namedText.Name);
						if (File.Exists(existingFilePath))
						{
							// ignore CR when comparing files
							if (namedText.Text.Replace("\r", "") != File.ReadAllText(existingFilePath).Replace("\r", ""))
							{
								namedTextsToWrite.Add(namedText);
								if (!isQuiet)
									System.Console.WriteLine("changed " + namedText.Name);
							}
						}
						else
						{
							namedTextsToWrite.Add(namedText);
							if (!isQuiet)
								System.Console.WriteLine("added " + namedText.Name);
						}
					}

					var namesToDelete = new List<string>();
					if (shouldClean && output.PatternsToClean.Count != 0)
					{
						var directoryInfo = new DirectoryInfo(outputPath);
						if (directoryInfo.Exists)
						{
							foreach (string nameMatchingPattern in FindNamesMatchingPatterns(directoryInfo, output.PatternsToClean))
							{
								if (output.NamedTexts.All(x => x.Name != nameMatchingPattern))
								{
									namesToDelete.Add(nameMatchingPattern);
									if (!isQuiet)
										System.Console.WriteLine("removed " + nameMatchingPattern);
								}
							}
						}
					}

					if (isVerify)
						return namedTextsToWrite.Count != 0 || namesToDelete.Count != 0 ? 1 : 0;

					if (!isDryRun)
					{
						if (!Directory.Exists(outputPath))
							Directory.CreateDirectory(outputPath);

						foreach (var namedText in namedTextsToWrite)
						{
							string outputFilePath = Path.Combine(outputPath, namedText.Name);

							string outputFileDirectoryPath = Path.GetDirectoryName(outputFilePath);
							if (outputFileDirectoryPath != null && outputFileDirectoryPath != outputPath && !Directory.Exists(outputFileDirectoryPath))
								Directory.CreateDirectory(outputFileDirectoryPath);

							if (ShouldWriteByteOrderMark(namedText.Name))
								File.WriteAllText(outputFilePath, namedText.Text, s_utf8WithBom);
							else
								File.WriteAllText(outputFilePath, namedText.Text);
						}

						foreach (string nameToDelete in namesToDelete)
							File.Delete(Path.Combine(outputPath, nameToDelete));
					}
				}

				return 0;
			}
			catch (Exception exception)
			{
				if (exception is ApplicationException || exception is ArgsReaderException || exception is ServiceDefinitionException)
				{
					System.Console.Error.WriteLine(exception.Message);
					if (exception is ArgsReaderException)
					{
						System.Console.Error.WriteLine();
						WriteUsage();
					}
					return 2;
				}
				else
				{
					System.Console.Error.WriteLine(exception.ToString());
					return 3;
				}
			}
		}

		/// <summary>
		/// The app description lines for help.
		/// </summary>
		protected abstract IReadOnlyList<string> Description { get; }

		/// <summary>
		/// Any extra usage lines for help.
		/// </summary>
		protected virtual IReadOnlyList<string> ExtraUsage => new string[0];

		/// <summary>
		/// True if the application supports output to a file and/or standard output. (Default false.)
		/// </summary>
		protected virtual bool SupportsSingleOutput => false;

		/// <summary>
		/// True if the application supports the clean option. (Default false.)
		/// </summary>
		protected virtual bool SupportsClean => false;

		/// <summary>
		/// True if the application supports the custom indent option. (Default true.)
		/// </summary>
		protected virtual bool SupportsCustomIndent => true;

		/// <summary>
		/// True if the application supports the custom new line option. (Default true.)
		/// </summary>
		protected virtual bool SupportsCustomNewLine => true;

		/// <summary>
		/// Creates the code generator.
		/// </summary>
		protected abstract CodeGenerator CreateGenerator(ArgsReader args);

		/// <summary>
		/// Prepares the code generator.
		/// </summary>
		protected virtual void PrepareGenerator(CodeGenerator generator, ServiceInfo service, string outputPath)
		{
		}

		/// <summary>
		/// True if a BOM should be written for a file with the specified name.
		/// </summary>
		protected virtual bool ShouldWriteByteOrderMark(string name) => false;

		private IEnumerable<string> FindNamesMatchingPatterns(DirectoryInfo directoryInfo, IReadOnlyList<CodeGenPattern> patternsToClean)
		{
			foreach (var patternToClean in patternsToClean)
			{
				foreach (string name in FindNamesMatchingPatterns(directoryInfo, patternToClean))
					yield return name;
			}
		}

		private IEnumerable<string> FindNamesMatchingPatterns(DirectoryInfo directoryInfo, CodeGenPattern patternToClean)
		{
			var parts = patternToClean.NamePattern.Split(new[] { '/' }, 2);
			if (parts[0].Length == 0)
				throw new InvalidOperationException("Invalid name pattern.");

			if (parts.Length == 1)
			{
				foreach (var fileInfo in directoryInfo.GetFiles(parts[0]))
				{
					if (File.ReadAllText(fileInfo.FullName).Contains(patternToClean.RequiredSubstring))
						yield return fileInfo.Name;
				}
			}
			else
			{
				foreach (var subdirectoryInfo in directoryInfo.GetDirectories(parts[0]))
				{
					foreach (string name in FindNamesMatchingPatterns(subdirectoryInfo, new CodeGenPattern(parts[1], patternToClean.RequiredSubstring)))
						yield return parts[0] + '/' + name;
				}
			}
		}

		private void WriteUsage()
		{
			System.Console.WriteLine($"Usage: {s_assemblyName} input output [options]");
			System.Console.WriteLine();
			System.Console.WriteLine("   input");
			System.Console.WriteLine("      The path to the input file (- for stdin).");
			System.Console.WriteLine("   output");
			System.Console.WriteLine("      The path to the output directory" + (SupportsSingleOutput ? " or file (- for stdout)." : "."));
			System.Console.WriteLine();

			foreach (var usage in ExtraUsage)
				System.Console.WriteLine(usage);

			System.Console.WriteLine("   --serviceName <name>");
			System.Console.WriteLine("      The name of the input service (for non-FSD input).");

			if (SupportsClean)
			{
				System.Console.WriteLine("   --clean");
				System.Console.WriteLine("      Deletes previously generated files that are no longer used.");
			}

			if (SupportsCustomIndent)
			{
				System.Console.WriteLine("   --indent (tab|1|2|3|4|5|6|7|8)");
				System.Console.WriteLine("      The indent used in the output: a tab or a number of spaces.");
			}

			if (SupportsCustomNewLine)
			{
				System.Console.WriteLine("   --newline (auto|lf|crlf)");
				System.Console.WriteLine("      The newline used in the output.");
			}

			System.Console.WriteLine("   --dryrun");
			System.Console.WriteLine("      Executes the tool without making changes to the file system.");
			System.Console.WriteLine("   --verify");
			System.Console.WriteLine("      Exits with error code 1 if changes to the file system are needed.");
			System.Console.WriteLine("   --quiet");
			System.Console.WriteLine("      Suppresses normal console output.");
		}

		static readonly string s_assemblyName = Assembly.GetEntryAssembly().GetName().Name;
		static readonly Encoding s_utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
	}
}

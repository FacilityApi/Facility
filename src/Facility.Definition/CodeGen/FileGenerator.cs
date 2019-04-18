using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Facility.Definition.Fsd;

namespace Facility.Definition.CodeGen
{
	/// <summary>
	/// Used to parse input files and generate output files.
	/// </summary>
	public static class FileGenerator
	{
		/// <summary>
		/// Parses input files and generates output files.
		/// </summary>
		/// <param name="generator">The code generator.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The number of updated files.</returns>
		public static int GenerateFiles(CodeGenerator generator, FileGeneratorSettings settings) => GenerateFiles(new FsdParser(), generator, settings);

		/// <summary>
		/// Parses input files and generates output files.
		/// </summary>
		/// <param name="parser">The service parser.</param>
		/// <param name="generator">The code generator.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The number of updated files.</returns>
		public static int GenerateFiles(ServiceParser parser, CodeGenerator generator, FileGeneratorSettings settings)
		{
			if (parser == null)
				throw new ArgumentNullException(nameof(parser));
			if (generator == null)
				throw new ArgumentNullException(nameof(generator));

			if (settings.IndentText != null)
			{
				if (!generator.RespectsIndentText)
					throw new ArgumentException("Generator does not support IndentText setting.");
				generator.IndentText = settings.IndentText;
			}

			if (settings.NewLine != null)
			{
				if (!generator.RespectsNewLine)
					throw new ArgumentException("Generator does not support NewLine setting.");
				generator.NewLine = settings.NewLine;
			}

			generator.ApplySettings(settings);

			bool shouldClean = settings.ShouldClean;
			if (shouldClean && !generator.HasPatternsToClean)
				throw new ArgumentException("Generator does not support ShouldClean setting.");

			ServiceDefinitionText input;
			if (settings.InputPath == "-")
			{
				input = new ServiceDefinitionText("", Console.In.ReadToEnd());
			}
			else
			{
				if (!File.Exists(settings.InputPath))
					throw new ApplicationException("Input file does not exist: " + settings.InputPath);
				input = new ServiceDefinitionText(Path.GetFileName(settings.InputPath), File.ReadAllText(settings.InputPath));
			}

			var service = parser.ParseDefinition(input);

			foreach (string excludeTag in settings.ExcludeTags)
				service = service.ExcludeTag(excludeTag);

			var output = generator.GenerateOutput(service);

			var filesToWrite = new List<CodeGenFile>();
			var namesToDelete = new List<string>();
			bool outputIsFile = false;
			bool writeToConsole = false;

			if (generator.HasSingleOutput &&
				!settings.OutputPath.EndsWith("/", StringComparison.Ordinal) &&
				!settings.OutputPath.EndsWith("\\", StringComparison.Ordinal) &&
				!Directory.Exists(settings.OutputPath))
			{
				if (output.Files.Count > 1)
					throw new InvalidOperationException("Multiple outputs not expected.");

				outputIsFile = true;
				writeToConsole = settings.OutputPath == "-";
			}

			bool notQuiet = !settings.IsQuiet && !outputIsFile;

			foreach (var file in output.Files)
			{
				string existingFilePath = outputIsFile ? settings.OutputPath : Path.Combine(settings.OutputPath, file.Name);
				if (File.Exists(existingFilePath))
				{
					// ignore CR when comparing files
					if (file.Text.Replace("\r", "") != File.ReadAllText(existingFilePath).Replace("\r", ""))
					{
						filesToWrite.Add(file);
						if (notQuiet)
							Console.WriteLine("changed " + file.Name);
					}
				}
				else
				{
					filesToWrite.Add(file);
					if (notQuiet)
						Console.WriteLine("added " + file.Name);
				}
			}

			if (shouldClean && output.PatternsToClean.Count != 0)
			{
				var directoryInfo = new DirectoryInfo(settings.OutputPath);
				if (directoryInfo.Exists)
				{
					foreach (string nameMatchingPattern in FindNamesMatchingPatterns(directoryInfo, output.PatternsToClean))
					{
						if (output.Files.All(x => x.Name != nameMatchingPattern))
						{
							namesToDelete.Add(nameMatchingPattern);
							if (notQuiet)
								Console.WriteLine("removed " + nameMatchingPattern);
						}
					}
				}
			}

			if (!settings.IsDryRun)
			{
				if (!outputIsFile && !Directory.Exists(settings.OutputPath))
					Directory.CreateDirectory(settings.OutputPath);

				foreach (var fileToWrite in filesToWrite)
				{
					string outputFilePath = outputIsFile ? settings.OutputPath : Path.Combine(settings.OutputPath, fileToWrite.Name);

					string outputFileDirectoryPath = Path.GetDirectoryName(outputFilePath);
					if (outputFileDirectoryPath != null && outputFileDirectoryPath != settings.OutputPath && !Directory.Exists(outputFileDirectoryPath))
						Directory.CreateDirectory(outputFileDirectoryPath);

					if (writeToConsole)
						Console.Write(fileToWrite.Text);
					else
						File.WriteAllText(outputFilePath, fileToWrite.Text);
				}

				foreach (string nameToDelete in namesToDelete)
					File.Delete(Path.Combine(settings.OutputPath, nameToDelete));
			}

			return filesToWrite.Count + namesToDelete.Count;
		}

		private static IEnumerable<string> FindNamesMatchingPatterns(DirectoryInfo directoryInfo, IReadOnlyList<CodeGenPattern> patternsToClean)
		{
			foreach (var patternToClean in patternsToClean)
			{
				foreach (string name in FindNamesMatchingPattern(directoryInfo, patternToClean))
					yield return name;
			}
		}

		private static IEnumerable<string> FindNamesMatchingPattern(DirectoryInfo directoryInfo, CodeGenPattern patternToClean)
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
					foreach (string name in FindNamesMatchingPattern(subdirectoryInfo, new CodeGenPattern(parts[1], patternToClean.RequiredSubstring)))
						yield return subdirectoryInfo.Name + '/' + name;
				}
			}
		}
	}
}

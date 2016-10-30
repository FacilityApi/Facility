using System;
using System.Collections.Generic;
using System.IO;
using Facility.Definition;
using Facility.Definition.Console;
using Facility.Definition.Fsd;

namespace fsdgenfsd
{
	public sealed class FsdGenFsdApp
	{
		public static int Main(string[] args)
		{
			try
			{
				var argsReader = new ArgsReader(args);
				if (argsReader.ReadHelpFlag())
				{
					foreach (string line in s_usageText)
						Console.WriteLine(line);
					return 0;
				}
				else
				{
					var app = new FsdGenFsdApp(argsReader);
					argsReader.VerifyComplete();
					return app.Run();
				}
			}
			catch (ArgsReaderException exception)
			{
				Console.Error.WriteLine(exception.Message);
				Console.Error.WriteLine();
				foreach (string line in s_usageText)
					Console.Error.WriteLine(line);
				return 1;
			}
			catch (ServiceDefinitionException exception)
			{
				Console.Error.WriteLine(exception.Message);
				return 1;
			}
			catch (Exception exception)
			{
				Console.Error.WriteLine(exception.ToString());
				return 2;
			}
		}

		public FsdGenFsdApp(ArgsReader args)
		{
			m_generator = new FsdGenerator
			{
				GeneratorName = "fsdgenfsd",
				IndentText = args.ReadIndentOption(),
				NewLine = args.ReadNewLineOption(),
			};

			m_inputFile = args.ReadArgument();
			m_outputFile = args.ReadArgument();
		}

		public int Run()
		{
			var input = m_inputFile == null || m_inputFile == "-" ?
				new ServiceTextSource(Console.In.ReadToEnd()) :
				new ServiceTextSource(File.ReadAllText(m_inputFile)).WithName(Path.GetFileName(m_inputFile));

			var parser = new FsdParser();
			var definition = parser.ParseDefinition(input);

			var output = m_generator.GenerateOutput(definition);

			if (m_outputFile == null)
				Console.Out.Write(output.Text);
			else
				File.WriteAllText(m_outputFile, output.Text);

			return 0;
		}

		static readonly IReadOnlyList<string> s_usageText = new[]
		{
			"Usage: fsdgenfsd [input] [output] [options]",
			"",
			"   input",
			"      The source FSD file. (Standard input if omitted or \"-\".)",
			"   output",
			"      The destination FSD file. (Standard output if omitted.)",
			"",
			"   --indent (tab|1|2|3|4|5|6|7|8)",
			"      The indent used in the output: a tab or a number of spaces.",
			"   --newline (auto|lf|crlf)",
			"      The newline used in the output.",
		};

		readonly FsdGenerator m_generator;
		readonly string m_inputFile;
		readonly string m_outputFile;
	}
}

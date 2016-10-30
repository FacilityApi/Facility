using System;
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
				var argParser = new ArgsReader(args);
				var app = new FsdGenFsdApp(argParser);
				argParser.VerifyComplete();
				return app.Run();
			}
			catch (Exception exception) when (exception is ApplicationException || exception is ArgsReaderException || exception is ServiceDefinitionException)
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

		readonly FsdGenerator m_generator;
		readonly string m_inputFile;
		readonly string m_outputFile;
	}
}

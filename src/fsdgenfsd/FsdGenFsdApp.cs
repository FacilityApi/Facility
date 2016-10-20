using System;
using System.Collections.Generic;
using System.IO;
using Facility.Definition;
using Facility.Definition.Fsd;

namespace fsdgenfsd
{
	public sealed class FsdGenFsdApp
	{
		public static int Main(string[] args)
		{
			return new FsdGenFsdApp().Run(args);
		}

		public int Run(IReadOnlyList<string> args)
		{
			try
			{
				return RunCore(args);
			}
			catch (Exception exception) when (exception is ApplicationException || exception is ServiceDefinitionException)
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

		private int RunCore(IReadOnlyList<string> args)
		{
			var input = args.Count <= 0 || args[0] == "-" ?
				new ServiceTextSource("input", Console.In.ReadToEnd()) :
				new ServiceTextSource(Path.GetFileName(args[0]), File.ReadAllText(args[0]));

			var parser = new FsdParser();
			var definition = parser.ParseDefinition(input);

			var generator = new FsdGenerator { GeneratorName = "fsdgenfsd" };
			var output = generator.GenerateOutput(definition);

			if (args.Count <= 1)
				Console.Out.Write(output.Text);
			else
				File.WriteAllText(args[1], output.Text);

			return 0;
		}
	}
}

using System;
using System.Collections.Generic;
using Facility.Definition.Fsd;

namespace Facility.Definition.UnitTests
{
	internal static class TestUtility
	{
		public static ServiceInfo ParseTestApi(string text)
		{
			return new FsdParser().ParseDefinition(new ServiceDefinitionText("TestApi.fsd", text));
		}

		public static ServiceDefinitionException ParseInvalidTestApi(string text)
		{
			try
			{
				ParseTestApi(text);
				throw new InvalidOperationException("Parse did not fail.");
			}
			catch (ServiceDefinitionException exception)
			{
				return exception;
			}
		}

		public static IReadOnlyList<ServiceDefinitionError> TryParseInvalidTestApi(string text)
		{
			if (new FsdParser().TryParseDefinition(new ServiceDefinitionText("TestApi.fsd", text), out _, out var errors))
				throw new InvalidOperationException("Parse did not fail.");
			return errors;
		}

		public static string[] GenerateFsd(ServiceInfo service)
		{
			var generator = new FsdGenerator { GeneratorName = "TestUtility" };
			return generator.GenerateOutput(service).Files[0].Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
		}
	}
}

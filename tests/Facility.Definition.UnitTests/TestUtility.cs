using System;
using System.Collections.Generic;
using Facility.Definition.Fsd;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	internal static class TestUtility
	{
		public static void ThrowsServiceDefinitionException(Action action, NamedTextPosition position)
		{
			try
			{
				action();
				throw new InvalidOperationException("Action did not throw.");
			}
			catch (ServiceDefinitionException exception)
			{
				Assert.AreSame(position, exception.Position);
			}
		}

		public static void ThrowsServiceDefinitionException(Func<object> func, NamedTextPosition position)
		{
			ThrowsServiceDefinitionException(
				() =>
				{
					func();
				}, position);
		}

		public static ServiceInfo ParseTestApi(string text)
		{
			return new FsdParser().ParseDefinition(new NamedText("TestApi.fsd", text));
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
			ServiceInfo service;
			IReadOnlyList<ServiceDefinitionError> errors;
			if (new FsdParser().TryParseDefinition(new NamedText("TestApi.fsd", text), out service, out errors))
				throw new InvalidOperationException("Parse did not fail.");
			return errors;
		}

		public static string[] GenerateFsd(ServiceInfo service)
		{
			var generator = new FsdGenerator { GeneratorName = "TestUtility" };
			return generator.GenerateOutput(service).NamedTexts[0].Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
		}
	}
}

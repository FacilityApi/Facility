using System;
using System.Collections.Generic;
using Facility.Definition.Fsd;
using Facility.Definition.Http;

namespace Facility.Definition.UnitTests.Http
{
	public abstract class HttpInfoTestsBase
	{
		protected static HttpServiceInfo ParseHttpApi(string text)
		{
			return new HttpServiceInfo(TestUtility.ParseTestApi(text));
		}

		protected static ServiceDefinitionException ParseInvalidHttpApi(string text)
		{
			try
			{
				ParseHttpApi(text);
				throw new InvalidOperationException("Parse did not fail.");
			}
			catch (ServiceDefinitionException exception)
			{
				return exception;
			}
		}

		protected IReadOnlyList<ServiceDefinitionError> TryParseInvalidHttpApi(string text)
		{
			HttpServiceInfo httpServiceInfo;
			IReadOnlyList<ServiceDefinitionError> errors;
			if (HttpServiceInfo.TryCreate(TestUtility.ParseTestApi(text), out httpServiceInfo, out errors))
				throw new InvalidOperationException("Parse did not fail.");
			return errors;
		}
	}
}

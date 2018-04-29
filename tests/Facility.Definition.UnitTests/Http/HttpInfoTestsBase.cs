using System;
using Facility.Definition.Http;

namespace Facility.Definition.UnitTests.Http
{
	public abstract class HttpInfoTestsBase
	{
		protected static HttpServiceInfo ParseHttpApi(string text)
		{
			return HttpServiceInfo.Create(TestUtility.ParseTestApi(text));
		}

		protected static ServiceDefinitionError ParseInvalidHttpApi(string text)
		{
			HttpServiceInfo.TryCreate(TestUtility.ParseTestApi(text), out _, out var errors);
			if (errors.Count == 0)
				throw new InvalidOperationException("Parse did not fail.");
			return errors[0];
		}
	}
}

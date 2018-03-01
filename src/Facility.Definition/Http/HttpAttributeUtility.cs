using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace Facility.Definition.Http
{
	internal static class HttpAttributeUtility
	{
		public static ServiceAttributeInfo TryGetHttpAttribute(this IServiceElementInfo element)
		{
			return element.TryGetAttribute("http");
		}

		public static IReadOnlyList<ServiceAttributeParameterInfo> GetHttpParameters(this IServiceElementInfo element)
		{
			return element.TryGetAttribute("http")?.Parameters ?? new ServiceAttributeParameterInfo[0];
		}

		public static ServiceDefinitionError CreateInvalidHttpParameterError(this ServiceAttributeParameterInfo parameter)
		{
			return new ServiceDefinitionError($"Unexpected 'http' parameter '{parameter.Name}'.", parameter.Position);
		}

		public static HttpStatusCode ParseStatusCodeInteger(ServiceAttributeParameterInfo parameter)
		{
			int valueAsInteger;
			int.TryParse(parameter.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out valueAsInteger);
			if (valueAsInteger < 200 || valueAsInteger >= 599)
				throw new ServiceDefinitionException($"'{parameter.Name}' parameter must be an integer between 200 and 599.", parameter.Position);
			return (HttpStatusCode) valueAsInteger;
		}
	}
}

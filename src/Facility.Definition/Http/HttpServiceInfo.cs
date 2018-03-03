using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition.Http
{
	/// <summary>
	/// The HTTP mapping for a service.
	/// </summary>
	public sealed class HttpServiceInfo
	{
		/// <summary>
		/// Creates an HTTP mapping for a service.
		/// </summary>
		public HttpServiceInfo(ServiceInfo serviceInfo)
			: this(serviceInfo, ValidationMode.Throw)
		{
		}

		/// <summary>
		/// Attempts to create an HTTP mapping for a service.
		/// </summary>
		/// <remarks>Returns true if there are no errors.</remarks>
		public static bool TryCreate(ServiceInfo serviceInfo, out HttpServiceInfo httpServiceInfo, out IReadOnlyList<ServiceDefinitionError> errors)
		{
			httpServiceInfo = new HttpServiceInfo(serviceInfo, ValidationMode.Return);
			errors = httpServiceInfo.GetValidationErrors().ToList();
			return errors.Count == 0;
		}

		private HttpServiceInfo(ServiceInfo serviceInfo, ValidationMode validationMode)
		{
			Service = serviceInfo;

			foreach (var parameter in serviceInfo.GetHttpParameters())
			{
				if (parameter.Name == "url")
					Url = parameter.Value;
				else
					m_errors.Add(parameter.CreateInvalidHttpParameterError());
			}

			Methods = serviceInfo.Methods.Select(x => new HttpMethodInfo(x, serviceInfo)).ToList();
			ErrorSets = serviceInfo.ErrorSets.Select(x => new HttpErrorSetInfo(x)).ToList();

			var unexpectedHttpAttribute = serviceInfo.Dtos.AsEnumerable<IServiceElementInfo>()
				.Concat(serviceInfo.Dtos.SelectMany(x => x.Fields))
				.Concat(serviceInfo.Enums)
				.Concat(serviceInfo.Enums.SelectMany(x => x.Values))
				.Select(x => x.TryGetHttpAttribute())
				.FirstOrDefault(x => x != null);
			if (unexpectedHttpAttribute != null)
				m_errors.Add(new ServiceDefinitionError("'http' attribute not supported on this element.", unexpectedHttpAttribute.Position));

			var methodsByRoute = Methods.OrderBy(x => x, HttpMethodInfo.ByRouteComparer).ToList();
			for (int index = 1; index < methodsByRoute.Count; index++)
			{
				var left = methodsByRoute[index - 1];
				var right = methodsByRoute[index];
				if (HttpMethodInfo.ByRouteComparer.Compare(left, right) == 0)
					m_errors.Add(new ServiceDefinitionError($"Methods '{left.ServiceMethod.Name}' and '{right.ServiceMethod.Name}' have the same route: {right.Method} {right.Path}", right.ServiceMethod.Position));
			}

			if (validationMode == ValidationMode.Throw)
				GetValidationErrors().ThrowIfAny();
		}

		/// <summary>
		/// The service.
		/// </summary>
		public ServiceInfo Service { get; }

		/// <summary>
		/// The URL of the HTTP service.
		/// </summary>
		public string Url { get; }

		/// <summary>
		/// The HTTP mapping for the methods.
		/// </summary>
		public IReadOnlyList<HttpMethodInfo> Methods { get; }

		/// <summary>
		/// The HTTP mapping for the error sets.
		/// </summary>
		public IReadOnlyList<HttpErrorSetInfo> ErrorSets { get; }

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return m_errors
				.Concat(Methods.SelectMany(x => x.GetValidationErrors()))
				.Concat(ErrorSets.SelectMany(x => x.GetValidationErrors()));
		}

		private readonly List<ServiceDefinitionError> m_errors = new List<ServiceDefinitionError>();
	}
}

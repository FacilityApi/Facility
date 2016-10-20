using System;

namespace Facility.Definition
{
	/// <summary>
	/// A service definition.
	/// </summary>
	public sealed class ServiceDefinitionInfo
	{
		/// <summary>
		/// Creates a service definition.
		/// </summary>
		public ServiceDefinitionInfo(ServiceInfo service)
		{
			if (service == null)
				throw new ArgumentNullException(nameof(service));

			Service = service;
		}

		/// <summary>
		/// The service.
		/// </summary>
		public ServiceInfo Service { get; }
	}
}

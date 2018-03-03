using System.Collections.Generic;

namespace Facility.Definition.Http
{
	/// <summary>
	/// Information about a field that corresponds to a request or response HTTP header.
	/// </summary>
	public sealed class HttpHeaderFieldInfo
	{
		/// <summary>
		/// The service field.
		/// </summary>
		public ServiceFieldInfo ServiceField { get; }

		/// <summary>
		/// The name of the HTTP header.
		/// </summary>
		public string Name { get; }

		internal HttpHeaderFieldInfo(ServiceFieldInfo fieldInfo)
		{
			ServiceField = fieldInfo;
			Name = fieldInfo.Name;

			foreach (var parameter in fieldInfo.GetHttpParameters())
			{
				if (parameter.Name == "name")
					Name = parameter.Value;
				else if (parameter.Name != "from")
					m_errors.Add(parameter.CreateInvalidHttpParameterError());
			}
		}

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return m_errors;
		}

		private readonly List<ServiceDefinitionError> m_errors = new List<ServiceDefinitionError>();
	}
}

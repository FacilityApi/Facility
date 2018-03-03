using System.Collections.Generic;

namespace Facility.Definition.Http
{
	/// <summary>
	/// Information about a field that corresponds to a request path parameter.
	/// </summary>
	public sealed class HttpPathFieldInfo
	{
		/// <summary>
		/// The service field.
		/// </summary>
		public ServiceFieldInfo ServiceField { get; }

		/// <summary>
		/// The name of the path parameter.
		/// </summary>
		public string Name => ServiceField.Name;

		internal HttpPathFieldInfo(ServiceFieldInfo fieldInfo)
		{
			ServiceField = fieldInfo;

			foreach (var parameter in fieldInfo.GetHttpParameters())
			{
				if (parameter.Name != "from")
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

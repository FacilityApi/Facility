using System.Collections.Generic;

namespace Facility.Definition.Http
{
	/// <summary>
	/// Information about a field that corresponds to a request query parameter.
	/// </summary>
	public sealed class HttpQueryFieldInfo
	{
		/// <summary>
		/// The service field.
		/// </summary>
		public ServiceFieldInfo ServiceField { get; }

		/// <summary>
		/// The name of the query parameter.
		/// </summary>
		public string Name { get; }

		internal HttpQueryFieldInfo(ServiceFieldInfo fieldInfo)
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

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors() => m_errors;

		private readonly List<ServiceDefinitionError> m_errors = new List<ServiceDefinitionError>();
	}
}

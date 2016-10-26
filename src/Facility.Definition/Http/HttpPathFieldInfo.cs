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

		internal HttpPathFieldInfo(ServiceFieldInfo fieldInfo)
		{
			ServiceField = fieldInfo;

			foreach (var parameter in fieldInfo.GetHttpParameters())
			{
				if (parameter.Name != "from")
					throw parameter.CreateInvalidHttpParameterException();
			}
		}
	}
}

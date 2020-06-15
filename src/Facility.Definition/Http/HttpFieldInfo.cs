namespace Facility.Definition.Http
{
	/// <summary>
	/// Information about a field.
	/// </summary>
	public abstract class HttpFieldInfo : HttpElementInfo
	{
		/// <summary>
		/// The service field.
		/// </summary>
		public ServiceFieldInfo ServiceField { get; }

		private protected HttpFieldInfo(ServiceFieldInfo fieldInfo)
		{
			ServiceField = fieldInfo;
		}
	}
}

namespace Facility.Definition.Http
{
	/// <summary>
	/// Information about a normal request or response field.
	/// </summary>
	public sealed class HttpNormalFieldInfo
	{
		/// <summary>
		/// The service field.
		/// </summary>
		public ServiceFieldInfo ServiceField { get; }

		internal HttpNormalFieldInfo(ServiceFieldInfo fieldInfo)
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

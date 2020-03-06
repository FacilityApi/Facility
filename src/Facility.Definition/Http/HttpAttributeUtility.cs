namespace Facility.Definition.Http
{
	internal static class HttpAttributeUtility
	{
		public static ServiceAttributeInfo? TryGetHttpAttribute(this ServiceElementWithAttributesInfo element) => element.TryGetAttribute("http");
	}
}

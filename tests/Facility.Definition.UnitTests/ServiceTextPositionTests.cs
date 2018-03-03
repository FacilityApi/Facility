using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceMethodInfoTests
	{
		[Test]
		public void InvalidNameThrows()
		{
			var position = new NamedTextPosition("source");
			TestUtility.ThrowsServiceDefinitionException(() => new ServiceMethodInfo(name: "4u", position: position), position);
		}

		[TestCase(true), TestCase(false)]
		public void DuplicateFieldThrows(bool isRequest)
		{
			var fields = new[]
			{
				new ServiceFieldInfo("why", "int32", position: new NamedTextPosition("source", 1)),
				new ServiceFieldInfo("Why", "int32", position: new NamedTextPosition("source", 2)),
			};
			TestUtility.ThrowsServiceDefinitionException(
				() => new ServiceMethodInfo(name: "x", requestFields: isRequest ? fields : null, responseFields: isRequest ? null : fields), fields[1].Position);
		}
	}
}

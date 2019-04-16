using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public sealed class InterleavedMarkdownTests
	{
		[Test]
		public void EmptyServiceNoRemarks()
		{
			var service = TestUtility.ParseTestApi(@"
```fsd
service TestApi{}
```");

			service.Name.Should().Be("TestApi");
			service.Remarks.Count.Should().Be(0);
		}

		[Test]
		public void EmptyServiceWithRemarks()
		{
			var service = TestUtility.ParseTestApi(@"
# TestApi

```fsd
service TestApi
{
```

A test API.

```fsd
}
```

That's it.
");

			service.Name.Should().Be("TestApi");
			service.Remarks.Should().Equal("A test API.");
		}

		[Test]
		public void VariousRemarks()
		{
			var service = TestUtility.ParseTestApi(@"
# TestApi

```fsd
service TestApi
{
```

A test API.

```fsd
// nothing
```

More remarks.

```fsd
data Data
{
}

method do
{
}:
{
}
```

A test method.

```fsd
}
```

That's it.
");

			service.Name.Should().Be("TestApi");
			service.Remarks.Should().Equal("A test API.", "", "More remarks.");
			service.Dtos.Single().Remarks.Should().BeEmpty();
			service.Methods.Single().Remarks.Should().Equal("A test method.");
		}

		[Test]
		public void VariousRemarksNoServiceBraces()
		{
			var service = TestUtility.ParseTestApi(@"
# TestApi

```fsd
service TestApi
```

A test API.

```fsd
// nothing
```

More remarks.

```fsd
data Data
{
}

method do
{
}:
{
}
```

A test method.
");

			service.Name.Should().Be("TestApi");
			service.Remarks.Should().Equal("A test API.", "", "More remarks.");
			service.Dtos.Single().Remarks.Should().BeEmpty();
			service.Methods.Single().Remarks.Should().Equal("A test method.");
		}
	}
}

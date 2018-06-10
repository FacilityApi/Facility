using System.Collections.Generic;

namespace Facility.Definition.Fsd
{
	internal sealed class FsdRemarksSection
	{
		public FsdRemarksSection(IReadOnlyList<string> lines, ServiceDefinitionPosition position)
		{
			Lines = lines;
			Position = position;
		}

		public IReadOnlyList<string> Lines { get; }

		public ServiceDefinitionPosition Position { get; }
	}
}

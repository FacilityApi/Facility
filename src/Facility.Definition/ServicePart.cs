namespace Facility.Definition
{
	/// <summary>
	/// A part of a service element.
	/// </summary>
	public sealed class ServicePart
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public ServicePart(ServicePartKind kind, ServiceDefinitionPosition position, ServiceDefinitionPosition? endPosition = null)
		{
			Kind = kind;
			Position = position;
			EndPosition = endPosition ?? position;
		}

		/// <summary>
		/// The kind of service part.
		/// </summary>
		public ServicePartKind Kind { get; }

		/// <summary>
		/// The position of the service part.
		/// </summary>
		public ServiceDefinitionPosition Position { get; }

		/// <summary>
		/// The end position of the service part.
		/// </summary>
		public ServiceDefinitionPosition EndPosition { get; }
	}
}

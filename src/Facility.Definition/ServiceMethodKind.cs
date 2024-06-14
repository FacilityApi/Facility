namespace Facility.Definition;

/// <summary>
/// The kind of service method.
/// </summary>
public enum ServiceMethodKind
{
	/// <summary>
	/// A normal method.
	/// </summary>
	Normal,

	/// <summary>
	/// An event, i.e. a method that can indefinitely and repeatedly provide responses.
	/// </summary>
	Event,
}

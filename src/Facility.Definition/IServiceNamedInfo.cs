namespace Facility.Definition
{
	/// <summary>
	/// Properties common to named service components.
	/// </summary>
	public interface IServiceNamedInfo
	{
		/// <summary>
		/// The name.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The position in the definition.
		/// </summary>
		ServiceTextPosition Position { get; }
	}
}

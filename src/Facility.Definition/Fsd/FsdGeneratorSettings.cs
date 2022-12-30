using Facility.Definition.CodeGen;

namespace Facility.Definition.Fsd;

/// <summary>
/// Settings for generating an FSD file for a service definition.
/// </summary>
public sealed class FsdGeneratorSettings : FileGeneratorSettings
{
	/// <summary>
	/// True to generate a file-scoped service (instead of using braces).
	/// </summary>
	public bool FileScopedService { get; set; }
}

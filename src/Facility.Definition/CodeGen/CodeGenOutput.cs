namespace Facility.Definition.CodeGen;

/// <summary>
/// The output of a code generator.
/// </summary>
public sealed class CodeGenOutput
{
	/// <summary>
	/// Creates a single-output instance.
	/// </summary>
	public CodeGenOutput(CodeGenFile? file)
		: this(files: file is null ? null : new[] { file }, patternsToClean: null)
	{
	}

	/// <summary>
	/// Creates a multi-output instance.
	/// </summary>
	public CodeGenOutput(IReadOnlyList<CodeGenFile>? files, IReadOnlyList<CodeGenPattern>? patternsToClean)
	{
		Files = files ?? [];
		PatternsToClean = patternsToClean ?? [];

		var duplicate = Files.GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.Skip(1).Any());
		if (duplicate is not null)
			throw new ArgumentException($"File names must be unique but '{duplicate.Key}' is duplicated.", nameof(files));
	}

	/// <summary>
	/// The files.
	/// </summary>
	public IReadOnlyList<CodeGenFile> Files { get; }

	/// <summary>
	/// The patterns to clean.
	/// </summary>
	public IReadOnlyList<CodeGenPattern> PatternsToClean { get; }
}

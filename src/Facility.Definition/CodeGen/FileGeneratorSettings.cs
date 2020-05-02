using System.Collections.Generic;

namespace Facility.Definition.CodeGen
{
	/// <summary>
	/// Common settings for file generator settings.
	/// </summary>
	public abstract class FileGeneratorSettings
	{
		/// <summary>
		/// The path to the input file ("-" for stdin).
		/// </summary>
		public string? InputPath { get; set; }

		/// <summary>
		/// The path to the output directory or file ("-" for stdout).
		/// </summary>
		public string? OutputPath { get; set; }

		/// <summary>
		/// Excludes service elements with the specified tags.
		/// </summary>
		public IReadOnlyList<string>? ExcludeTags { get; set; }

		/// <summary>
		/// The indent used in the output.
		/// </summary>
		public string? IndentText { get; set; }

		/// <summary>
		/// The newline used in the output.
		/// </summary>
		public string? NewLine { get; set; }

		/// <summary>
		/// Deletes previously generated files that are no longer used.
		/// </summary>
		public bool ShouldClean { get; set; }

		/// <summary>
		/// Suppresses normal console output.
		/// </summary>
		public bool IsQuiet { get; set; }

		/// <summary>
		/// Executes without making changes to the file system.
		/// </summary>
		public bool IsDryRun { get; set; }

		/// <summary>
		/// Does not overwrite files that only differ by newline.
		/// </summary>
		public bool IgnoreNewLines { get; set; }
	}
}

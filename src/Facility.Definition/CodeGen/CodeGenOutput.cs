using System.Collections.Generic;

namespace Facility.Definition.CodeGen
{
	/// <summary>
	/// The output of a code generator.
	/// </summary>
	public sealed class CodeGenOutput
	{
		/// <summary>
		/// Creates a single-output instance.
		/// </summary>
		public CodeGenOutput(CodeGenFile file)
			: this(files: file == null ? null : new[] { file }, patternsToClean: null)
		{
		}

		/// <summary>
		/// Creates a multi-output instance.
		/// </summary>
		public CodeGenOutput(IReadOnlyList<CodeGenFile> files, IReadOnlyList<CodeGenPattern> patternsToClean)
		{
			Files = files ?? new CodeGenFile[0];
			PatternsToClean = patternsToClean ?? new CodeGenPattern[0];
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
}

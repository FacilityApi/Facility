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
		public CodeGenOutput(NamedText namedText)
			: this(namedTexts: namedText == null ? null : new[] { namedText }, patternsToClean: null)
		{
		}

		/// <summary>
		/// Creates a multi-output instance.
		/// </summary>
		public CodeGenOutput(IReadOnlyList<NamedText> namedTexts, IReadOnlyList<CodeGenPattern> patternsToClean)
		{
			NamedTexts = namedTexts ?? new NamedText[0];
			PatternsToClean = patternsToClean ?? new CodeGenPattern[0];
		}

		/// <summary>
		/// The named texts.
		/// </summary>
		public IReadOnlyList<NamedText> NamedTexts { get; }

		/// <summary>
		/// The patterns to clean.
		/// </summary>
		public IReadOnlyList<CodeGenPattern> PatternsToClean { get; }
	}
}

using System;
using System.IO;

namespace Facility.Definition.CodeGen
{
	/// <summary>
	/// Base class for code generators.
	/// </summary>
	public abstract class CodeGenerator
	{
		/// <summary>
		/// The name of the generator (for comments).
		/// </summary>
		public string? GeneratorName { get; set; }

		/// <summary>
		/// The text to use for each indent (null for tab).
		/// </summary>
		public string? IndentText { get; set; }

		/// <summary>
		/// The text to use for each new line (null for default).
		/// </summary>
		public string? NewLine { get; set; }

		/// <summary>
		/// Generates output for the specified service.
		/// </summary>
		public abstract CodeGenOutput GenerateOutput(ServiceInfo service);

		/// <summary>
		/// True if the generator supports writing output to a single file. (Default false.)
		/// </summary>
		public virtual bool SupportsSingleOutput => false;

		/// <summary>
		/// True if patterns to clean are returned with the output. (Default false.)
		/// </summary>
		public virtual bool HasPatternsToClean => false;

		/// <summary>
		/// True if the generator respects <see cref="IndentText"/>. (Default true.)
		/// </summary>
		public virtual bool RespectsIndentText => true;

		/// <summary>
		/// True if the generator respects <see cref="NewLine"/>. (Default true.)
		/// </summary>
		public virtual bool RespectsNewLine => true;

		/// <summary>
		/// Applies any generator-specific settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public virtual void ApplySettings(FileGeneratorSettings settings)
		{
		}

		/// <summary>
		/// Creates a file from a name and code writer.
		/// </summary>
		protected CodeGenFile CreateFile(string name, Action<CodeWriter> writeTo)
		{
			using var stringWriter = new StringWriter();

			if (NewLine != null)
				stringWriter.NewLine = NewLine;

			var code = new CodeWriter(stringWriter);

			if (IndentText != null)
				code.IndentText = IndentText;

			writeTo(code);

			return new CodeGenFile(name, stringWriter.ToString());
		}
	}
}

using System;

namespace Facility.Definition.CodeGen
{
	/// <summary>
	/// A code-generated file.
	/// </summary>
	public sealed class CodeGenFile
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public CodeGenFile(string name, string text)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Text = text ?? throw new ArgumentNullException(nameof(text));
		}

		/// <summary>
		/// The file name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The text.
		/// </summary>
		public string Text { get; }
	}
}

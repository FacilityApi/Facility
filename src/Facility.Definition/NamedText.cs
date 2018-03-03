using System;

namespace Facility.Definition
{
	/// <summary>
	/// A source of text.
	/// </summary>
	public sealed class NamedText
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public NamedText(string name, string text)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Text = text ?? throw new ArgumentNullException(nameof(text));
		}

		/// <summary>
		/// The name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The text.
		/// </summary>
		public string Text { get; }
	}
}

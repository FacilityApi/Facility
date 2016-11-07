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
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (text == null)
				throw new ArgumentNullException(nameof(text));

			Name = name;
			Text = text;
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

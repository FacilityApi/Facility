using System;

namespace Facility.Definition
{
	/// <summary>
	/// A source of text.
	/// </summary>
	public sealed class ServiceTextSource
	{
		/// <summary>
		/// Creates an instance from text.
		/// </summary>
		public ServiceTextSource(string text)
			: this(text, "")
		{
		}

		/// <summary>
		/// The text of the source.
		/// </summary>
		public string Text { get; }

		/// <summary>
		/// The name of the source.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Assigns a name to the text source.
		/// </summary>
		public ServiceTextSource WithName(string name)
		{
			return new ServiceTextSource(Text, name);
		}

		private ServiceTextSource(string text, string name)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Text = text;
			Name = name;
		}
	}
}

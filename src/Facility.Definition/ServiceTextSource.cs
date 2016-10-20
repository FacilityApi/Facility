namespace Facility.Definition
{
	/// <summary>
	/// A source of text.
	/// </summary>
	public sealed class ServiceTextSource
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public ServiceTextSource(string name, string text)
		{
			Name = name;
			Text = text;
		}

		/// <summary>
		/// The name of the source.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The text of the source.
		/// </summary>
		public string Text { get; }
	}
}

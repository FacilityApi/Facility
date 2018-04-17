using System;

namespace Facility.Definition
{
	/// <summary>
	/// Named text containing a service definition.
	/// </summary>
	public sealed class ServiceDefinitionText
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public ServiceDefinitionText(string name, string text)
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

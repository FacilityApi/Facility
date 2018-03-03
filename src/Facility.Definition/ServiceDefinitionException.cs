using System;

namespace Facility.Definition
{
	/// <summary>
	/// Thrown when an error occurs while processing a service definition.
	/// </summary>
	public sealed class ServiceDefinitionException : Exception
	{
		/// <summary>
		/// Creates an exception.
		/// </summary>
		public ServiceDefinitionException(string error, NamedTextPosition position = null, Exception innerException = null)
			: base("", innerException)
		{
			Error = error ?? throw new ArgumentNullException(nameof(error));
			Position = position;
		}

		/// <summary>
		/// The error message.
		/// </summary>
		public string Error { get; }

		/// <summary>
		/// The position where the error took place, if any.
		/// </summary>
		public NamedTextPosition Position { get; }

		/// <summary>
		/// The exception message, which displays the file name, line number, column number, and error message.
		/// </summary>
		public override string Message => Position != null ? $"{Position}: {Error}" : Error;
	}
}

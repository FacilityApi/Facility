using System;

namespace Facility.Definition
{
	/// <summary>
	/// An error while processing a service definition.
	/// </summary>
	public sealed class ServiceDefinitionError
	{
		/// <summary>
		/// Creates an error.
		/// </summary>
		public ServiceDefinitionError(string message, NamedTextPosition position, Exception exception = null)
		{
			Message = message ?? throw new ArgumentNullException(nameof(message));
			Position = position;
			Exception = exception;
		}

		/// <summary>
		/// The error message.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// The position where the error took place, if any.
		/// </summary>
		public NamedTextPosition Position { get; }

		/// <summary>
		/// The exception that caused the error, if any.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		public override string ToString() => Position != null ? $"{Position}: {Message}" : Message;

		internal ServiceDefinitionException CreateException()
		{
			return new ServiceDefinitionException(Message, Position, Exception);
		}
	}
}

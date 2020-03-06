using System;

namespace Facility.Definition
{
	/// <summary>
	/// An error while processing a service definition.
	/// </summary>
	public sealed class ServiceDefinitionError
	{
		/// <summary>
		/// Creates a service definition error.
		/// </summary>
		public ServiceDefinitionError(string message, ServiceDefinitionPosition? position = null)
		{
			Message = message ?? throw new ArgumentNullException(nameof(message));
			Position = position;
		}

		/// <summary>
		/// The error message.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// The position where the error took place, if any.
		/// </summary>
		public ServiceDefinitionPosition? Position { get; }

		/// <summary>
		/// Returns a string with the position and the error message.
		/// </summary>
		public override string ToString() => Position != null ? $"{Position}: {Message}" : Message;
	}
}

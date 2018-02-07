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
		public ServiceDefinitionError(string error, NamedTextPosition position = null, Exception innerException = null)
		{
			if (error == null)
				throw new ArgumentNullException(nameof(error));

			Error = error;
			Position = position;
			InnerException = innerException;
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
		/// The exception that caused the error, if any.
		/// </summary>
		public Exception InnerException { get; }

		internal ServiceDefinitionException CreateException()
		{
			return new ServiceDefinitionException(Error, Position, InnerException);
		}
	}
}

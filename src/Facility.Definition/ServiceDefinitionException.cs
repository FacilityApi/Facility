using System;
using System.Collections.Generic;

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
		public ServiceDefinitionException(IEnumerable<ServiceDefinitionError> errors, Exception? innerException = null)
			: base("", innerException)
		{
			if (errors == null)
				throw new ArgumentNullException(nameof(errors));

			Errors = errors.ToReadOnlyList();

			if (Errors.Count == 0)
				throw new ArgumentException("There must be at least one error.", nameof(errors));
		}

		/// <summary>
		/// The errors.
		/// </summary>
		public IReadOnlyList<ServiceDefinitionError> Errors { get; }

		/// <summary>
		/// The exception message, which displays the file name, line number, column number, and error message of the first error.
		/// </summary>
		public override string Message
		{
			get
			{
				var firstError = Errors[0];
				return firstError.Position != null ? $"{firstError.Position}: {firstError.Message}" : firstError.Message;
			}
		}
	}
}

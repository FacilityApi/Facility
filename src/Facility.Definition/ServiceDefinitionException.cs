using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
			: this(new ServiceDefinitionError(error, position), innerException)
		{
		}

		/// <summary>
		/// Creates an exception.
		/// </summary>
		public ServiceDefinitionException(ServiceDefinitionError error, Exception innerException = null)
			: this(new[] { error }, innerException)
		{
		}

		/// <summary>
		/// Creates an exception.
		/// </summary>
		public ServiceDefinitionException(IEnumerable<ServiceDefinitionError> errors, Exception innerException = null)
			: base("", innerException)
		{
			if (errors == null)
				throw new ArgumentNullException(nameof(errors));

			Errors = new ReadOnlyCollection<ServiceDefinitionError>(errors.ToList());

			if (Errors.Count == 0)
				throw new ArgumentException("There must be at least one error.", nameof(errors));
		}

		/// <summary>
		/// The errors.
		/// </summary>
		public IReadOnlyList<ServiceDefinitionError> Errors { get; }

		/// <summary>
		/// The error message of the first error.
		/// </summary>
		[Obsolete("Prefer Errors[0].Message.")]
		public string Error => Errors[0].Message;

		/// <summary>
		/// The position where the first error took place, if any.
		/// </summary>
		[Obsolete("Prefer Errors[0].Position.")]
		public NamedTextPosition Position => Errors[0].Position;

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

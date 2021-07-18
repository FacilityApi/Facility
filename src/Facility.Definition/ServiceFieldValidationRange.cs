using System;

namespace Facility.Definition
{
	/// <summary>
	/// A range for validating numeric values
	/// </summary>
	public sealed class ServiceFieldValidationRange
	{
		/// <summary>
		/// Creates a range for validation
		/// </summary>
		/// <param name="startInclusive">The inclusive start of the range</param>
		/// <param name="endInclusive">The inclusive end of the range</param>
		public ServiceFieldValidationRange(decimal? startInclusive, decimal? endInclusive)
		{
			if (startInclusive == null && endInclusive == null)
				throw new ArgumentException($"{nameof(startInclusive)} or {nameof(endInclusive)} must be specified");

			StartInclusive = startInclusive;
			EndInclusive = endInclusive;
		}

		/// <summary>
		/// The start of the range
		/// </summary>
		public decimal? StartInclusive { get; }

		/// <summary>
		/// The end of the range
		/// </summary>
		public decimal? EndInclusive { get; }

		/// <summary>
		/// Verifies if a value falls within the range
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns><c>true</c> if the range contains the value</returns>
		public bool Contains(decimal value) => !(StartInclusive > value || EndInclusive < value);

		/// <summary>
		/// Verifies if a value falls within the range
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns><c>true</c> if the range contains the value</returns>
		public bool Contains(double value) => Contains(Convert.ToDecimal(value));

		/// <summary>
		/// Verifies if a value falls within the range
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns><c>true</c> if the range contains the value</returns>
		public bool Contains(float value) => Contains(Convert.ToDecimal(value));

		/// <summary>
		/// Verifies if a value falls within the range
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns><c>true</c> if the range contains the value</returns>
		public bool Contains(int value) => Contains(Convert.ToDecimal(value));

		/// <summary>
		/// Verifies if a value falls within the range
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns><c>true</c> if the range contains the value</returns>
		public bool Contains(long value) => Contains(Convert.ToDecimal(value));

		/// <summary>
		/// Verifies if a value falls within the range
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns><c>true</c> if the range contains the value</returns>
		public bool Contains(uint value) => Contains(Convert.ToDecimal(value));

		/// <summary>
		/// Verifies if a value falls within the range
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns><c>true</c> if the range contains the value</returns>
		public bool Contains(ulong value) => Contains(Convert.ToDecimal(value));
	}
}

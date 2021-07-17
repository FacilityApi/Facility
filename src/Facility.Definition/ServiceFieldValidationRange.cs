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
		/// <returns><c>true</c> if the range contains the value, <c>false</c> otherwise</returns>
		public bool Contains(decimal value) => StartInclusive <= value && value <= EndInclusive;
	}
}

namespace Facility.Definition
{
	/// <summary>
	/// A kind of field type.
	/// </summary>
	public enum ServiceTypeKind
	{
		/// <summary>
		/// A string.
		/// </summary>
		String,

		/// <summary>
		/// A Boolean.
		/// </summary>
		Boolean,

		/// <summary>
		/// A double-precision floating point.
		/// </summary>
		Double,

		/// <summary>
		/// A 32-bit signed integer.
		/// </summary>
		Int32,

		/// <summary>
		/// A 64-bit signed integer.
		/// </summary>
		Int64,

		/// <summary>
		/// A 128-bit number appropriate for monetary calculations.
		/// </summary>
		Decimal,

		/// <summary>
		/// An array of bytes.
		/// </summary>
		Bytes,

		/// <summary>
		/// A JSON object.
		/// </summary>
		Object,

		/// <summary>
		/// An error object.
		/// </summary>
		Error,

		/// <summary>
		/// A DTO.
		/// </summary>
		Dto,

		/// <summary>
		/// An enumerated type.
		/// </summary>
		Enum,

		/// <summary>
		/// A service result.
		/// </summary>
		Result,

		/// <summary>
		/// An array.
		/// </summary>
		Array,

		/// <summary>
		/// A map.
		/// </summary>
		Map,
	}
}

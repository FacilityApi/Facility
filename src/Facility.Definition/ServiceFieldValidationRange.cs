namespace Facility.Definition
{
	public class ServiceFieldValidationRange
	{
		public ServiceFieldValidationRange(decimal? startInclusive, decimal? endInclusive)
		{
			StartInclusive = startInclusive;
			EndInclusive = endInclusive;
		}

		public decimal? StartInclusive { get; }
		public decimal? EndInclusive { get; }
	}
}

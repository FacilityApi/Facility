using System;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class RangeTests
	{
		[Test]
		public void SingleValuedRangeContains()
		{
			var range = new ServiceFieldValidationRange(2M, 2M);
			range.Contains(2M).Should().BeTrue();
		}

		[Test]
		public void FullRangeContains()
		{
			var range = new ServiceFieldValidationRange(0M, 100M);
			range.Contains(50M).Should().BeTrue();
		}

		[Test]
		public void FullRangeDoesNotContainHigh()
		{
			var range = new ServiceFieldValidationRange(0M, 100M);
			range.Contains(1000M).Should().BeFalse();
		}

		[Test]
		public void FullRangeDoesNotContainLow()
		{
			var range = new ServiceFieldValidationRange(0M, 100M);
			range.Contains(-1M).Should().BeFalse();
		}

		[Test]
		public void UnboundedStartRangeContains()
		{
			var range = new ServiceFieldValidationRange(null, 100M);
			range.Contains(50M).Should().BeTrue();
		}

		[Test]
		public void UnboundedStartRangeDoesNotContainHigh()
		{
			var range = new ServiceFieldValidationRange(null, 100M);
			range.Contains(1000M).Should().BeFalse();
		}

		[Test]
		public void UnboundedStartRangeContainsMin()
		{
			var range = new ServiceFieldValidationRange(null, 100M);
			range.Contains(decimal.MinValue).Should().BeTrue();
		}

		[Test]
		public void UnboundedEndRangeContains()
		{
			var range = new ServiceFieldValidationRange(0M, null);
			range.Contains(50M).Should().BeTrue();
		}

		[Test]
		public void UnboundedEndRangeDoesNotContainLow()
		{
			var range = new ServiceFieldValidationRange(0M, null);
			range.Contains(-1M).Should().BeFalse();
		}

		[Test]
		public void UnboundedEndRangeContainsMax()
		{
			var range = new ServiceFieldValidationRange(0M, null);
			range.Contains(decimal.MaxValue).Should().BeTrue();
		}

#pragma warning disable CA1806
		[Test]
		public void ConstructorArgumentsMustBeSpecified()
		{
			Action action = () => new ServiceFieldValidationRange(null, null);
			action.Should().Throw<ArgumentException>();
		}
#pragma warning restore CA1806
	}
}

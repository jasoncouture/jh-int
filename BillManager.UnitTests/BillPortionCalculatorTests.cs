using BillManager.Core;
using System;
using System.Linq;
using Xunit;

namespace BillManager.UnitTests
{
    public class BillPortionCalculatorTests
    {
        [Fact]
        public void BillPortionCalculatorThrowsExceptionWhenCountIsLessThan1()
        {
            var systemUnderTest = new BillPortionCalculator();

            Assert.Throws<ArgumentOutOfRangeException>(() => systemUnderTest.SplitBill(10m, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => systemUnderTest.SplitBill(10m, -1));
        }

        [Fact]
        public void BillPortionsAddUpToTotalWhenNotEvenlyDivisible()
        {
            var systemUnderTest = new BillPortionCalculator();
            decimal expectedTotal = 10m;

            var actualTotal = systemUnderTest.SplitBill(expectedTotal, 9).Sum();

            Assert.Equal(expectedTotal, actualTotal);
        }

        [Fact]
        public void BillPortionsAddUpToTotalWhenEvenlyDivisible()
        {
            var systemUnderTest = new BillPortionCalculator();
            decimal expectedTotal = 10m;

            var actualTotal = systemUnderTest.SplitBill(expectedTotal, 10).Sum();

            Assert.Equal(expectedTotal, actualTotal);
        }
    }
}

using System;
using System.Collections.Generic;

namespace BillManager.Core
{
    public class BillPortionCalculator : IBillPortionCalculator
    {
        public IEnumerable<decimal> SplitBill(decimal total, int count)
        {
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
            return SplitBillImplementation(total, count);
        }

        private IEnumerable<decimal> SplitBillImplementation(decimal total, int count)
        {
            var integerTotal = (long)(total * 100);
            var portion = integerTotal / count;
            var remainder = integerTotal % count;

            for (var x = 0; x < count; x++)
            {
                var amount = portion;
                if (remainder > 0)
                {
                    amount += 1;
                    remainder -= 1;
                }
                yield return (decimal)amount / 100m;
            }
        }
    }
}

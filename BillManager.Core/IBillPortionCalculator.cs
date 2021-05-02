using System.Collections.Generic;

namespace BillManager.Core
{
    public interface IBillPortionCalculator
    {
        IEnumerable<decimal> SplitBill(decimal total, int count);
    }
}

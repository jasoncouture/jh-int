using System.Collections.Generic;

namespace BillManager.Core
{
    public record BillSummaryModel
    {
        public BillModel Bill { get; init; }
        public IEnumerable<PersonBillModel> People { get; init; }
    }
}

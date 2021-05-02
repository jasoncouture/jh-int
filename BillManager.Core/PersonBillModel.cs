namespace BillManager.Core
{
    public record PersonBillModel
    {
        public long BillId { get; init; }
        public decimal Amount { get; init; }
    }
}

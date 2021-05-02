namespace BillManager.Core
{
    public record BillModel
    {
        public long Id { get; init; }
        public string Name { get; init; }
        public decimal Amount { get; init; }
    }
}

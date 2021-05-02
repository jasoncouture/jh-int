using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BillManager.Core
{
    public interface IBillRepository
    {
        IAsyncEnumerable<BillModel> GetBillsAsync(int? count, int? offset, CancellationToken cancellation = default);
        Task<BillSummaryModel> GetBillSummaryAsync(long id, CancellationToken cancellationToken);
        Task<BillSummaryModel> AddPeopleToBillAsync(long billId, long[] peopleIds, CancellationToken cancellationToken);
        Task<BillSummaryModel> UpdateBillAsync(long billId, BillModel billModel, CancellationToken cancellationToken);
        Task<IEnumerable<ErrorModel>> DeleteBillAsync(long billId, CancellationToken cancellationToken);
        Task<BillModel> CreateBillAsync(BillModel billModel, CancellationToken cancellationToken);
        Task<BillSummaryModel> RemovePeopleFromBillAsync(long billId, long[] peopleIds, CancellationToken cancellationToken);
    }
}

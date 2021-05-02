using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BillManager.Core
{

    public interface IPersonRepository
    {
        IAsyncEnumerable<PersonModel> GetPeople(int? count, int? offset, CancellationToken cancellationToken = default);
        Task<PersonModel> GetPersonAsync(long id, CancellationToken cancellationToken);
        Task<PersonModel> CreatePersonAsync(PersonModel person, CancellationToken cancellationToken);
        Task<PersonModel> UpdatePersonAsync(long id, PersonModel person, CancellationToken cancellationToken);
        Task<IEnumerable<ErrorModel>> DeletePersonAsync(long id, CancellationToken cancellationToken);
    }
}

using BillManager.Core.Database;
using BillManager.Core.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BillManager.Core
{
    public class PersonRepository : IPersonRepository
    {
        private readonly BillManagerDbContext _billManagerDbContext;

        public PersonRepository(BillManagerDbContext billManagerDbContext)
        {
            _billManagerDbContext = billManagerDbContext;
        }
        public async Task<PersonModel> CreatePersonAsync(PersonModel person, CancellationToken cancellationToken)
        {
            var result = await _billManagerDbContext.People.AddAsync(new Person()
            {
                Name = person.Name
            });

            await _billManagerDbContext.SaveChangesAsync(cancellationToken);

            await result.ReloadAsync(cancellationToken);

            return person with { Id = result.Entity.Id };
        }

        public async Task<IEnumerable<ErrorModel>> DeletePersonAsync(long id, CancellationToken cancellationToken)
        {
            using (var transaction = await _billManagerDbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken))
            {
                if (await _billManagerDbContext.PersonBillPortions.AnyAsync(i => i.PersonId == id, cancellationToken))
                {
                    return new[] { new ErrorModel("Please remove this person from all bills before attempting to delete them") };
                }
                var findResult = await _billManagerDbContext.People.FindAsync(new object[] { id }, cancellationToken);
                if (findResult == null) return Enumerable.Empty<ErrorModel>();
                _billManagerDbContext.Entry(findResult).State = EntityState.Deleted;
                await _billManagerDbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            return Enumerable.Empty<ErrorModel>();
        }

        public async IAsyncEnumerable<PersonModel> GetPeople(int? count, int? offset, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            IQueryable<Person> query = _billManagerDbContext.People;
            if (offset != null)
            {
                query = query.Skip(offset.Value);
            }
            if (count != null)
            {
                query = query.Take(count.Value);
            }
            IQueryable<PersonModel> personModelQuery = query.Select(i => new PersonModel
            {
                Id = i.Id,
                Name = i.Name
            });
            await foreach (var item in personModelQuery.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return item;
            }
        }

        public async Task<PersonModel> GetPersonAsync(long id, CancellationToken cancellationToken)
        {
            var dbModel = await _billManagerDbContext.People.FindAsync(new object[] { id }, cancellationToken);
            if (dbModel == null) return null;
            return new PersonModel
            {
                Id = dbModel.Id,
                Name = dbModel.Name
            };
        }

        public async Task<PersonModel> UpdatePersonAsync(long id, PersonModel person, CancellationToken cancellationToken)
        {
            var dbModel = await _billManagerDbContext.People.FindAsync(new object[] { id }, cancellationToken);
            if (dbModel == null) return null;
            dbModel.Name = person.Name;
            _billManagerDbContext.Entry(dbModel).DetectChanges();
            await _billManagerDbContext.SaveChangesAsync(cancellationToken);
            await _billManagerDbContext.Entry(dbModel).ReloadAsync(cancellationToken);

            return person with { Id = dbModel.Id, Name = dbModel.Name };
        }
    }
}

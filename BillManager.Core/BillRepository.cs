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
    public class BillRepository : IBillRepository
    {
        private readonly BillManagerDbContext _billManagerDbContext;
        private readonly IBillPortionCalculator _billPortionCalculator;

        public BillRepository(BillManagerDbContext billManagerDbContext, IBillPortionCalculator billPortionCalculator)
        {
            _billManagerDbContext = billManagerDbContext;
            _billPortionCalculator = billPortionCalculator;
        }
        public async Task<BillSummaryModel> AddPeopleToBillAsync(long billId, long[] people, CancellationToken cancellationToken)
        {
            using (var transaction = await _billManagerDbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead))
            {
                var billDbModel = await _billManagerDbContext.Bills.FindAsync(new object[] { billId }, cancellationToken);
                people = people.Distinct().ToArray();
                var peopleDbModels = await _billManagerDbContext.People.Where(i => people.Contains(i.Id)).ToListAsync();

                foreach (var person in peopleDbModels)
                {
                    var existingRecord = await _billManagerDbContext.PersonBillPortions.FindAsync(new object[] { billId, person.Id });
                    if (existingRecord != null) continue;
                    await _billManagerDbContext.PersonBillPortions.AddAsync(new PersonBillPortion()
                    {
                        BillId = billId,
                        PersonId = person.Id
                    });
                }

                if (await _billManagerDbContext.SaveChangesAsync(cancellationToken) != 0)
                {
                    _billManagerDbContext.ChangeTracker.Clear();
                    await RecomputeBillPortions(billId, cancellationToken);
                }
                await transaction.CommitAsync(cancellationToken);
            }

            return await GetBillSummaryAsync(billId, cancellationToken);
        }

        private async Task RecomputeBillPortions(long billId, CancellationToken cancellationToken)
        {
            var bill = await _billManagerDbContext.Bills.FindAsync(new object[] { billId }, cancellationToken);
            if (bill == null) return;
            // The order bys here make this deterministic, so that when the bill cannot be split evenly
            // The same people will be assigned the 0.01 extra.
            var people = await _billManagerDbContext.PersonBillPortions.Where(i => i.BillId == bill.Id).OrderByDescending(i => i.Amount).ThenBy(i => i.PersonId).ToListAsync(cancellationToken);
            if (people.Count == 0) return;
            var portions = _billPortionCalculator.SplitBill(bill.Total, people.Count).ToArray();
            for(var x = 0; x < people.Count; x++)
            {
                people[x].Amount = portions[x];
                _billManagerDbContext.Entry(people[x]).DetectChanges();
            }
            await _billManagerDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<BillModel> CreateBillAsync(BillModel billModel, CancellationToken cancellationToken)
        {
            var entry = await _billManagerDbContext.Bills.AddAsync(new Bill
            {
                Name = billModel.Name,
                Total = billModel.Amount
            });

            await _billManagerDbContext.SaveChangesAsync(cancellationToken);

            await entry .ReloadAsync(cancellationToken);
            return billModel with { Amount = entry.Entity.Total, Id = entry.Entity.Id, Name = entry.Entity.Name };
        }

        public async Task<IEnumerable<ErrorModel>> DeleteBillAsync(long billId, CancellationToken cancellationToken)
        {
            using (var transaction = await _billManagerDbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead))
            {
                var entry = await _billManagerDbContext.Bills.FindAsync(new object[] { billId }, cancellationToken);
                if (entry == null) return Enumerable.Empty<ErrorModel>();
                var personBillEntries = await _billManagerDbContext.PersonBillPortions.Where(i => i.BillId == billId).ToListAsync(cancellationToken);
                foreach(var personBillEntry in personBillEntries)
                {
                    _billManagerDbContext.Entry(personBillEntries).State = EntityState.Deleted;
                }
                await _billManagerDbContext.SaveChangesAsync(cancellationToken);
                _billManagerDbContext.Entry(entry).State = EntityState.Deleted;
                await _billManagerDbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            return Enumerable.Empty<ErrorModel>();
        }

        public async IAsyncEnumerable<BillModel> GetBillsAsync(int? count, int? offset, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            IQueryable<Bill> query = _billManagerDbContext.Bills.OrderBy(i => i.Id);
            if(offset != null && offset != 0)
            {
                query = query.Skip(offset.Value);
            }

            if(count != null && count != 0)
            {
                query = query.Take(count.Value);
            }

            await foreach(var billModel in query.Select(i => new BillModel { Amount = i.Total, Id = i.Id, Name = i.Name }).AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return billModel;
            }
        }

        public async Task<BillSummaryModel> GetBillSummaryAsync(long id, CancellationToken cancellationToken)
        {
            var billDbModel = await _billManagerDbContext.Bills.FindAsync(new object[] { id }, cancellationToken);
            if (billDbModel == null) return null;
            var billPeople = await _billManagerDbContext.PersonBillPortions.Where(i => i.BillId == billDbModel.Id).Select(i => new PersonBillModel
            {
                Amount = i.Amount,
                BillId = i.BillId
            }).ToListAsync(cancellationToken);

            return new BillSummaryModel
            {
                Bill = new BillModel
                {
                    Amount = billDbModel.Total,
                    Id = billDbModel.Id,
                    Name = billDbModel.Name
                },
                People = billPeople
            };
        }

        public async Task<BillSummaryModel> RemovePeopleFromBillAsync(long billId, long[] people, CancellationToken cancellationToken)
        {
            using (var transaction = await _billManagerDbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead))
            {
                var billDbModel = await _billManagerDbContext.Bills.FindAsync(new object[] { billId }, cancellationToken);
                var peopleList = people.Distinct().ToList();
                var peopleDbModels = await _billManagerDbContext.People.Where(i => peopleList.Contains(i.Id)).ToListAsync();

                foreach (var person in peopleDbModels)
                {
                    var existingRecord = await _billManagerDbContext.PersonBillPortions.FindAsync(new object[] { billId, person.Id });
                    if (existingRecord == null) continue;
                    _billManagerDbContext.Entry(existingRecord).State = EntityState.Deleted;
                }

                if (await _billManagerDbContext.SaveChangesAsync(cancellationToken) != 0)
                {
                    _billManagerDbContext.ChangeTracker.Clear();
                    await RecomputeBillPortions(billId, cancellationToken);
                }
                await transaction.CommitAsync(cancellationToken);
            }

            return await GetBillSummaryAsync(billId, cancellationToken);
        }

        public async Task<BillSummaryModel> UpdateBillAsync(long billId, BillModel billModel, CancellationToken cancellationToken)
        {
            var billDbModel = await _billManagerDbContext.Bills.FindAsync(new object[] { billId }, cancellationToken);
            if (billDbModel == null) return null;
            billDbModel.Name = billModel.Name;
            billDbModel.Total = billModel.Amount;
            _billManagerDbContext.Entry(billDbModel).DetectChanges();
            await _billManagerDbContext.SaveChangesAsync(cancellationToken);
            _billManagerDbContext.ChangeTracker.Clear();
            return await GetBillSummaryAsync(billId, cancellationToken);
        }
    }
}

using BillManager.Core;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BillManager.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BillController : ControllerBase
    {
        private readonly IBillRepository _billRepository;

        public BillController(IBillRepository billRepository)
        {
            _billRepository = billRepository;
        }

        [HttpGet]
        [Produces(typeof(IEnumerable<BillModel>))]
        public async Task<IActionResult> GetBillsAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            List<BillModel> bills = new();

            await foreach (var bill in _billRepository.GetBillsAsync(count, offset).WithCancellation(cancellationToken))
            {
                bills.Add(bill);
            }

            return Ok(bills);
        }

        [HttpGet("{id}")]
        [Produces(typeof(BillSummaryModel))]
        public async Task<IActionResult> GetBillSummary(long id, CancellationToken cancellationToken)
        {
            BillSummaryModel billSummary = await _billRepository.GetBillSummaryAsync(id, cancellationToken);
            if (billSummary == null) return NotFound();
            return Ok(billSummary);
        }

        [HttpPost("{billId}/people")]
        [Produces(typeof(BillSummaryModel))]
        public async Task<IActionResult> AddPeopleToBill(long billId, IEnumerable<PersonModel> people, CancellationToken cancellationToken)
        {
            BillSummaryModel billSummary = await _billRepository.AddPeopleToBillAsync(billId, people.Select(i => i.Id).ToArray(), cancellationToken);
            if (billSummary == null) return NotFound();
            return Ok(billSummary);
        }
        
        [HttpDelete("{billId}/people")]
        [Produces(typeof(BillSummaryModel))]
        public async Task<IActionResult> RemovePeopleFromBill(long billId, long[] people, CancellationToken cancellationToken)
        {
            BillSummaryModel billSummary = await _billRepository.RemovePeopleFromBillAsync(billId, people, cancellationToken);
            if (billSummary == null) return NotFound();
            return Ok(billSummary);
        }

        [HttpPost("{billId}")]
        [Produces(typeof(BillSummaryModel))]
        public async Task<IActionResult> UpdateBill(long billId, BillModel billModel, CancellationToken cancellationToken)
        {
            BillSummaryModel billSummary = await _billRepository.UpdateBillAsync(billId, billModel, cancellationToken);
            if (billSummary == null) return NotFound();
            return Ok(billSummary);
        }
        
        [HttpPost]
        [Produces(typeof(BillModel))]
        public async Task<IActionResult> CreateBill(BillModel billModel, CancellationToken cancellationToken)
        {
            billModel = await _billRepository.CreateBillAsync(billModel, cancellationToken);
            return Ok(billModel);
        }

        [HttpDelete("{billId}")]
        [Produces(typeof(IEnumerable<ErrorModel>))]
        public async Task<IActionResult> DeleteBill(long billId, CancellationToken cancellationToken)
        {
            IEnumerable<ErrorModel> errors = await _billRepository.DeleteBillAsync(billId, cancellationToken);
            if (errors.Any())
                return Ok(errors);
            return NoContent();
        }
    }
}

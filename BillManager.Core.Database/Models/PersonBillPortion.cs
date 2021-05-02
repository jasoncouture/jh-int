using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillManager.Core.Database.Models
{
    public class PersonBillPortion
    {
        public long PersonId { get; set; }
        [ForeignKey(nameof(PersonId))]
        public virtual Person Person { get; set; }
        public long BillId { get; set; }
        [ForeignKey(nameof(BillId)), Key]
        public virtual Bill Bill { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountPaid { get; set; }
       
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
    }
}

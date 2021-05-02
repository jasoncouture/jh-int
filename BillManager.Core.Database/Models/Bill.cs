using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillManager.Core.Database.Models
{
    public class Bill
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public virtual ICollection<PersonBillPortion> BillPortions { get; set; } = new List<PersonBillPortion>();
        public decimal Total { get; set; }
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
        public string Name { get; set; }
    }
}

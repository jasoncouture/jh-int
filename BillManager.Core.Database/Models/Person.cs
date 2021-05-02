using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillManager.Core.Database.Models
{
    public class Person
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        public virtual ICollection<PersonBillPortion> BillPortions { get; set; } = new List<PersonBillPortion>();
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
    }
}

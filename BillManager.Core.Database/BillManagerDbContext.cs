using BillManager.Core.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace BillManager.Core.Database
{
    public class BillManagerDbContext : DbContext
    {
        public BillManagerDbContext(DbContextOptions<BillManagerDbContext> dbContextOptions) : base(dbContextOptions) { }
        public DbSet<Person> People { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<PersonBillPortion> PersonBillPortions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersonBillPortion>(i => i.HasKey(x => new { x.PersonId, x.BillId }));
            base.OnModelCreating(modelBuilder);
        }
    }


}

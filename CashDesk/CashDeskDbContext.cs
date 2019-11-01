using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CashDesk
{
    public class CashDeskDbContext: DbContext
    {
        public DbSet<Member> Members { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Deposit> Deposits { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("Contract");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          
            // https://docs.microsoft.com/en-us/ef/core/saving/cascade-delete
            modelBuilder.Entity<Member>()
                .HasMany(m => m.Memberships)
                .WithOne(m => m.Member)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Membership>()
               .HasMany(m => m.Deposits)
               .WithOne(m => m.Membership)
               .OnDelete(DeleteBehavior.Cascade);

        }
    }
}

using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.SQL.Models;

namespace PersonalFinanceTracker.SQL
{
    public class PFTDbContext: DbContext
    {
        public DbSet<Income> Incomes { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Banks> Banks { get; set; }
        public DbSet<Expenses> Expenses { get; set; }
        public DbSet<PlaidUserItems> PlaidUserItems { get; set; }

        public PFTDbContext() { }

        public PFTDbContext(DbContextOptions<PFTDbContext> options)
          : base(options)
        {
        }
    }
}

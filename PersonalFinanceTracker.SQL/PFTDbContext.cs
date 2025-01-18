using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.SQL.Models;

namespace PersonalFinanceTracker.SQL
{
    public class PFTDbContext: DbContext
    {
        public DbSet<Income> Incomes { get; set; }

        public PFTDbContext() { }

        public PFTDbContext(DbContextOptions<PFTDbContext> options)
          : base(options)
        {
        }
    }
}

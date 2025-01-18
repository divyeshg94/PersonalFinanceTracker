using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Model
{
    public class ExpenseIn
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        public string Notes { get; set; }
        public float Amount { get; set; }
        public string PaidVia { get; set; }
        public string Category { get; set; }
        public string? CurrencyCode { get; set; }
        public DateTime Date { get; set; }
        public string? TransactionId { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public Guid UserId { get; set; }

        public Guid? BankId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Model
{
    public class IncomeOut
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Notes { get; set; }

        [Required]
        public float Amount { get; set; }

        public string Category { get; set; }

        public DateTime IncomeDate { get; set; }
        public bool IsExcludeTransaction { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? UpdatedDateTime { get; set; }

        public Guid UserId { get; set; }
        public Guid? BankId { get; set; }
        public string CurrencyCode { get; set; }
        public List<BankBasicInfo> Banks { get; set; }
        public List<string> UserCurrencies { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Model
{
    public class AddIncomeDetail
    {
        public List<BankBasicInfo> Banks { get; set; }
        public List<string> UserCurrencies { get; set; }
        public DateTime IncomeDate { get; set; }

        public string Name { get; set; }
        public string Notes { get; set; }
        public float Amount { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsExcludeTransaction { get; set; }
        public string Category { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public Guid UserId { get; set; }
        public Guid? BankId { get; set; }
    }

    public class AddExpenseDetail
    {
        public List<BankBasicInfo> Banks { get; set; }
        public DateTime Date { get; set; }

        public string Name { get; set; }

        public string Notes { get; set; }
        public long Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string PaidVia { get; set; }
        public string Category { get; set; }
        public bool IsExcludeTransaction { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public Guid UserId { get; set; }
        public Guid? BankId { get; set; }
        public List<string> UserCurrencies { get; set; }
    }
}

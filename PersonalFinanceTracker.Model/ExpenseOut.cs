using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.Model
{
    public class ExpenseOut
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
        public string Notes { get; set; }
        public float Amount { get; set; }
        public string PaidVia { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public bool IsExcludeTransaction { get; set; }
        public Guid UserId { get; set; }
        public Guid? BankId { get; set; }
        public string CurrencyCode { get; set; }
        public List<BankBasicInfo> Banks { get; set; }
        public List<string> UserCurrencies { get; set; }
    }
}

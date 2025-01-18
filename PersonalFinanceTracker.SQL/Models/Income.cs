using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceTracker.SQL.Models
{
    [Table("Income")]
    public class Income
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        public string? Notes { get; set; }
        private string _encryptedAmount;

        [Column("Amount")]
        [Encrypted]
        [MaxLength(500)] 
        public string EncryptedAmount
        {
            get => _encryptedAmount;
            set => _encryptedAmount = value;
        }

        [NotMapped]
        public float Amount
        {
            get
            {
                if (string.IsNullOrEmpty(_encryptedAmount)) return 0;
                if (float.TryParse(_encryptedAmount, out float amount))
                {
                    return amount;
                }
                return 0;
            }
            set
            {

            }
        }

        public string? CurrencyCode { get; set; }
        public string? Category { get; set; }
        public string? TransactionId { get; set; }
        public bool IsExcludeTransaction { get; set; }
        public DateTime IncomeDate { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }

        public Guid UserId { get; set; }

        public Guid? BankId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.SQL.Models
{
    [Table("Expenses")]
    public class Expenses
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        public string? Notes { get; set; }

        // Backing field for encrypted amount
        private string _encryptedAmount;

        // Encrypted amount stored in the database as string
        [Column("Amount")]
        [Encrypted]
        [MaxLength(500)]  // Adjust based on encryption size requirements
        public string EncryptedAmount
        {
            get => _encryptedAmount;
            set => _encryptedAmount = value;
        }

        // Virtual property to expose the decrypted amount as float
        [NotMapped]  // This property will not be mapped directly to the database
        public float Amount
        {
            get
            {
                if (string.IsNullOrEmpty(_encryptedAmount)) return 0;

                // Decrypt the value stored in the database (string) and convert to float
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
        public string? PaidVia { get; set; }
        public string? Category { get; set; }
        public DateTime Date { get; set; }
        public string? TransactionId { get; set; }
        public bool IsExcludeTransaction { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public Guid UserId { get; set; }

        public Guid? BankId { get; set; }

        public virtual Banks Bank { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual Users User { get; set; }

        public Expenses() { }


        public Expenses(Users user)
        {
            User = user;
        }
    }
}

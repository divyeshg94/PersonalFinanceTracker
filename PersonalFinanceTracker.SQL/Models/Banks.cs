using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Model.Helpers;

namespace PersonalFinanceTracker.SQL.Models
{
    [Table("Banks")]
    public class Banks
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Encrypted]
        public string? AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }
        public string? IFSC { get; set; }
        public string? AccountType { get; set; }
        public string? AccountSubType { get; set; }
        public string CurrencyCode { get; set; }
        [Encrypted]
        public string ClosingBalance { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }

        public Guid UserId { get; set; }
        public Guid? PlaidItemId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEncrypted { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual Users User { get; set; }

        public Banks()
        {
        }

        public Banks(Users user)
        {
            User = user;
        }

        public static BanksOut GetBankOut(Banks bank)
        {
            return DataMapper.Map<Banks, BanksOut>(bank);
        }

        public static Banks GetBank(BanksOut bank)
        {
            return DataMapper.Map<BanksOut, Banks>(bank);
        }
    }
}

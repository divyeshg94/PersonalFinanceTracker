using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceTracker.SQL.Models
{
    [Table("Users")]
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string UserUpn { get; set; }

        public string EmailId { get; set; }

        public string Currencies { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDateTime { get; set; }

        public Users()
        {
            CreatedDateTime = DateTime.UtcNow;
        }
    }
}

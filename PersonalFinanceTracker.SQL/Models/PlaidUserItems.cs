using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.SQL.Models
{
    [Table("PlaidUserItems")]
    public class PlaidUserItems
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [Encrypted]
        public string ItemId { get; set; }

        [Encrypted]
        public string AccessToken { get; set; }

        [Encrypted]
        public string? Cursor { get; set; }
        public DateTime? LastSyncDate { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsEncrypted { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual Users User { get; set; }

        public PlaidUserItems()
        {
        }
    }
}

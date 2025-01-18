using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Model
{
    public class BanksOut
    {
        public Guid Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }
        //public string AccountNumber { get; set; }
        //public string AccountHolderName { get; set; }
        //public string IFSC { get; set; }
        public string CurrencyCode { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }

        public Guid UserId { get; set; }
        public string ClosingBalance { get; set; }

        public List<string>? UserCurrencyCode { get; set; }

        public BanksOut()
        {
        }
    }
}

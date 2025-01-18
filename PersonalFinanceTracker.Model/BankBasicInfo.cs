using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Model
{
    public class BankBasicInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CurrencyCode { get; set; }

    }

    public class BankAccountInfo
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; }

    }
}

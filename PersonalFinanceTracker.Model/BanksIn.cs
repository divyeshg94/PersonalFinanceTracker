using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Model
{
    public class BanksIn
    {
        public string Name { get; set; }
        public string CurrencyCode { get; set; }
        public string ClosingBalance { get; set; }

        public BanksIn()
        {
        }
    }
}

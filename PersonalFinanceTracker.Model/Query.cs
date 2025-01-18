using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Model
{
    public class Query
    {
        public string Filter { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public string SortDirection { get; set; }
        public string SortProperty { get; set; }
    }
}

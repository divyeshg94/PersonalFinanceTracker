using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Model
{
    public class DashboardData
    {
        public float TotalIncome { get; set; }
        public float TotalAvailableBalance { get; set; }
        public float TotalExpense { get; set; }
        public float TotalInvestment { get; set; }
    }

    public class ChartData
    {
        public List<string> xValues { get; set; }
        public List<string> yValues { get; set; }
        public string TotalValue { get; set; }

        public ChartData()
        {
            xValues = new List<string>();
            yValues = new List<string>();
        }
    }

    public class IncomeExpenseChart
    {
        public List<string> xValues { get; set; }

        public List<string> IncomeData { get; set; }
        public List<string> ExpenseData { get; set; }

        public IncomeExpenseChart()
        {
            xValues = new List<string>();
            IncomeData = new List<string>();
            ExpenseData = new List<string>();
        }
    }

    public class MonthlyBreakdown
    {
        public float TotalIncome { get; set; }
        public ChartData Expenses { get; set; }
        public float NetSavings { get; set; }
    }

    public class TimeFrameData
    {
        public string TimeFrame { get; set; }
        public float Value { get; set; }
    }
}

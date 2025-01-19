using System.Globalization;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.SQL;
using PersonalFinanceTracker.SQL.Models;

namespace PersonalFinanceTracker.Service
{
    public class DashboardService
    {
        public PFTDbContext DbContext { get; set; }
        private UsersService _usersService;
        private IncomeService _incomeService;
        private ExpenseService _expenseService;
        private readonly BanksService _banksService;

        public DashboardService(PFTDbContext dbContext,
            IncomeService incomeService,
            ExpenseService expenseService,
            BanksService banksService,
            UsersService usersService)
        {
            _usersService = usersService;
            _expenseService = expenseService;
            _incomeService = incomeService;
            _banksService = banksService;
            DbContext = dbContext;
        }

        public async Task<DashboardData> GetDashboardData(Guid userId, DateTime? startDate = null, DateTime? endDate = null, string? incomeCategory = null, string? expenseCategory = null, string? paidVia = null)
        {
            var result = new DashboardData();
            var user = await _usersService.GetUser(userId);

            result.TotalIncome = await _incomeService.GetTotalIncomeAmount(user.Id, startDate, endDate, incomeCategory);
            result.TotalExpense = await _expenseService.GetTotalExpenseAmount(user.Id, startDate, endDate, expenseCategory, paidVia);
            result.TotalAvailableBalance = await _banksService.GetAvailableBalance(user.Id);

            return result;
        }

        public async Task<ChartData> GetCategoryWiseSpending(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var result = new ChartData();
            var user = await _usersService.GetUser(userId);

            var expenses = await _expenseService.GetExpensesByQuery(user.Id, startDate, endDate);
            var categories = expenses.GroupBy(e => e.Category).ToList();

            foreach (var category in categories)
            {
                result.xValues.Add(category.Key);
                result.yValues.Add(category.Sum(obj => obj.Amount).ToString());
            }

            result.TotalValue = expenses.Sum(obj => obj.Amount).ToString();
            return result;
        }

        public async Task<ChartData> GetPaymentWiseSpending(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var result = new ChartData();
            var user = await _usersService.GetUser(userId);

            var expenses = await _expenseService.GetExpensesByQuery(user.Id, startDate, endDate);
            var categories = expenses.GroupBy(e => e.PaidVia).ToList();

            foreach (var category in categories)
            {
                result.xValues.Add(category.Key);
                result.yValues.Add(category.Sum(obj => obj.Amount).ToString());
            }

            result.TotalValue = expenses.Sum(obj => obj.Amount).ToString();
            return result;
        }

        public async Task<MonthlyBreakdown> GetMonthlyBreakdown(Guid userId, int? month, int? year)
        {
            var result = new MonthlyBreakdown();
            if (month == null || month == 0 || year == null || year == 0)
            {
                month = DateTime.UtcNow.Month;
                year = DateTime.UtcNow.Year;
            }

            var startDate = new DateTime(year.Value, month.Value, 1);
            var endDate = startDate.AddMonths(1).AddSeconds(-1);

            result.TotalIncome = await _incomeService.GetTotalIncomeAmount(userId, startDate, endDate);
            result.Expenses = await GetCategoryWiseSpending(userId, startDate, endDate);
            result.NetSavings = result.TotalIncome - float.Parse(result.Expenses.TotalValue);
            return result;
        }

        public async Task<IncomeExpenseChart> GetIncomeExpenseComparison(Guid userId, DateTime? startDate = null, DateTime? endDate = null, string selectedTimeframe = "day")
        {
            var result = new IncomeExpenseChart();
            var user = await _usersService.GetUser(userId);

            var expenses = await _expenseService.GetExpensesByQuery(user.Id, startDate, endDate);

            // Group expenses by the appropriate timeframe (e.g., day, week, month, etc.)
            var expenseGrouped = expenses
                .Where(e => e.Date >= startDate && e.Date <= endDate) // Filter by the selected dates
                .GroupBy(e => GetTimeframe(e.Date, selectedTimeframe)) // Implement GetTimeframe method to group by timeframe
                .Select(g => GetTimeframeData(g));

            var incomes = await _incomeService.GetIncomeByQuery(user.Id, startDate, endDate);

            // Group incomes by the appropriate timeframe (e.g., day, week, month, etc.)
            var incomeGrouped = incomes
                .Where(i => i.IncomeDate >= startDate && i.IncomeDate <= endDate) // Filter by the selected dates
                .GroupBy(i => GetTimeframe(i.IncomeDate, selectedTimeframe)) // Implement GetTimeframe method to group by timeframe
                .Select(g => GetTimeframeData(g));


            var allDates = new HashSet<string>(incomeGrouped.Select(i => i.TimeFrame).Union(expenseGrouped.Select(e => e.TimeFrame))).OrderBy(d => d);
            foreach (var date in allDates.OrderBy(d => d))
            {
                var incomeEntry = incomeGrouped.FirstOrDefault(i => i.TimeFrame == date);
                if (incomeEntry != null)
                {
                    result.xValues.Add(date);
                    result.IncomeData.Add(incomeEntry.Value.ToString());
                }
                else
                {
                    result.xValues.Add(date);
                    result.IncomeData.Add("0");
                }

                var expenseEntry = expenseGrouped.FirstOrDefault(e => e.TimeFrame == date);
                if (expenseEntry != null)
                {
                    result.ExpenseData.Add(expenseEntry.Value.ToString());
                }
                else
                {
                    result.ExpenseData.Add("0");
                }
            }

            return result;
        }

        private TimeFrameData GetTimeframeData(IGrouping<string, Expenses> g)
        {
            DateOnly.TryParseExact(g.Key, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);
            return new TimeFrameData()
            {
                TimeFrame = g.Key,
                Value = g.Sum(obj => obj.Amount)
            };
        }

        private TimeFrameData GetTimeframeData(IGrouping<string, Income> g)
        {
            DateOnly.TryParseExact(g.Key, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);
            return new TimeFrameData()
            {
                TimeFrame = g.Key,
                Value = g.Sum(obj => obj.Amount)
            };
        }

        // Implement a method to determine the timeframe (e.g., day, week, month) based on the date
        private string GetTimeframe(DateTime date, string selectedTimeframe)
        {
            switch (selectedTimeframe.ToLower())
            {
                case "day":
                    return date.ToString("dd/MM/yyyy");
                case "week":
                    var firstDayOfWeek = date.AddDays(-(int)date.DayOfWeek).Date;
                    var lastDayOfWeek = firstDayOfWeek.AddDays(6).Date;
                    return $"{firstDayOfWeek.ToString("dd/MM/yyyy")} - {lastDayOfWeek.ToString("dd/MM/yyyy")}";
                case "month":
                    return date.ToString("MMMM yyyy");
                case "quarter":
                    var quarter = (date.Month - 1) / 3 + 1;
                    var quarterStart = new DateTime(date.Year, (quarter - 1) * 3 + 1, 1);
                    var quarterEnd = quarterStart.AddMonths(3).AddDays(-1);
                    return $"{quarterStart.ToString("MMMM yyyy")} - {quarterEnd.ToString("MMMM yyyy")}";
                case "half":
                    var halfYearStart = date.Month <= 6 ? new DateTime(date.Year, 1, 1) : new DateTime(date.Year, 7, 1);
                    var halfYearEnd = date.Month <= 6 ? new DateTime(date.Year, 6, 30) : new DateTime(date.Year, 12, 31);
                    return $"{halfYearStart.ToString("MMMM yyyy")} - {halfYearEnd.ToString("MMMM yyyy")}";
                case "year":
                    return date.ToString("yyyy");
                default:
                    return string.Empty; // Handle other cases or return an empty string for unknown input
            }
        }

    }
}

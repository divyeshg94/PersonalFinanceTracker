using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Model.Helpers;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Service;
using PersonalFinanceTracker.SQL;

namespace PersonalFinanceTracker.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UsersService _usersService;
        private readonly PFTDbContext _dbContext;
        private readonly DashboardService _dashboardService;

        public HomeController(ILogger<HomeController> logger, UsersService usersService, PFTDbContext dbContext, DashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
            _usersService = usersService;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index(string state, string code)
        {
            string userEmail = null;

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userClaims = User.Claims;
                var userClaimUserId = ((ClaimsIdentity)User.Identity).HasClaim(c => c.Type == "UserId");
                if (!userClaimUserId) //TODO: This is not working, Adding claims for every call
                {
                    userEmail = userClaims.FirstOrDefault(c => c.Type == "emails")?.Value;

                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        Callback(state, code);
                        var user = new SQL.Models.Users()
                        {
                            Currencies = JsonConvert.SerializeObject(new List<string>() { "USD" }),
                            CreatedDateTime = DateTime.UtcNow,
                            EmailId = userEmail,
                            UserUpn = userEmail
                        };
                        var addedUser = await _usersService.Add(user);

                        var newClaim = new Claim("User", JsonConvert.SerializeObject(addedUser));
                        var userIdClaim = new Claim("UserId", addedUser.Id.ToString());
                        ((ClaimsIdentity)User.Identity).AddClaim(newClaim);
                        ((ClaimsIdentity)User.Identity).AddClaim(userIdClaim);

                        var userCurrency = new Claim("UserCurrencies", addedUser.Currencies.ToString());
                        ((ClaimsIdentity)User.Identity).AddClaim(userCurrency);
                        ViewData["UserCurrency"] = addedUser.Currencies;
                    }
                }
            }

            var dateTime = DateHelper.GetDefaultStartEndTimeOfMonth();
            var userId = GetUserId(_dbContext);
            var dashboardData = await _dashboardService.GetDashboardData(userId, dateTime.Item1, dateTime.Item2);

            return View(dashboardData);
        }

        public async Task<DashboardData> GetDashboardData(DateTime startDate, DateTime endDate, string? incomeCategory = null, string? expenseCategory = null, string? paidVia = null)
        {
            var userId = GetUserId(_dbContext);
            return await _dashboardService.GetDashboardData(userId, startDate, endDate, incomeCategory, expenseCategory, paidVia);
        }

        [HttpGet]
        public async Task<ChartData> GetCategoryWiseSpendingChart(DateTime startDate, DateTime endDate)
        {
            var userId = GetUserId(_dbContext);
            return await _dashboardService.GetCategoryWiseSpending(userId, startDate, endDate);
        }

        [HttpGet]
        public async Task<ChartData> GetPaymentWiseSpendingChart(DateTime startDate, DateTime endDate)
        {
            var userId = GetUserId(_dbContext);
            return await _dashboardService.GetPaymentWiseSpending(userId, startDate, endDate);
        }

        [HttpGet]
        public async Task<IncomeExpenseChart> GetIncomeExpenseComparison(DateTime startDate, DateTime endDate, string timeFrame)
        {
            var userId = GetUserId(_dbContext);
            return await _dashboardService.GetIncomeExpenseComparison(userId, startDate, endDate, timeFrame);
        }


        [HttpGet]
        public async Task<MonthlyBreakdown> GetMonthlyBreakdown(int month)
        {
            var userId = GetUserId(_dbContext);
            var currentMonth = DateTime.UtcNow.Month;
            var year = DateTime.UtcNow.Year;
            if (currentMonth < month)
            {
                year = year - 1;
            }

            return await _dashboardService.GetMonthlyBreakdown(userId, month, year);
        }


        [AllowAnonymous]
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string state, string code)
        {
            if (string.IsNullOrEmpty(state) || string.IsNullOrEmpty(code))
            {
                return BadRequest("Invalid state or code");
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

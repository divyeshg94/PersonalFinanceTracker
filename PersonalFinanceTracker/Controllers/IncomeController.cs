using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Model.Helpers;
using PersonalFinanceTracker.Service;
using PersonalFinanceTracker.SQL;
using PersonalFinanceTracker.SQL.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace PersonalFinanceTracker.Controllers
{
    public class IncomeController : BaseController
    {
        private readonly PFTDbContext _dbContext;
        private readonly IncomeService _incomeService;

        public IncomeController(PFTDbContext dbContext, IncomeService incomeService)
        {
            _dbContext = dbContext;
            _incomeService = incomeService;
        }

        public async Task<IActionResult> Index()
        {
            var dateTime = DateHelper.GetDefaultStartEndTimeOfMonth();
            var userId = GetUserId(_dbContext);
            var incomes = GetIncome(userId, dateTime.Item1, dateTime.Item2, null);
            return View(incomes);
        }

        public async Task<PagedEntities<Income>> GetIncome(Guid? userId, DateTime? startDate, DateTime? endDate, PersonalFinanceTracker.Model.Query query)
        {
            if (!userId.HasValue)
                userId = GetUserId(_dbContext);

            ValidateUser(userId.Value);
            return await _incomeService.GetIncomes(userId.Value, startDate, endDate, query);
        }

        private void ValidateUser(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new Exception("Invalid User");

            var isUserExists = _dbContext.Users.Any(u => u.Id == userId);
            if (!isUserExists)
                throw new Exception("User not found");
        }
    }
}

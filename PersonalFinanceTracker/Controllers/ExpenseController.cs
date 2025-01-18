using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Service;
using PersonalFinanceTracker.SQL;
using PersonalFinanceTracker.SQL.Models;

namespace PersonalFinanceTracker.Controllers
{
    public class ExpenseController : BaseController
    {
        private readonly ILogger<ExpenseController> _logger;
        private readonly IConfiguration configuration;
        private readonly PFTDbContext _dbContext;
        private readonly BanksService _banksService;
        private readonly ExpenseService _expenseService;
        private readonly UsersService _usersService;
        private readonly IMapper _mapper;

        public ExpenseController(ILogger<ExpenseController> logger,
            BanksService banksService,
            PFTDbContext dbContext,
            IMapper mapper,
            IConfiguration configuration,
            ExpenseService expenseService,
            UsersService usersService)
        {
            _logger = logger;
            _banksService = banksService;
            _dbContext = dbContext;
            this.configuration=configuration;
            _expenseService=expenseService;
            _usersService=usersService;
            _mapper=mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<PagedEntities<Expenses>> GetExpense(Guid? userId, DateTime? startDate, DateTime? endDate, Query query)
        {
            if (!userId.HasValue)
                userId = GetUserId(_dbContext);

            return await _expenseService.GetExpenses(userId.Value, startDate, endDate, query);
        }

        public async Task<ActionResult> Create()
        {
            var userId = GetUserId(_dbContext);
            var userBanks = await _banksService.GetUserBanks(userId);
            var userCurrencies = await _usersService.GetUserCurrencyCodes(userId);
            return View(new AddExpenseDetail
            {
                UserCurrencies = userCurrencies,
                UserId = userId,
                Banks = userBanks,
                Date = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ExpenseIn expense)
        {
            if (!ModelState.IsValid)
            {
                ModelStateException(ModelState);
            }

            await _expenseService.AddExpense(expense);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            var expense = await _expenseService.Get(id);
            var expenseOut = _mapper.Map<ExpenseOut>(expense);
            expenseOut.Amount = expense.Amount;

            expenseOut.UpdatedDateTime = DateTime.UtcNow;
            var userId = GetUserId(_dbContext);

            var userBanks = await _banksService.GetUserBanks(userId);

            var userCurrencies = await _usersService.GetUserCurrencyCodes(userId);

            expenseOut.UserCurrencies = userCurrencies;
            expenseOut.Banks = userBanks;
            return View(expenseOut);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditExpense(ExpenseIn expenseOut)
        {
            if (!ModelState.IsValid)
            {
                ModelStateException(ModelState);
            }

            var userId = GetUserId(_dbContext);
            var expenses = _mapper.Map<Expenses>(expenseOut);
            expenses.EncryptedAmount = expenseOut.Amount.ToString();

            await _expenseService.Edit(expenses, userId);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            var expense = await _expenseService.Get(id);
            return View(expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new Exception("Invalid Input");
            }

            var userId = GetUserId(_dbContext);
            await _expenseService.Delete(id, userId);
            return RedirectToAction("Index");
        }
    }
}

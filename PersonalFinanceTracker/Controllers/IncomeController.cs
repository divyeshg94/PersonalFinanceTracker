using System.Diagnostics;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Model.Helpers;
using PersonalFinanceTracker.Models;
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
        private readonly BanksService _banksService;
        private readonly IMapper _mapper;
        private readonly UsersService _usersService;

        public IncomeController(PFTDbContext dbContext, IncomeService incomeService,
            IMapper mapper,
            UsersService usersService,
             BanksService banksService)
        {
            _dbContext = dbContext;
            _incomeService = incomeService;
            _banksService = banksService;
            _mapper = mapper;
            _usersService = usersService;
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

        public async Task<ActionResult> Add()
        {
            var userId = GetUserId(_dbContext);
            var userBanks = await _banksService.GetUserBanks(userId);

            return View(new AddIncomeDetail
            {
                UserCurrencies = await _usersService.GetUserCurrencyCodes(userId),
                Banks = userBanks,
                IncomeDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(IncomeIn incomeIn)
        {
            if (!ModelState.IsValid)
            {
                ModelStateException(ModelState);
            }

            var userId = GetUserId(_dbContext);
            incomeIn.UserId = userId;
            await _incomeService.AddIncome(incomeIn);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            var income = await _incomeService.Get(id);
            var incomeOut = _mapper.Map<IncomeOut>(income);
            incomeOut.Amount = income.Amount;
            incomeOut.UpdatedDateTime = DateTime.Now;

            var userId = GetUserId(_dbContext);
            var userBanks = await _banksService.GetUserBanks(userId);
            incomeOut.Banks = userBanks;

            incomeOut.UserCurrencies = await _usersService.GetUserCurrencyCodes(userId);

            return View(incomeOut);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditIncome(IncomeIn incomeOut)
        {
            if (!ModelState.IsValid)
            {
                ModelStateException(ModelState);
            }

            var income = _mapper.Map<Income>(incomeOut);
            income.EncryptedAmount = incomeOut.Amount.ToString();
            await _incomeService.Edit(income);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult DownloadExcel()
        {
            // Define the path to the file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "StatementSamplev1.xlsx");
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            string fileName = "StatementSamplev1.xlsx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            var income = await _incomeService.Get(id);
            return View(income);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new Exception("Invalid Input");
            }

            await _incomeService.Delete(id);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Details(Guid? id)
        {
            var income = await _incomeService.Get(id);
            return View(income);
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

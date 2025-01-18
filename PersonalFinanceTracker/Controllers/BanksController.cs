using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Service;
using PersonalFinanceTracker.SQL;
using PersonalFinanceTracker.SQL.Models;

namespace PersonalFinanceTracker.Controllers
{
    [Authorize]
    public class BankController : BaseController
    {
        private readonly ILogger<BankController> _logger;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private readonly IConfiguration configuration;
        private readonly PFTDbContext _dbContext;
        private readonly BanksService _banksService;
        private readonly UsersService _usersService;

        public BankController(ILogger<BankController> logger,
            PFTDbContext dbContext,
            UsersService usersService,
            BanksService banksService,
            Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, IConfiguration configuration)
        {
            _logger = logger;
            _banksService = banksService;
            _usersService = usersService;
            _dbContext = dbContext;
            _environment = environment;
            this.configuration=configuration;
        }

        public IActionResult Index(Guid? userId, Query query)
        {
            return View();
        }

        public async Task<PagedEntities<Banks>> GetBanks(Guid? userId, Query query)
        {
            if (!userId.HasValue)
                userId = GetUserId(_dbContext);

            return await _banksService.GetBanks(userId.Value, query);
        }

        public async Task<ActionResult> Add()
        {
            var banksOut = new BanksOut();
            var userId = GetUserId(_dbContext);

            banksOut.UserCurrencyCode = await _usersService.GetUserCurrencyCodes(userId);
            return View(banksOut);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(BanksIn bank)
        {
            if (!ModelState.IsValid)
            {
                ModelStateException(ModelState);
            }

            var userId = GetUserId(_dbContext);

            await _banksService.AddBanks(userId, bank);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (!id.HasValue)
                throw new Exception("Bank Id not given");

            var bank = await _banksService.Get(id.Value);
            var userId = GetUserId(_dbContext);

            var banksOut = Banks.GetBankOut(bank);
            banksOut.UserCurrencyCode = await _usersService.GetUserCurrencyCodes(userId);
            banksOut.UpdatedDateTime = DateTime.Now;
            return View(banksOut);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BanksOut banks)
        {
            if (!ModelState.IsValid)
            {
                ModelStateException(ModelState);
            }

            var bank = Banks.GetBank(banks);
            _banksService.Edit(bank);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            if (!id.HasValue)
                throw new Exception("Id is null");

            var bank = await _banksService.Get(id.Value);
            return View(bank);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteBank(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new Exception("Invalid Input");
            }

            _banksService.Delete(id);
            return RedirectToAction("Index");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Model;

namespace PersonalFinanceTracker.Controllers
{
    [Authorize]
    public class FireCalculatorController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration configuration;

        public FireCalculatorController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration=configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public FireResultModel Calculate([FromBody] FireInputModel input)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Invalid input!");
            }

            var result = CalculateFire(input);
            return result;
        }

        public FireResultModel CalculateFire(FireInputModel input)
        {
            var annualExpensesToday = input.MonthlyExpense * 12;
            var inflationFactor = (decimal)Math.Pow(1 + (double)input.InflationRate / 100, input.RetirementAge - input.CurrentAge);
            var annualExpensesAtRetirement = annualExpensesToday * inflationFactor;

            var result = new FireResultModel
            {
                CoastFire = CalculateCoastFire(input, annualExpensesAtRetirement),
                LeanFire = annualExpensesAtRetirement * 20,
                Fire = annualExpensesAtRetirement * 25,
                FatFire = annualExpensesAtRetirement * 50,
                ExpenseToday = annualExpensesToday,
                ExpenseAtRetirement = annualExpensesAtRetirement
            };

            return result;
        }

        private decimal CalculateCoastFire(FireInputModel input, decimal annualExpensesAtRetirement)
        {
            // Assuming Coast FIRE is the amount needed to grow without further savings until retirement
            var futureValueFactor = (decimal)Math.Pow(1 + (double)input.InflationRate / 100, input.RetirementAge - input.CoastFireAge);
            return annualExpensesAtRetirement / futureValueFactor;
        }
    }
}
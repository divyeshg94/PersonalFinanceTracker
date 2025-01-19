using Microsoft.AspNetCore.Mvc;

namespace PersonalFinanceTracker.Controllers
{
    public class ToolsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RetirementCalculator()
        {
            return View();
        }

        public IActionResult LoanCalculator()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace PersonalFinanceTracker.Controllers
{
    public class IncomeController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

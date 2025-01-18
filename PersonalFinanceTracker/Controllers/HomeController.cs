using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PersonalFinanceTracker.Model.Helpers;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Service;

namespace PersonalFinanceTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UsersService _usersService;

        public HomeController(ILogger<HomeController> logger, UsersService usersService)
        {
            _logger = logger;
            _usersService = usersService;
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
                            Currencies = JsonConvert.SerializeObject(new List<string>() { "INR", "USD" }),
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

            return View();
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

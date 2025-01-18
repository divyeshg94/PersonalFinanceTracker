using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PersonalFinanceTracker.SQL;

namespace PersonalFinanceTracker.Controllers
{
    public class BaseController : Controller
    {
        public PFTDbContext DbContext { get; set; }

        private object dblock = new object();

        public void ModelStateException(ModelStateDictionary modelState)
        {
            var errorMessages = modelState.Values
                                   .SelectMany(v => v.Errors)
                                   .Select(e => e.ErrorMessage)
                                   .ToList();

            var errorMessage = "Invalid ModelState. Errors: " + string.Join(", ", errorMessages);
            throw new Exception(errorMessage);
        }

        protected string UserCurrency => User.FindFirst("UserCurrencies")?.Value;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            ViewData["UserCurrencies"] = UserCurrency;
        }

        public Guid GetUserId(PFTDbContext dbContext)
        {
            var userClaimUserId = ((ClaimsIdentity)User.Identity).FindFirst(c => c.Type == "UserId")?.Value;

            if (string.IsNullOrEmpty(userClaimUserId))
            {
                var emailId = ((ClaimsIdentity)User.Identity).FindFirst(c => c.Type == "emails")?.Value;
                var user = dbContext.Users.SingleOrDefault(u => u.UserUpn == emailId);
                if (user == null)
                {
                    throw new Exception("User claim doesnot exists");
                }

                return user.Id;
            }

            Guid.TryParse(userClaimUserId, out var userId);
            return userId;
        }
    }
}

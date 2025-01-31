using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KaraokeApp.Filters
{
    public class AuthenticationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            var isLoginPage = context.ActionDescriptor.DisplayName.Contains("Login");
            var isSignUpPage = context.ActionDescriptor.DisplayName.Contains("SignUp");
            
            if (userId != null && (isLoginPage || isSignUpPage))
            {
                // Redirect to home if trying to access login/signup while authenticated
                context.Result = new RedirectToActionResult("HomePage", "Home", null);
                return;
            }
            
            if (userId == null && !isLoginPage && !isSignUpPage)
            {
                // Redirect to login if not authenticated
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
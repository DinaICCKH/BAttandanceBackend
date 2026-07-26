using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class SessionExpireFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var controller = context.RouteData.Values["Controller"]?.ToString();


        // Allow Login page
        if (controller == "Login")
        {
            return;
        }

        var token = context.HttpContext.Session
            .GetString("UserToken");


        if (string.IsNullOrEmpty(token))
        {
            context.Result = new RedirectToActionResult(
                "Index",
                "Login",
                null
            );
        }
    }


    public void OnActionExecuted(ActionExecutedContext context)
    {

    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class SessionExpireFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var path = context.HttpContext.Request.Path.Value?.ToLower() ?? string.Empty;

        // 1. Bypass all API routes so they return JSON instead of HTML redirects
        if (path.StartsWith("/api"))
        {
            return;
        }

        var controller = context.RouteData.Values["Controller"]?.ToString();

        // Allow Login page or controller
        if (controller == "Login" || controller == "App")
        {
            return;
        }

        var token = context.HttpContext.Session.GetString("UserToken");

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
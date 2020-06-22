using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SYE.Helpers.Extensions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PreventDuplicateRequestAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var formRequest = context.HttpContext.Request.Form;
            var session = context.HttpContext.Session;
            
            if (formRequest.ContainsKey("__RequestVerificationToken"))
            {
                var currentToken = formRequest["__RequestVerificationToken"].ToString();
                var lastToken = session.GetString("LastProcessedToken");

                if (lastToken == currentToken)
                    context.ModelState.AddModelError(string.Empty, "Looks like you accidentally submitted the same form twice.");

                session.SetString("LastProcessedToken", currentToken);
            }
        }
    }
}

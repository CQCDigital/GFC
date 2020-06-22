using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using SYE.ViewModels;

namespace SYE.Filters
{   
    public class HomeRedirectFilter : ActionFilterAttribute
    {
        private IOptions<ApplicationSettings> _config;

        public HomeRedirectFilter(IOptions<ApplicationSettings> config)
        {
            _config = config;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var redirectUrl = _config.Value.GFCUrls.RedirectUrl;
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                filterContext.Result = new RedirectResult(redirectUrl);
            }
        }
         
    }
}

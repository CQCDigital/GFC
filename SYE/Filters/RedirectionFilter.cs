using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using SYE.ViewModels;
using SYE.Services;

namespace SYE.Filters
{   
    public class RedirectionFilter : ActionFilterAttribute
    {
        private IOptions<ApplicationSettings> _config;
        private readonly ISessionService _session;

        public RedirectionFilter(IOptions<ApplicationSettings> config, ISessionService session)
        {
            _config = config;
            _session = session;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var redirectUrl = _config.Value.GFCUrls.RedirectUrl;
            var emptySession = string.IsNullOrEmpty(_session.GetRedirectionCookie());
            if (!string.IsNullOrEmpty(redirectUrl) && emptySession)
            {
               filterContext.HttpContext.Response.Redirect(redirectUrl);
            }
        }
         
    }
}

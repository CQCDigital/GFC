using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using SYE.Controllers;
using SYE.Helpers.Algorithm;
using SYE.Helpers.Enums;
using SYE.ViewModels;

namespace SYE.Filters
{
    public class CorsFilter : ActionFilterAttribute
    {
        public const string _gfcKeyName = "id";
        private readonly IOptions<ApplicationSettings> _config;
        private readonly IOptions<CQCRedirection> _corsConfig;

        public CorsFilter(IOptions<ApplicationSettings> config, IOptions<CQCRedirection> corsConfig)
        {
            _config = config;
            _corsConfig = corsConfig;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //Get the start page
            var startPage = _config.Value.GFCUrls.StartPage;

            //Get the controller
            var controller = (BaseController)filterContext.Controller;

            //Check the allowed CORS Domains
            var origin = GetOrigin(filterContext);

            //Get the allowed domains
            var allowedDomains = string.IsNullOrEmpty(_config.Value.AllowedCorsDomains)
                ? new string[] { }
                : _config.Value.AllowedCorsDomains.Split(',');

            //Check origin against allowed domains
            var isOriginAllowed = allowedDomains.Contains(origin.Host);


            //If origin is not allowed send user to error page
            if (!isOriginAllowed)
            {
                filterContext.Result = controller.GetCustomErrorCode(EnumStatusCode.CrossDomainOriginResourcesSharing,
                    $"Cross Origin Resources Sharing - Invalid Domain - {origin.Host}");
            }
            


            //Check the encrypted key in the form post
            var encKeyFromPost = filterContext.HttpContext.Request.Form[_gfcKeyName].FirstOrDefault();
            var keyFromPost = AesAlgorithm.Decrypt(_corsConfig.Value.GFCKey, encKeyFromPost);

            // If no key or invalid key, send user to error page
            if (string.IsNullOrEmpty(keyFromPost) || _corsConfig.Value.GFCPassword != keyFromPost)
            {
                filterContext.Result = controller.GetCustomErrorCode(EnumStatusCode.CrossDomainOriginResourcesSharing,
                    "Cross Origin Resources Sharing - Form Post key was invalid");
            }
            


            //We've passed our checks, add the headers to the response
            filterContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", $"{origin.Scheme}://{origin.Host}");
            filterContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "Origin, X-Requested-With, Content-Type, Accept" });
            filterContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", new[] { "POST, GET, OPTIONS" }); // new[] { "GET, POST, PUT, DELETE, OPTIONS" }
            
        }

        private static Uri GetOrigin(ActionContext authorizationFilterContext)
        {
            var context = authorizationFilterContext.HttpContext.Request;

            var originHeader = context.Headers["Referer"].FirstOrDefault() ?? context.Headers["Host"].FirstOrDefault();
            if (!string.IsNullOrEmpty(originHeader) && Uri.TryCreate(originHeader, UriKind.Absolute, out var origin))
                return origin;

            return null;
        }

    }
}

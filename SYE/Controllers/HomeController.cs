using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SYE.Helpers.Enums;
using SYE.Services;
using SYE.ViewModels;
using SYE.Filters;

namespace SYE.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<ApplicationSettings> _config;
        public ISessionService _sessionService { get; }

        private readonly ILocationService _locationService;

        public HomeController(ILogger<HomeController> logger, 
            IHttpContextAccessor httpContextAccessor, ISessionService sessionService, ILocationService locationService,
            IOptions<ApplicationSettings> config)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _sessionService = sessionService;
            _locationService = locationService;
            _config = config;
        }


        [HttpGet]
        [TypeFilter(typeof(HomeRedirectFilter))]
        public IActionResult Index()
        {  
            ViewBag.Title = _config.Value.SiteTextStrings.SiteTitle;
            ViewBag.HideSiteTitle = true;
            if (TempData.ContainsKey("search"))
                TempData.Remove("search");            
            return View();
        }

        [HttpPost, Route("website-redirect")]
        [TypeFilter(typeof(CorsFilter))]
        public IActionResult Index([FromForm] ProviderDetailsVM providerDetails)
        {          
            ViewBag.Title = _config.Value.SiteTextStrings.SiteTitle;
            ViewBag.HideSiteTitle = true;

            //Track that the user came from CQC.org.uk
            _sessionService.SetRedirectionCookie("true");

            if (!string.IsNullOrEmpty(providerDetails.LocationId) && !string.IsNullOrEmpty(providerDetails.ProviderId) && !string.IsNullOrEmpty(providerDetails.LocationName) && !string.IsNullOrEmpty(providerDetails.CookieAccepted))
            {
                _sessionService.SetCookieFlagOnSession(providerDetails.CookieAccepted.ToLower().Trim());

                var result = _locationService.GetByIdAsync(providerDetails.LocationId).Result;

                if (result == null)
                {
                    _logger.LogError("Error with CQC PayLoad; Provider Information not exist in the system", EnumStatusCode.CQCIntegrationPayLoadNotExist);
                   
                    return RedirectToAction("Index", "Search");
                }

                providerDetails.ProviderId = result.ProviderId;
                providerDetails.LocationName = result.LocationName;

                return RedirectToAction("SelectLocation", "Search", routeValues: providerDetails);               
            }
            else if (!string.IsNullOrEmpty(providerDetails.CookieAccepted))
            {
                _sessionService.SetCookieFlagOnSession(providerDetails.CookieAccepted.ToLower().Trim());
                return RedirectToAction("Index", "Search");
            }             
            else            
            {
                _logger.LogError("Error with CQC PayLoad null on the redirection post request", EnumStatusCode.CQCIntegrationPayLoadNullError);
                _sessionService.SetCookieFlagOnSession("false");
                return RedirectToAction("Index", "Search");
            }            
           
        }       
             
        [HttpGet, Route("website-redirect/{staticPage}/{cookieAccepted}")]
        public IActionResult Index(string staticPage, string cookieAccepted)
        {           
            ViewBag.Title = _config.Value.SiteTextStrings.SiteTitle;
            ViewBag.HideSiteTitle = true;

            //Track that the user came from CQC.org.uk
            _sessionService.SetRedirectionCookie("true");

            if (!string.IsNullOrEmpty(cookieAccepted))
            {
                _sessionService.SetCookieFlagOnSession(cookieAccepted.ToLower().Trim());
            }
            else
            {
                return GetCustomErrorCode(EnumStatusCode.CQCIntegrationPayLoadNullError, "Error with CQC Cookie PayLoad redirection");
            }

            switch (staticPage)
            {
                case "how-we-handle-information":
                    return RedirectToAction("Index", "HowWeUseYourInformation");
                case "accessibility":
                    return RedirectToAction("Index", "Accessibility");
                case "report-a-problem":
                    return RedirectToAction("Feedback", "Help");
                default:
                    break;
            }
            return RedirectToAction("Index", "Home");
        }

        [Route("set-version")]
        public IActionResult SetVersion(string v = "")
        {
            //Set the version for A/B testing
            //This will be used when we load the form
            ViewBag.HideSiteTitle = true;
            HttpContext.Session.SetString("FormVersion", v);
            return View("Index");
        }

        [Route("Clear-Data")]
        public IActionResult ClearData()
        {
            ControllerContext.HttpContext.Session.Clear();
            return new RedirectResult("/");
        }   
    }
}

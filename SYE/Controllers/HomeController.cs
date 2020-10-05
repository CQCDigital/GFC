using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GDSHelpers.Models.FormSchema;
using SYE.Helpers.Enums;
using SYE.Services;
using SYE.ViewModels;
using SYE.Filters;
using SYE.Models;

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
            var startPage = _config.Value.FormStartPage;

            if (!string.IsNullOrEmpty(providerDetails.LocationId) && !string.IsNullOrEmpty(providerDetails.ProviderId) && !string.IsNullOrEmpty(providerDetails.LocationName) && !string.IsNullOrEmpty(providerDetails.CookieAccepted))
            {
                _sessionService.SetCookieFlagOnSession(providerDetails.CookieAccepted.ToLower().Trim());

                var result = _locationService.GetByIdAsync(providerDetails.LocationId).Result;

                if (result == null)
                {
                    _logger.LogError("Error with CQC PayLoad; Provider Information not exist in the system", EnumStatusCode.CQCIntegrationPayLoadNotExist);

                    return RedirectToAction("StartQuestions", "Home");
                }

                providerDetails.ProviderId = result.ProviderId;
                providerDetails.LocationName = result.LocationName;

                return RedirectToAction("StartQuestions", "Home", routeValues: providerDetails);
            }
            else if (!string.IsNullOrEmpty(providerDetails.CookieAccepted))
            {
                _sessionService.SetCookieFlagOnSession(providerDetails.CookieAccepted.ToLower().Trim());
                return RedirectToAction("StartQuestions", "Home");
            }             
            else            
            {
                _logger.LogError("Error with CQC PayLoad null on the redirection post request", EnumStatusCode.CQCIntegrationPayLoadNullError);
                _sessionService.SetCookieFlagOnSession("false");
                return RedirectToAction("StartQuestions", "Home", routeValues: providerDetails);
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
                case "search":
                    return RedirectToAction("StartQuestions", "Home");
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
        [Route("home/start-questions")]
        public IActionResult StartQuestions(UserSessionVM vm)
        {
            //Make Sure we have a clean session
            _sessionService.ClearSession();

            _sessionService.SetLastPage("home/start-questions");

            try
            {
                var isFromCqc = false;
                if (string.IsNullOrWhiteSpace(vm.LocationName))
                {
                    vm.LocationName = _config.Value.SiteTextStrings.NonSelectedServiceName;
                }
                else
                {
                    //this location has been set at the CQC site
                    isFromCqc = true;
                }

                //Store the location we are giving feedback about
                _sessionService.SetUserSessionVars(vm);

                //Set up our replacement text
                var replacements = new Dictionary<string, string>
                {
                    {"!!location_name!!", vm.LocationName}
                };
                try
                {
                    //Load the Form and the search url into Session
                    var formVm = _sessionService.LoadLatestFormIntoSession(replacements);
                    //setup the definitive page order
                    var pageOrder = formVm.Pages.OrderBy(x => x.DisplayOrder).Select(p => p.PageId).ToList();
                    _sessionService.PageOrder = string.Join(",", pageOrder);
                    if (isFromCqc)
                    {
                        //make sure the search page can be accessed from check your answers
                        var searchPage = formVm.Pages.FirstOrDefault(p => p.PageId == "search");
                        if (searchPage != null) searchPage.Questions.FirstOrDefault().Answer = vm.LocationName;
                        _sessionService.SaveFormVmToSession(formVm);

                        //Add a flag to the form, and save location data
                        _sessionService.UpdateFormData(new DataItemVM()
                        {
                            Id = "LocationFromCqcFlag",
                            Value = "true"
                        });
                        _sessionService.UpdateFormData(new DataItemVM()
                        {
                            Id = "LocationName",
                            Value = vm.LocationName
                        });
                        _sessionService.UpdateFormData(new DataItemVM()
                        {
                            Id = "LocationId",
                            Value = vm.LocationId
                        });
                    }
                    var startPage = _config.Value.FormStartPage;
                    return RedirectToAction("Index", "Form", new { id = startPage, searchReferrer = "start-questions" });
                }
                catch (Exception ex)
                {
                    return GetCustomErrorCode(EnumStatusCode.SearchSelectLocationJsonError, "Error selecting location. json form not loaded");
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error start questions: " + vm.LocationName);
                throw ex;
            }
        }

    }
}

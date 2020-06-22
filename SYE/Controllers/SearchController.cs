using System;
using System.Collections.Generic;
using System.Linq;
using GDSHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SYE.Models;
using SYE.Services;
using SYE.ViewModels;
using SYE.Helpers;
using SYE.Helpers.Enums;
using SYE.Filters;

namespace SYE.Controllers
{
    public class SearchController : BaseController
    {
        private readonly int _pageSize = 20;
        private readonly int _maxSearchChars = 1000;
        private readonly int _minSearchChars = 1;
        private readonly ISearchService _searchService;
        private readonly ISessionService _sessionService;
        private readonly IOptions<ApplicationSettings> _config;
        private readonly IGdsValidation _gdsValidate;

        private static readonly HashSet<char> allowedChars = new HashSet<char>(@"1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.,'()?!#$£%^@*;:+=_-/ ");
        private static readonly List<string> restrictedWords = new List<string> { "javascript", "onblur", "onchange", "onfocus", "onfocusin", "onfocusout", "oninput", "onmouseenter", "onmouseleave",
            "onselect", "onclick", "ondblclick", "onkeydown", "onkeypress", "onkeyup", "onmousedown", "onmousemove", "onmouseout", "onmouseover", "onmouseup", "onscroll", "ontouchstart",
            "ontouchend", "ontouchmove", "ontouchcancel", "onwheel" };


        public SearchController(ISearchService searchService, ISessionService sessionService, IOptions<ApplicationSettings> config, IGdsValidation gdsValidate)
        {
            _searchService = searchService;
            _sessionService = sessionService;
            _config = config;
            _gdsValidate = gdsValidate;
            
        }


        [HttpGet]
        [TypeFilter(typeof(RedirectionFilter))]
        [Route("search/find-a-service")]
        public IActionResult Index()
        {
            //Make Sure we have a clean session
            _sessionService.ClearSession();

            _sessionService.SetLastPage("search/find-a-service");

            ViewBag.BackLink = new BackLinkVM { Show = true, Url = _config.Value.GFCUrls.StartPage, Text = _config.Value.SiteTextStrings.BackLinkText };

            ViewBag.title = "Find a service" + _config.Value.SiteTextStrings.SiteTitleSuffix;
            var vm = new SearchVm();
            return View(vm);
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(RedirectionFilter))]
        [Route("search/find-a-service")]
        public IActionResult Index(SearchVm vm)
        {
            _sessionService.SetLastPage("search/find-a-service");

            ViewBag.BackLink = new BackLinkVM { Show = true, Url = _config.Value.GFCUrls.StartPage, Text = _config.Value.SiteTextStrings.BackLinkText };

            //Make Sure we have a clean session
            _sessionService.ClearSession();

            if (!ModelState.IsValid)
            {
                ViewBag.title = $"Error: Find a service" + _config.Value.SiteTextStrings.SiteTitleSuffix;
                return View(vm);
            }
                

            var cleanSearch = _gdsValidate.CleanText(vm.SearchTerm, true, restrictedWords, allowedChars);
            if (string.IsNullOrEmpty(cleanSearch))
            {
                ModelState.AddModelError("SearchTerm", _config.Value.SiteTextStrings.EmptySearchError);
                ModelState.SetModelValue("SearchTerm", null, string.Empty);
                return View(vm);
            }
            
            return RedirectToAction(nameof(SearchResults), new { search = cleanSearch });
        }


        [HttpGet]
        [Route("search/results")]//searches
        public IActionResult SearchResults(string search, int pageNo = 1, string selectedFacets = "")
        {
            _sessionService.SetLastPage("search/results");

            var cleanSearch = _gdsValidate.CleanText(search, true, restrictedWords, allowedChars);

            var errorMessage = ValidateSearch(cleanSearch);
            if (errorMessage != null)
            {
                ViewBag.Title = $"Error: Find a service" + _config.Value.SiteTextStrings.SiteTitleSuffix;
                return GetSearchResult(cleanSearch, pageNo, selectedFacets, errorMessage);
            }

            ViewBag.Title = "Results for " + cleanSearch + _config.Value.SiteTextStrings.SiteTitleSuffix;
            return GetSearchResult(cleanSearch, pageNo, selectedFacets);
        }


        [HttpPost]
        [Route("search/results")]//applies the filter & does a search
        public IActionResult SearchResults(string search, List<SelectItem> facets = null, List<SelectItem> facetsModal = null)
        {
            _sessionService.SetLastPage("search/results");

            var cleanSearch = _gdsValidate.CleanText(search, true, restrictedWords, allowedChars);
            if (string.IsNullOrEmpty(cleanSearch))
                return RedirectToAction("Index", new { isError = "true"});

            var selectedFacets = string.Empty;
            if (facets != null && facetsModal != null)
            {
                //Logic to manage the sidebar and modal filters
                //If we have both facets and facetsModal something has gone wrong and we will not apply either filter
                if (facets.Count != 0 && facetsModal.Count == 0) //Sidebar filter
                    selectedFacets = string.Join(',', facets.Where(x => x.Selected).Select(x => x.Text).ToList());

                else if (facets.Count == 0 && facetsModal.Count != 0) //Modal filter
                    selectedFacets = string.Join(',', facetsModal.Where(x => x.Selected).Select(x => x.Text).ToList());
            }

            ViewBag.Title = "Results for " + cleanSearch + _config.Value.SiteTextStrings.SiteTitleSuffix;;

            return GetSearchResult(cleanSearch, 1, selectedFacets);
        }


        [HttpGet]
        public IActionResult LocationNotFound()
        {
            var defaultServiceName = _config.Value.SiteTextStrings.DefaultServiceName;

            try
            {
                //Store the user entered details
                _sessionService.SetUserSessionVars(new UserSessionVM { LocationId = "0", LocationName = defaultServiceName, ProviderId = "" });
                _sessionService.ClearNavOrder();
                //Set up our replacement text
                var replacements = new Dictionary<string, string>
                {
                    {"!!location_name!!", defaultServiceName}
                };

                try
                {
                    //Load the Form into Session
                    _sessionService.LoadLatestFormIntoSession(replacements);
                    var searchUrl = Request.Headers["Referer"].ToString();
                    _sessionService.SaveSearchUrl(searchUrl);
                }
                catch
                {
                    return GetCustomErrorCode(EnumStatusCode.SearchLocationNotFoundJsonError, "Error in location not found. json form not loaded");
                }

                var serviceNotFoundPage = _config.Value.ServiceNotFoundPage;
                return RedirectToAction("Index", "Form", new { id = serviceNotFoundPage });
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error in location not found.");
                throw ex;
            }
        }
       

        [Route("search/select-location")]
        public IActionResult SelectLocation(UserSessionVM vm)
        {
            _sessionService.SetLastPage("search/select-location");

            try
            {
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
                    _sessionService.LoadLatestFormIntoSession(replacements);
                    var searchUrl = Request.Headers["Referer"].ToString();
                    _sessionService.SaveSearchUrl(searchUrl);

                    var startPage = _config.Value.FormStartPage;
                    return RedirectToAction("Index", "Form", new { id = startPage, searchReferrer = "select-location" });
                }
                catch(Exception ex)
                {
                    return GetCustomErrorCode(EnumStatusCode.SearchSelectLocationJsonError, "Error selecting location. json form not loaded");
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error selecting location: '" + vm.LocationName + "'");
                throw ex;
            }
        }




        #region "Private Methods"

        private string ValidateSearch(string cleanSearch)
        {
            string errorMessage = null;
            if (string.IsNullOrEmpty(cleanSearch))
            {
                errorMessage = "Enter the name of a service, its address, postcode or a combination of these";
            }
            else
            {
                if (! (cleanSearch.Length <= _maxSearchChars && cleanSearch.Length >= _minSearchChars))
                {
                    errorMessage = $"Your search must be 1,000 characters or less";
                }
            }

            return errorMessage;
        }

        private IActionResult GetSearchResult(string search, int pageNo, string selectedFacets, string errorMessage = "")
        {
            //This is commented out as it is causing Facets to not work
            //Make Sure we have a clean session
            //_sessionService.ClearSession();

            var vm = new SearchResultsVM();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                vm.Data = new List<SearchResult>();
                vm.Facets = new List<SelectItem>();
                vm.FacetsModal = new List<SelectItem>();
                vm.ErrorMessage = errorMessage;
                return View(vm);
            }



            try
            {
                if (string.IsNullOrWhiteSpace(search))
                {
                    //reset search
                    //return RedirectToAction("Index", new { isError = true });

                    vm.ErrorMessage = "Enter the name of a service, its address, postcode or a combination of these";
                    return View(vm);

                }

                if (search.Length > _maxSearchChars)
                {
                    return View(new SearchResultsVM
                    {
                        Search = search,
                        ShowExceededMaxLengthMessage = true,
                        Facets = new List<SelectItem>(),
                        FacetsModal = new List<SelectItem>(),
                        Data = new List<SearchResult>()
                    });
                }

                var newSearch = SetNewSearch(search);

                var viewModel = GetViewModel(search, pageNo, selectedFacets, newSearch);
                if (viewModel == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.SearchUnavailableError,
                        "Search unavailable: Search string='" + search + "'");
                }
                
                ViewBag.BackLink = new BackLinkVM { Show = true, Url = Url.Action("Index", "Search"), Text = "Back" };

                TempData["search"] = search;

                if (viewModel.Count == 0)
                    viewModel.ErrorMessage = "There are no results matching your search";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error in search :'" + search + "'");
                throw ex;
            }
        }

        /// <summary>
        /// loads up the view model with paged data when there is a search string and page number
        /// otherwise it just returns a new view model with a show error flag
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageNo"></param>
        /// <param name="refinementFacets">comma separated list of selected facets to filter on</param>
        /// <returns></returns>
        private SearchResultsVM GetViewModel(string search, int pageNo, string refinementFacets, bool newSearch)
        {
            var returnViewModel = new SearchResultsVM();

            if (!string.IsNullOrEmpty(search) && pageNo > 0)
            {
                SearchServiceResult searchResult = null;
                try
                {
                    searchResult = _searchService.GetPaginatedResult(search, pageNo, _pageSize, refinementFacets, newSearch).Result;
                }
                catch (Exception ex)
                {
                    return null;//search is not working for some reason
                }                
                returnViewModel.Data = searchResult?.Data?.ToList() ?? new List<SearchResult>();
                returnViewModel.ShowResults = true;
                returnViewModel.Search = search;
                returnViewModel.PageSize = _pageSize;
                returnViewModel.Count = searchResult?.Count ?? 0;
                returnViewModel.Facets = SubmissionHelper.ConvertList(searchResult?.Facets);
                returnViewModel.FacetsModal = SubmissionHelper.ConvertList(searchResult?.Facets);
                returnViewModel.TypeOfService = searchResult?.Facets;
                returnViewModel.CurrentPage = pageNo;

                if (returnViewModel.Facets != null && (!string.IsNullOrEmpty(refinementFacets)) && !newSearch)
                {
                    foreach (var facet in returnViewModel.Facets)
                    {
                        facet.Selected = (refinementFacets.Contains(facet.Text));
                    }
                    foreach (var facetModal in returnViewModel.FacetsModal)
                    {
                        facetModal.Selected = (refinementFacets.Contains(facetModal.Text));
                    }
                }
            }

            return returnViewModel;
        }
        
        /// <summary>
        /// saves the search and checks saved search to see if it is a new search       
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        private bool SetNewSearch(string search)
        {
            bool newSearch = true;

            if (!string.IsNullOrEmpty(search))
            {
                var previousSearch = _sessionService.GetUserSearch();
                newSearch = !(search.Equals(previousSearch, StringComparison.CurrentCultureIgnoreCase));
                _sessionService.SaveUserSearch(search);
            }

            return newSearch;
        }


        #endregion "Private Methods"

    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.VariantTypes;
using GDSHelpers;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
        private const string _pageId = "search";
        private readonly int _pageSize = 20;
        private readonly int _maxSearchChars = 1000;
        private readonly int _minSearchChars = 1;
        private readonly ISearchService _searchService;
        private readonly ISessionService _sessionService;
        private readonly IOptions<ApplicationSettings> _config;
        private readonly IGdsValidation _gdsValidate;
        private readonly IPageHelper _pageHelper;
        private readonly IConfiguration _configuration;

        private static readonly HashSet<char> allowedChars = new HashSet<char>(@"1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.,'()?!#$£%^@*;:+=_-/ ");
        private static readonly List<string> restrictedWords = new List<string> { "javascript", "onblur", "onchange", "onfocus", "onfocusin", "onfocusout", "oninput", "onmouseenter", "onmouseleave",
            "onselect", "onclick", "ondblclick", "onkeydown", "onkeypress", "onkeyup", "onmousedown", "onmousemove", "onmouseout", "onmouseover", "onmouseup", "onscroll", "ontouchstart",
            "ontouchend", "ontouchmove", "ontouchcancel", "onwheel" };


        public SearchController(ISearchService searchService, ISessionService sessionService, IOptions<ApplicationSettings> config, IGdsValidation gdsValidate, IPageHelper pageHelper, IConfiguration configuration)
        {
            _searchService = searchService;
            _sessionService = sessionService;
            _config = config;
            _gdsValidate = gdsValidate;
            _pageHelper = pageHelper;
            _configuration = configuration;
        }


        [HttpGet]
        [TypeFilter(typeof(RedirectionFilter))]
        [Route("search/find-a-service")]
        public IActionResult Index()
        {
            var lastPage = _sessionService.GetLastPage();

            if (lastPage != null && lastPage.Contains("you-have-sent-your-feedback"))
            {
                return GetCustomErrorCode(EnumStatusCode.FormPageAlreadySubmittedError, "Error with user action. Feedback already submitted");
            }

            var formVm = _sessionService.GetFormVmFromSession();
            if (formVm == null)
            {
                //clicking on old link or back button from submit does this
                return GetCustomErrorCode(EnumStatusCode.CYAFormNullError, "Error with user session. formVm is null.");
            }
            if ((_sessionService.GetUserSession().LocationName) == null)
            {
                return GetCustomErrorCode(EnumStatusCode.CYALocationNullError, "Error with user session. Location is null.");
            }
            var pageVm = formVm.Pages.FirstOrDefault(p => p.PageId == _pageId);
            var serviceNotFoundPage = _config.Value.ServiceNotFoundPage;
            var formStartPage = _config.Value.FormStartPage;

            if (!_pageHelper.CheckPageHistory(pageVm, lastPage, false, _sessionService, null, serviceNotFoundPage, formStartPage, true))
            {
                //user jumps between pages
                return GetCustomErrorCode(EnumStatusCode.CYASubmissionHistoryError, "Error with user submission. Page history not found: Id='" + _pageId + "'");
            }
            _sessionService.SetLastPage("search/find-a-service");

            ViewBag.BackLink = new BackLinkVM { Show = true, Url = Url.Action("Index", "Form", new { id = _config.Value.FormStartPage }), Text = _config.Value.SiteTextStrings.BackLinkText };
            
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
        [TypeFilter(typeof(RedirectionFilter))]
        public IActionResult SearchResults(string search, int pageNo = 1, string selectedFacets = "")
        {
            var lastPage = _sessionService.GetLastPage();

            if (lastPage != null && lastPage.Contains("you-have-sent-your-feedback"))
            {
                return GetCustomErrorCode(EnumStatusCode.FormPageAlreadySubmittedError, "Error with user action. Feedback already submitted");
            }

            var formVm = _sessionService.GetFormVmFromSession();
            if (formVm == null)
            {
                //clicking on old link or back button from submit does this
                return GetCustomErrorCode(EnumStatusCode.CYAFormNullError, "Error with user session. formVm is null.");
            }
            if ((_sessionService.GetUserSession().LocationName) == null)
            {
                return GetCustomErrorCode(EnumStatusCode.CYALocationNullError, "Error with user session. Location is null.");
            }
            var pageVm = formVm.Pages.FirstOrDefault(p => p.PageId == _pageId);
            var serviceNotFoundPage = _config.Value.ServiceNotFoundPage;
            var formStartPage = _config.Value.FormStartPage;

            if (!_pageHelper.CheckPageHistory(pageVm, lastPage, false, _sessionService, null, serviceNotFoundPage, formStartPage, true))
            {
                //user jumps between pages
                return GetCustomErrorCode(EnumStatusCode.CYASubmissionHistoryError, "Error with user submission. Page history not found: Id='" + _pageId + "'");
            }

            var refererIsCheckYourAnswers = ((lastPage ?? "").Contains(_config.Value.SiteTextStrings.ReviewPage) || _sessionService.ChangedLocationMode == true);
            if (refererIsCheckYourAnswers)
            {
                //comes from check your answers
                _sessionService.SaveChangeMode(_config.Value.SiteTextStrings.ReviewPageId);
                //leave last page as check your answers
            }
            else
            {
                _sessionService.ClearChangeMode();
                _sessionService.SetLastPage("search/results");
            }

            var cleanSearch = _gdsValidate.CleanText(search, true, restrictedWords, allowedChars);

            var errorMessage = ValidateSearch(cleanSearch);
            if (errorMessage != null)
            {
                ViewBag.Title = $"Error: Find a service" + _config.Value.SiteTextStrings.SiteTitleSuffix;
                return GetSearchResult(cleanSearch, pageNo, selectedFacets, refererIsCheckYourAnswers, errorMessage);
            }

            ViewBag.Title = "Results for " + cleanSearch + _config.Value.SiteTextStrings.SiteTitleSuffix;
            return GetSearchResult(cleanSearch, pageNo, selectedFacets, refererIsCheckYourAnswers);
        }


        [HttpPost]
        [Route("search/results")]//applies the filter & does a search
        [TypeFilter(typeof(RedirectionFilter))]
        public IActionResult ApplyFilter(string search, List<SelectItem> facets = null, List<SelectItem> facetsModal = null, string checkboxClicked = null)
        {
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

            if (checkboxClicked != null)
            {
                _sessionService.CheckboxClick = checkboxClicked;
            }

            return RedirectToAction(nameof(SearchResults), new { search = search, pageno = 1, selectedFacets = selectedFacets });
        }


        [HttpGet]
        public IActionResult LocationNotFound()
        {
            try
            {
                var serviceNotFoundPage = _config.Value.ServiceNotFoundPage;

                //Requesting location not found implies user may be shown exactly-where question later:
                _sessionService.UpdateFormData(new List<DataItemVM>()
                {
                    new DataItemVM() {Id = "skippedExactLocationFlag", Value = "False"}
                });

                if ((_sessionService.GetChangeMode() ?? "") == _config.Value.SiteTextStrings.ReviewPageId)
                {
                    //this happens when a user changes from a selected location to a location not found
                    _sessionService.ChangedLocationMode = true;

                    return RedirectToAction("Index", "Form", new { id = serviceNotFoundPage });
                }

                //get the next page from the start page answer
                var formVm = _sessionService.GetFormVmFromSession();
                //get the previous location name to replace
                var previousLocation = _sessionService.GetUserSession().LocationName;
                var defaultServiceName = _config.Value.SiteTextStrings.DefaultServiceName;

                //Store the user entered details
                _sessionService.SetUserSessionVars(new UserSessionVM { LocationId = "0", LocationName = defaultServiceName, ProviderId = "" });
                //Set up our replacement text
                var replacements = new Dictionary<string, string>
                {
                    {previousLocation, defaultServiceName}
                };

                try
                {
                    _sessionService.SaveFormVmToSession(formVm, replacements);
                    var searchUrl = Request.Headers["Referer"].ToString();
                    _sessionService.SaveSearchUrl(searchUrl);
                }
                catch
                {
                    return GetCustomErrorCode(EnumStatusCode.SearchLocationNotFoundJsonError, "Error in location not found. json form not loaded");
                }

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
            //get the previous location name to replace
            var previousLocation = _sessionService.GetUserSession().LocationName;
            //get the next page from the start page answer
            var formVm = _sessionService.GetFormVmFromSession();
            var startPageId = _config.Value.FormStartPage;
            var startPage = formVm.Pages.FirstOrDefault(p => p.PageId == startPageId);

            _sessionService.SetLastPage("search/select-location");

            try
            {
                //Lets clean our vm just in case anyone has tried to inject anything
                vm.LocationName = _gdsValidate.CleanText(vm.LocationName, true);
                vm.LocationId = _gdsValidate.CleanText(vm.LocationId, true);
                vm.ProviderId = _gdsValidate.CleanText(vm.ProviderId, true);


                //Store the location we are giving feedback about
                _sessionService.SetUserSessionVars(vm);
                //put the location into the answer
                var searchPage = formVm.Pages.FirstOrDefault(p => p.PageId == "search");
                if (searchPage != null) searchPage.Questions.FirstOrDefault().Answer = vm.LocationName;

                //Set up our replacement text
                var replacements = new Dictionary<string, string>
                {
                    {previousLocation, vm.LocationName}
                };
                try
                {
                    _sessionService.SaveFormVmToSession(formVm, replacements);

                    //Save location data to form
                    _sessionService.UpdateFormData(new List<DataItemVM>()
                    {
                        new DataItemVM() { Id = "LocationName", Value = vm.LocationName },
                        new DataItemVM() { Id = "LocationId", Value = vm.LocationId },
                        new DataItemVM() { Id = "LocationCategory", Value = vm.LocationCategory }
                    });

                    var searchUrl = Request.Headers["Referer"].ToString();
                    _sessionService.SaveSearchUrl(searchUrl);

                    var nextId = _pageHelper.GetNextPageIdFromPage(formVm, startPage.PageId);

                    //set up variables for skipping the where question and navigation to check-your-answers
                    var showExactlyWhere = _config.Value.SiteTextStrings.CategoriesForExactlyWhereQuestion;
                    var goodBadFeedbackQuestion = _config.Value.QuestionStrings.GoodBadFeedbackQuestion.id;
                    var goodExperienceAnswer = _config.Value.QuestionStrings.GoodBadFeedbackQuestion.GoodFeedbackAnswer;
                    var whereItHappened = _config.Value.PageIdStrings?.WhereItHappenedPage;
                    var whenItHappened = _config.Value.PageIdStrings?.WhenItHappenedPage;


                    var goodJourney = formVm.GetQuestion(goodBadFeedbackQuestion).Answer == goodExperienceAnswer;

                    var skipWhere = (!string.IsNullOrEmpty(vm.LocationCategory)
                                     && !showExactlyWhere.Any(category => vm.LocationCategory.Contains(category)));

                    //If skipWhere is true, set a flag that will ensure that attempts to load exactly-where will instead load exactly-when
                    _sessionService.UpdateFormData(new List<DataItemVM>()
                    {
                        new DataItemVM() {Id = "skippedExactLocationFlag", Value = skipWhere.ToString()}
                    });

                    //remove any previously entered location not found
                    _sessionService.RemoveFromNavOrder(_config.Value.ServiceNotFoundPage);

                    //If user came from check-your-answers
                    if ((_sessionService.GetChangeMode() ?? "") == _config.Value.SiteTextStrings.ReviewPageId)
                    {
                        //We want to send the user back to CYA unless: they previously skipped the 'where' question and now need to answer it
                        var returnToCya = true;
                        if (skipWhere || goodJourney)
                        {
                            //User should still skip where, so make sure where question is removed from the nav order
                            _sessionService.RemoveFromNavOrder(whereItHappened);
                        }
                        else
                        {
                            //User should have visited the 'where' question => Check if it's been visited/answered before returning to CYA
                            var wherePage = formVm.Pages.FirstOrDefault(p => p.PageId == whereItHappened);
                            
                            //If user has answered 'where', ensure that it's in the nav order
                            if (wherePage != null && wherePage.Questions.All(x => !string.IsNullOrWhiteSpace(x.Answer)) )
                            {
                                _sessionService.UpdateNavOrder(whereItHappened);
                            }

                            var wherePageInNavOrder = _sessionService.GetNavOrder().Contains(whereItHappened);
                            if (!wherePageInNavOrder)
                            {
                                //This journey doesn't include 'where' but does require it, so update the redirectId to trigger on 'when' and skip the return to CYA for now
                                returnToCya = false;
                                _sessionService.SaveChangeModeRedirectId(whenItHappened);
                            }
                        }

                        if (returnToCya)
                        {
                            _sessionService.ClearChangeMode();
                            return RedirectToAction("Index", "CheckYourAnswers");
                        }
                    }

                    _sessionService.UpdateNavOrder("search");
                    
                    return RedirectToAction("Index", "Form", new { id = nextId, searchReferrer = "select-location" });

                }
                catch (Exception ex)
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

        private IActionResult GetSearchResult(string search, int pageNo, string selectedFacets, bool refererIsCheckYourAnswers, string errorMessage = "")
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
                if (!string.IsNullOrWhiteSpace(search) && search.Length > _maxSearchChars)
                {
                    vm.ShowExceededMaxLengthMessage = true;
                }
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

                ViewBag.BackLink = new BackLinkVM { Show = true, Url = Url.Action("Index", refererIsCheckYourAnswers ? "CheckYourAnswers" : "Search"), Text = "Back" };

                TempData["search"] = search;

                if (viewModel.Count == 0)
                    viewModel.ErrorMessage = "There are no results matching your search";

                //New bit, to get the right selected checkbox
                if (!string.IsNullOrWhiteSpace(_sessionService.CheckboxClick))
                {
                    var selectedCheckbox = viewModel.Facets.FindIndex(f => f.Text == _sessionService.CheckboxClick).ToString();

                    ViewBag.SelectedCheckbox = $"Facets_{selectedCheckbox}__Selected";
                    _sessionService.CheckboxClick = "";
                }
                
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
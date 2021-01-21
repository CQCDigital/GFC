using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Text;
using DocumentFormat.OpenXml;
using GDSHelpers;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SYE.Helpers;
using SYE.Helpers.Enums;
using SYE.Models;
using SYE.Services;
using SYE.ViewModels;

namespace SYE.Controllers
{
    public class FormController : BaseController
    {
        private readonly IGdsValidation _gdsValidate;
        private readonly ISessionService _sessionService;
        private readonly IOptions<ApplicationSettings> _config;
        private readonly IPageHelper _pageHelper;
        
        public FormController(IGdsValidation gdsValidate, ISessionService sessionService, IOptions<ApplicationSettings> config, ILogger<FormController> logger, IPageHelper pageHelper)
        {
            _gdsValidate = gdsValidate;
            _sessionService = sessionService;
            _config = config;
            _pageHelper = pageHelper;
        }

        [HttpGet("form/{id}")]
        public IActionResult Index(string id = "", string searchReferrer = "")
        {
            //First, check for specific case: If this is 'exactly where' page and location choice means that this should be skipped, redirect user to 'exactly-when' instead
            var whereItHappened = _config.Value.PageIdStrings?.WhereItHappenedPage;
            var whenItHappened = _config.Value.PageIdStrings?.WhenItHappenedPage;
            var skippedWhereFlag = _sessionService.GetFormData("skippedExactLocationFlag")?.Value;

            if (id == whereItHappened && skippedWhereFlag == "True")
            {
                return RedirectToAction("Index", "Form", new { id = whenItHappened, searchReferrer = "select-location" });
            }

            //Now check whether user has already submitted the form
            var lastPage = _sessionService.GetLastPage();
            if (lastPage != null && lastPage.Contains("you-have-sent-your-feedback"))
            {
                return GetCustomErrorCode(EnumStatusCode.FormPageAlreadySubmittedError, "Error with user action. Feedback already submitted");
            }

            //this next piece of code determines if the user came to 'what-you-want-to-tell-us-about' from when it happened and pressed the back button
            //this happens if a user comes from cqc having selected a location
            //if so then the user skipped the search which would normally clear the change mode
            if (lastPage != null && lastPage.Contains(whereItHappened) && id == _config.Value.FormStartPage)
            {
                
                //If user picked a service on CQC site, search cannot have been the previous page ==> go to next previousPage
                var fromCqc = _sessionService.GetFormData("LocationFromCqcFlag").Value;
                if (fromCqc != null)
                {
                    _sessionService.ClearChangeMode();
                }
            }

            _sessionService.SetLastPage($"form/{id}");

            try
            {
                //Check for null session or null location errors
                var userSession = _sessionService.GetUserSession();
                if (userSession == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.FormPageLoadSessionNullError, "Error with user session. Session is null: Id='" + id + "'");
                }

                if (userSession.LocationName == null)
                {
                    if (string.IsNullOrWhiteSpace(lastPage))
                    {
                        //probably an old link
                        return GetCustomErrorCode(EnumStatusCode.FormPageLoadLocationNullError, "Error with user session. Location is null: Id='" + id + "'");
                    }
                    else
                    {
                        //example if user jumps from find a service straight into a question
                        return GetCustomErrorCode(EnumStatusCode.FormPageLoadHistoryError, "Error with user session. Location is null: Id='" + id + "'");
                    }
                }

                //Set flag to true if locationName is the default service name
                var serviceNotFound = userSession.LocationName.Equals(_config.Value.SiteTextStrings.DefaultServiceName);

                PageVM pageVm = null;

                //Populate the pageVM from the session
                try
                {
                    //if we've got this far then the session is ok
                    //if there an exception then the json cant be found
                    pageVm = _sessionService.GetPageById(id, serviceNotFound);
                }
                catch
                {
                    return GetCustomErrorCode(EnumStatusCode.FormPageLoadJsonError, "Error with json file: Id='" + id + "'");
                }
                if (pageVm == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.FormPageLoadNullError, "Error with user session. pageVm is null: Id='" + id + "'");
                }

                if (!_pageHelper.CheckPageHistory(pageVm, lastPage ?? searchReferrer, false, _sessionService, _config.Value.ExternalStartPage, _config.Value.ServiceNotFoundPage, _config.Value.FormStartPage, serviceNotFound))
                {
                    //user jumps between pages
                    return GetCustomErrorCode(EnumStatusCode.FormPageLoadHistoryError, "Error with page load. Page history not found: Id='" + id + "'");
                }

                //Set flag to true if the user came from check your answers
                var refererIsCheckYourAnswers = (lastPage ?? "").Contains(_config.Value.SiteTextStrings.ReviewPage);

                //Create a backLink
                ViewBag.BackLink = new BackLinkVM
                {
                    Show = true, 
                    Url =  refererIsCheckYourAnswers ? 
                        _config.Value.SiteTextStrings.ReviewPage : 
                        _pageHelper.GetPreviousPage(pageVm, _sessionService, _config, Url, serviceNotFound), 
                    Text = _config.Value.SiteTextStrings.BackLinkText
                };

                //Update the users journey                
                if (!string.IsNullOrWhiteSpace(_sessionService.GetChangeModeRedirectId()) && pageVm.NextPageId != _config.Value.SiteTextStrings.ReviewPageId)
                {
                    _sessionService.UpdateNavOrderAtRedirectTrigger(pageVm.PageId, _sessionService.GetChangeModeRedirectId());
                }
                else
                {                    
                    if (refererIsCheckYourAnswers)
                    {
                        //comes from check your answers
                        _sessionService.SaveChangeMode(_config.Value.SiteTextStrings.ReviewPageId);
                    }
                    else
                    {
                        _sessionService.ClearChangeMode();
                    }

                    var serviceNotFoundPageId = _config.Value.ServiceNotFoundPage;
                    _sessionService.UpdateNavOrder(pageVm.PageId, serviceNotFoundPageId);
                }                

                //Look for dynamic content, and update the page with dynamic content if it exists:
                var formContext = _sessionService.GetFormVmFromSession();
                pageVm.HandleDynamicContent(formContext);


                //Add Paging to the current page -- currently disabled
                //SetUpPaging(formContext, id);


                ViewBag.Title = pageVm.PageTitle + _config.Value.SiteTextStrings.SiteTitleSuffix;
                return View(pageVm);
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error loading form: Id='" + id + "'");
                throw ex;
            }
        }

        // -- Currently disabled, pending additional development
        //private void SetUpPaging(FormVM formContext, string id)
        //{
        //    var pages = formContext.Pages
        //             .Where(m => m.Questions.Count() > 0 && !m.RemoveFromSubmission)
        //             .Select(m => m.PageId)
        //             .ToList();
        //
        //    pages.Remove("tell-us-which-service");
        //
        //    var position = pages.IndexOf(id) + 1;
        //
        //    ViewBag.PageCount = pages.Count();
        //    ViewBag.Position = position;
        //    ViewBag.HidePageCounter = false;
        //
        //    var hideForJourney = _config.Value.Paging.StartPage;
        //    var hideForJourneyAnswer = _config.Value.Paging.HidePagingIfStartPageEqauls;
        //    var showFromPageNo = _config.Value.Paging.StartPagingAtPageNo;
        //
        //    var questions = formContext.Pages.SelectMany(m => m.Questions).ToList();
        //    var hidePagingForJourney = questions.Any(m => m.QuestionId == hideForJourney && m.Answer == hideForJourneyAnswer);
        //
        //    if (position == 0 || position < showFromPageNo || hidePagingForJourney)
        //    {
        //        ViewBag.HidePageCounter = true;
        //    }
        //                
        //}


        private static readonly HashSet<char> allowedChars = new HashSet<char>(@"1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.,'()?!#&$£%^@*;:+=_-/ ");
        private static readonly List<string> restrictedWords = new List<string> { "javascript", "onblur", "onchange", "onfocus", "onfocusin", "onfocusout", "oninput", "onmouseenter", "onmouseleave",
            "onselect", "onclick", "ondblclick", "onkeydown", "onkeypress", "onkeyup", "onmousedown", "onmousemove", "onmouseout", "onmouseover", "onmouseup", "onscroll", "ontouchstart",
            "ontouchend", "ontouchmove", "ontouchcancel", "onwheel" };


        [HttpPost("form/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Index(CurrentPageVM vm)
        {
            try
            {
                PageVM pageVm = null;
                try
                {
                    //Get the current PageVm from Session (throws exception if session is null/timed out)
                    pageVm = _sessionService.GetPageById(vm.PageId, false);
                }
                catch
                {
                    //user session has timed out
                    return GetCustomErrorCode(EnumStatusCode.FormPageContinueSessionNullError, "Error with user session. Session is null: Id='" + vm.PageId + "'");
                }

                if (pageVm == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.FormPageContinueNullError, "Error with user session. pageVm is null: Id='" + vm.PageId + "'");
                }
                var skipNextQuestions = false;//is true if for example user changes from "bad" to "good and bad"

                var formVm = _sessionService.GetFormVmFromSession();
                if (pageVm.PageId == _config.Value.ServiceNotFoundPage)
                {
                    //this happens when a user changes from a selected location to a location not found
                    _sessionService.UpdateNavOrder(pageVm.PageId);//this enables the page for edit to be this page
                    //so remove any previously selected location
                    var searchPage = formVm.Pages.FirstOrDefault(p => p.PageId == "search");
                    if (searchPage != null)
                    {
                        searchPage.Questions.FirstOrDefault().Answer = string.Empty;
                        //remove search from the nav order
                        _sessionService.RemoveFromNavOrder(searchPage.PageId);
                    }
                    _sessionService.UpdateNavOrder(pageVm.PageId);

                    //update any previously entered location not found
                    var previousLocation = _sessionService.GetUserSession().LocationName;
                    var defaultServiceName = _config.Value.SiteTextStrings.DefaultServiceName;
                    //Store the user entered details
                    _sessionService.SetUserSessionVars(new UserSessionVM { LocationId = "0", LocationName = defaultServiceName, ProviderId = "" });
                    _sessionService.UpdateFormData(new List<DataItemVM>()
                    {
                        new DataItemVM() { Id = "LocationName", Value = defaultServiceName },
                        new DataItemVM() { Id = "LocationId", Value = "0" },
                        new DataItemVM() { Id = "LocationCategory", Value = "unknown" }
                    });
                    //Set up our replacement text
                    var replacements = new Dictionary<string, string>
                    {
                        {previousLocation, defaultServiceName}
                    };

                    _sessionService.SaveFormVmToSession(formVm, replacements);
                }

                /*
                Commented out second part of IF statement

                Reason: this was stopping future nav order being edited properly when journey has changed.                
                Issue: it's not certain why this was included in the first place; it's possible that it should have been a second 'NOT' condition
                Consequences: in some unusual circumstances(without this second IF section), it is possible to have a journey where you can't hit
                              all of the questions to edit them.We have not been able to reproduce this, however, so leaving the change in for now.
                Original line:
                if (!string.IsNullOrWhiteSpace(_sessionService.PageForEdit) && (string.IsNullOrWhiteSpace(_sessionService.GetChangeModeRedirectId())))
                */
                if (!string.IsNullOrWhiteSpace(_sessionService.PageForEdit) ) // && (string.IsNullOrWhiteSpace(_sessionService.GetChangeModeRedirectId())))
                {
                    if (_sessionService.PageForEdit == pageVm.PageId)
                    {
                        if ((_sessionService.GetChangeMode() ?? "") == _config.Value.SiteTextStrings.ReviewPageId)
                        {
                            //this page was revisited and edited from check your answers so we have a completed form
                            if (!_pageHelper.HasAnswerChanged(Request, pageVm.Questions) && _pageHelper.IsQuestionAnswered(Request, pageVm.Questions))
                            {
                                //nothings changed so bomb out
                                _sessionService.ClearChangeMode();
                                return RedirectToAction("Index", "CheckYourAnswers");
                            }

                            if (pageVm.Questions.Any() && _pageHelper.HasPathChanged(Request, pageVm.Questions) && (! _sessionService.ChangedLocationMode))
                            {
                                //user journey will now go down a different path
                                //so save the page where the journey goes back to the existing path
                                _sessionService.SaveChangeModeRedirectId(pageVm.ChangeModeTriggerPageId);

                                if (pageVm.NextPageId == _config.Value.SiteTextStrings.ReviewPageId)
                                {
                                    //remove any possible answered questions further along the path
                                    _sessionService.RemoveNavOrderFrom(pageVm.PageId);
                                }
                                else
                                {
                                    //remove only the questions between this one and the end of the new path
                                    _sessionService.RemoveNavOrderSectionFrom(pageVm.PageId, pageVm.ChangeModeTriggerPageId);
                                }
                            }
                            else
                            {
                                //there's been a change but no change in the path so skip all the next questions
                                skipNextQuestions = true;

                            }
                        }
                        else
                        {
                            if (pageVm.Questions.Any() && _pageHelper.HasPathChanged(Request, pageVm.Questions) &&
                                (!_sessionService.ChangedLocationMode))
                            {
                                //page revisited from multiple back button clicks and the journey path has changed
                                //so remove any possible answered questions further along the path
                                _sessionService.RemoveNavOrderFrom(pageVm.PageId);
                                //user will have to go through the entire journey again
                                _sessionService.ClearChangeModeRedirectId();
                            }
                            //the path hasn't changed e.g. "Bad" to "Good And Bad"
                        }
                    }
                }

                var userSession = _sessionService.GetUserSession();
                if (userSession == null)//shouldn't happen as it's handled above
                {
                    return GetCustomErrorCode(EnumStatusCode.FormPageContinueSessionNullError, "Error with user session. Session is null: Id='" + vm.PageId + "'");
                }
                if (string.IsNullOrWhiteSpace(userSession.LocationName))
                {
                    return GetCustomErrorCode(EnumStatusCode.FormContinueLocationNullError, "Error with user session. Location is null: Id='" + vm.PageId + "'");
                }

                var serviceNotFound = userSession.LocationName.Equals(_config.Value.SiteTextStrings.DefaultServiceName);
                ViewBag.BackLink = new BackLinkVM { Show = true, Url = _pageHelper.GetPreviousPage(pageVm, _sessionService, _config, Url, serviceNotFound), Text = _config.Value.SiteTextStrings.BackLinkText };

                //We need to store the nextPageId before calling ValidatePage, so we can check if it gets updated by a question's answer logic
                var rootNextPageId = !string.IsNullOrWhiteSpace(pageVm.NextPageReferenceId) ? _pageHelper.GetNextPageIdFromPage(formVm, pageVm.NextPageReferenceId) : pageVm.NextPageId;

                if (Request?.Form != null)
                {
                    //Validate the Response against the page json and update PageVm to contain the answers
                    //This will also update the NextPageId with any answer logic in the questions.
                    _gdsValidate.ValidatePage(pageVm, Request.Form, true, restrictedWords);
                }

                //Get the error count
                var errorCount = pageVm.Questions?.Count(m => m.Validation != null && m.Validation.IsErrored);

                //If we have errors return to the View
                if (errorCount > 0)
                {
                    ViewBag.Title = "Error: " + pageVm.PageTitle + _config.Value.SiteTextStrings.SiteTitleSuffix;
                    return View(pageVm);
                }
                
                _sessionService.ChangedLocationMode = false;//always reset this

                //Now we need to update the FormVM in session.
                _sessionService.UpdatePageVmInFormVm(pageVm);

                //No errors redirect to the Index page with our new PageId
                var nextPageId = GetNextPageId(formVm, pageVm, userSession.LocationName, skipNextQuestions, serviceNotFound, rootNextPageId);              

                //Check the nextPageId for preset controller names
                switch (nextPageId)
                {
                    case "HowWeUseYourInformation":
                        return RedirectToAction("Index", "HowWeUseYourInformation");

                    case "CheckYourAnswers":
                        return RedirectToAction("Index", "CheckYourAnswers");

                    case "Home":
                        return RedirectToAction("Index", "Home");

                    case "search":
                        return RedirectToAction("Index", "Search");
                }

                //Finally, No Errors so load the next page
                return RedirectToAction("Index", new { id = nextPageId });

            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error updating PageVM. Id:='" + vm.PageId + "'");
                throw ex;
            }
        }

        private string GetNextPageId(FormVM formVm, PageVM pageVm, string location, bool skipNextQuestions, bool serviceNotFound, string rootNextPageId)
        {

            var nextPageId = !string.IsNullOrWhiteSpace(pageVm.NextPageReferenceId) ? _pageHelper.GetNextPageIdFromPage(formVm, pageVm.NextPageReferenceId) : pageVm.NextPageId;

            //We should only apply the path change logic if 1) there is a path change question and 2) we HAVE NOT already changed the nextPageId with answerLogic (which is higher priority)
            if (pageVm.PathChangeQuestion != null && nextPageId == rootNextPageId)
            {
                //branch the user journey if a previous question has a specific answer
                var questions = formVm.Pages.SelectMany(m => m.Questions).ToList();

                var startChangeJourney = questions.FirstOrDefault(m => m.QuestionId == pageVm.PathChangeQuestion.QuestionId);
                if (startChangeJourney != null && startChangeJourney.Answer == pageVm.PathChangeQuestion.Answer)
                {
                    nextPageId = pageVm.PathChangeQuestion.NextPageId;
                }
            }

            //check if this is the end of the changed question flow in edit mode
            if ((_sessionService.GetChangeModeRedirectId() ?? string.Empty) == nextPageId || (_sessionService.GetChangeModeRedirectId() ?? string.Empty) == pageVm.NextPageId || skipNextQuestions)
            {
                nextPageId = _config.Value.SiteTextStrings.ReviewPageId;
            }

            //Special case to handle if we are currently on the first page of the form, which would ordinarily direct the user to 'search'
            if (pageVm.PageId == _config.Value.FormStartPage)
            {
                if (location == _config.Value.SiteTextStrings.NonSelectedServiceName)
                {
                    //this is the first time into the search so show it
                    nextPageId = "search";
                }
                else
                {
                    //skip the search but put it into the page history
                    _sessionService.UpdateNavOrder("search");
                    if (serviceNotFound)
                    {
                        var serviceNotFoundPageId = _config.Value.ServiceNotFoundPage;
                        _sessionService.UpdateNavOrder(serviceNotFoundPageId);
                    }
                }
            }

            return nextPageId;
        }
    }
}
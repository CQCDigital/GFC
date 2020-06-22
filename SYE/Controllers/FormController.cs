using System;
using System.Collections.Generic;
using System.Linq;
using GDSHelpers;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            var lastPage = _sessionService.GetLastPage();
            _sessionService.SetLastPage($"form/{id}");

            try
            {
                var userSession = _sessionService.GetUserSession();
                if (userSession == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.FormPageLoadSessionNullError, "Error with user session. Session is null: Id='" + id + "'");
                }

                if (userSession.LocationName == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.FormPageLoadLocationNullError, "Error with user session. Location is null: Id='" + id + "'");
                }

                var serviceNotFound = userSession.LocationName.Equals(_config.Value.SiteTextStrings.DefaultServiceName);
                PageVM pageVm = null;
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
                //check if the user answered the required questions to show this page
                if (!_pageHelper.CheckPageHistory(pageVm, lastPage ?? searchReferrer, false, _sessionService, _config.Value.ExternalStartPage))
                {
                    return GetCustomErrorCode(EnumStatusCode.FormPageLoadHistoryError, "Error with page load. Page history not found: Id='" + id + "'");
                }

                var refererIsCheckYourAnswers = (lastPage ?? "").Contains(_config.Value.SiteTextStrings.ReviewPage);

                ViewBag.BackLink = new BackLinkVM { Show = true, Url =  refererIsCheckYourAnswers ? _config.Value.SiteTextStrings.ReviewPage : _pageHelper.GetPreviousPage(pageVm, _sessionService, _config, Url, serviceNotFound), Text = _config.Value.SiteTextStrings.BackLinkText };

                //Update the users journey                
                if (!string.IsNullOrWhiteSpace(_sessionService.GetChangeModeRedirectId()) && pageVm.NextPageId != _config.Value.SiteTextStrings.ReviewPageId)
                {
                    _sessionService.UpdateNavOrder(pageVm.PageId, _sessionService.GetChangeModeRedirectId());
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
                    _sessionService.UpdateNavOrder(pageVm.PageId);
                }                

                ViewBag.Title = pageVm.PageTitle + _config.Value.SiteTextStrings.SiteTitleSuffix;
                return View(pageVm);
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error loading form: Id='" + id + "'");
                throw ex;
            }
        }

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
                    return GetCustomErrorCode(EnumStatusCode.FormPageContinueSessionNullError, "Error with user session. Session is null: Id='" + vm.PageId + "'");
                }

                if (pageVm == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.FormPageContinueNullError, "Error with user session. pageVm is null: Id='" + vm.PageId + "'");
                }
                var skipNextQuestions = false;//is true if for example user changes from "bad" to "good and bad"

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
                            if (!_pageHelper.HasAnswerChanged(Request, pageVm.Questions))
                            {
                                //nothings changed so bomb out
                                _sessionService.ClearChangeMode();
                                return RedirectToAction("Index", "CheckYourAnswers");
                            }

                            if (pageVm.Questions.Any() && _pageHelper.HasPathChanged(Request, pageVm.Questions))
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
                            //page revisited from back button click
                            //this would be the first journey through the questions at this point
                            //so remove any possible answered questions further along the path
                            _sessionService.RemoveNavOrderFrom(pageVm.PageId);
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

                if (Request?.Form != null)
                {
                    //Validate the Response against the page json and update PageVm to contain the answers
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

                //Now we need to update the FormVM in session.
                _sessionService.UpdatePageVmInFormVm(pageVm);

                //No errors redirect to the Index page with our new PageId
                var nextPageId = pageVm.NextPageId;

                if (pageVm.PathChangeQuestion != null)
                {
                    //branch the user journey if a previous question has a specific answer
                    var formVm = _sessionService.GetFormVmFromSession();
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

                //Check the nextPageId for preset controller names
                switch (nextPageId)
                {
                    case "HowWeUseYourInformation":
                        return RedirectToAction("Index", "HowWeUseYourInformation");

                    case "CheckYourAnswers":
                        return RedirectToAction("Index", "CheckYourAnswers");

                    case "Home":
                        return RedirectToAction("Index", "Home");
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
   }
}
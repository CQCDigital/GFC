using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.WebPages;
using GDSHelpers;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SYE.Helpers.Enums;
using SYE.Models;
using SYE.Repository;
using SYE.Services;
using SYE.ViewModels;

namespace SYE.Controllers
{
    public class FeedbackController : BaseController
    {
        private readonly HashSet<char> _allowedChars = new HashSet<char>(@"1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.,'()?!#&$£%^@*;:+=_-/ ");
        private readonly List<string> _restrictedWords = new List<string> { "javascript", "onblur", "onchange", "onfocus", "onfocusin", "onfocusout", "oninput", "onmouseenter", "onmouseleave",
            "onselect", "onclick", "ondblclick", "onkeydown", "onkeypress", "onkeyup", "onmousedown", "onmousemove", "onmouseout", "onmouseover", "onmouseup", "onscroll", "ontouchstart",
            "ontouchend", "ontouchmove", "ontouchcancel", "onwheel" };

        private readonly ILogger _logger;//keep logger because extra logs are generated in this controller
        private readonly IFormService _formService;
        private readonly IGdsValidation _gdsValidate;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly ISessionService _sessionService;
        
        public FeedbackController(
            ILogger<FeedbackController> logger,
            IFormService formService,
            IGdsValidation gdsValidate,
            IConfiguration configuration,
            INotificationService notificationService,
            ISessionService sessionService)
        {
            _logger = logger;
            _formService = formService;
            _gdsValidate = gdsValidate;
            _configuration = configuration;
            _notificationService = notificationService;
            _sessionService = sessionService;
        }

        [HttpGet("what-do-you-think-of-this-form")]
        public IActionResult WhatDoYouThink([FromHeader(Name = "referer")] string urlReferer)
        {
            var lastPage = _sessionService.GetLastPage();
            if (lastPage == null || !lastPage.Contains("you-have-sent-your-feedback"))
            {
                return GetCustomErrorCode(EnumStatusCode.ExitSurveyOutOfSequence, "Post-completion survey hit out of //sequence");
            }

            try
            {
                var pageViewModel = GetPage();
                if (pageViewModel == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.ExitSurveyPageLoadJsonError, "Error loading post-completion feedback form. Json form not loaded");
                }

                ViewBag.UrlReferer = urlReferer;

                ViewBag.BackLink = new BackLinkVM { Show = false, Url = urlReferer, Text = _configuration.GetSection("ApplicationSettings:SiteTextStrings").GetValue<string>("BackLinkText")};

                ViewBag.Title = "What do you think" + _configuration.GetSection("ApplicationSettings:SiteTextStrings").GetValue<string>("SiteTitleSuffix");

                return View(pageViewModel);
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error loading post-completion feedback form.");
                throw ex;
            }
        }

        [HttpPost("what-do-you-think-of-this-form")]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitWhatDoYouThink([FromForm(Name = "url-referer")] string urlReferer)
        {
            var lastPage = _sessionService.GetLastPage();
            if (lastPage == null || !lastPage.Contains("you-have-sent-your-feedback"))
            {
                return GetCustomErrorCode(EnumStatusCode.ExitSurveyOutOfSequence, "Post-completion survey hit out of //sequence");
            }

            PageVM pageViewModel = null;
            try
            {
                pageViewModel = GetPage();

                if (pageViewModel == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.ExitSurveySubmissionJsonError, "Error submitting post-completion feedback. Json form not loaded");
                }

                urlReferer = _gdsValidate.CleanText(urlReferer, true, _restrictedWords, _allowedChars);

                if (urlReferer.IsEmpty())
                    urlReferer = _configuration.GetSection("ApplicationSettings:GFCUrls").GetValue<string>("StartPage");

                _gdsValidate.ValidatePage(pageViewModel, Request.Form, true, _restrictedWords, _allowedChars);

                if (pageViewModel.Questions.Any(m => m.Validation?.IsErrored == true))
                {
                    var cleanUrlReferer = urlReferer.Replace("feedback-thank-you", "");
                    ViewBag.BackLink = new BackLinkVM { Show = false, Url = cleanUrlReferer, Text = _configuration.GetSection("ApplicationSettings:SiteTextStrings").GetValue<string>("BackLinkText")};
                    ViewBag.UrlReferer = cleanUrlReferer;
                    ViewBag.HideHelpLink = true;
                    ViewBag.Title = "Error: Feedback" + _configuration.GetSection("ApplicationSettings:SiteTextStrings").GetValue<string>("SiteTitleSuffix");
                    
                    return View(nameof(WhatDoYouThink), pageViewModel);
                }
                
                Task.Run(async () =>
                {
                    await SendEmailNotificationAsync(pageViewModel)
                            .ContinueWith(notificationTask =>
                            {
                                if (notificationTask.IsFaulted)
                                {
                                    _logger.LogError(notificationTask.Exception, "Error sending post completion feedback email");
                                }
                            })
                            .ConfigureAwait(false);
                });

                _sessionService.SetLastPage("what-do-you-think-of-this-form");

                return RedirectToAction(nameof(ThanksForYourFeedback), new { urlReferer });
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error submitting post-completion feedback");
                throw ex;
            }
        }

        [Route("thanks-for-your-feedback")]
        public IActionResult ThanksForYourFeedback(string urlReferer)
        {
            var lastPage = _sessionService.GetLastPage();
            if (lastPage == null || !lastPage.Contains("what-do-you-think-of-this-form"))
            {
                return GetCustomErrorCode(EnumStatusCode.ExitSurveyOutOfSequence, "Post-completion survey hit out of //sequence");
            }
            _sessionService.SetLastPage("thanks-for-your-feedback");

            if (urlReferer.IsEmpty())
                urlReferer = _configuration.GetSection("ApplicationSettings:GFCUrls").GetValue<string>("StartPage");

            ViewBag.Title = "You've sent your feedback" + _configuration.GetSection("ApplicationSettings:SiteTextStrings").GetValue<string>("SiteTitleSuffix");

            ViewBag.HideHelpLink = true;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Continue(RedirectVM model)
        {
            return Redirect(model.Url);
        }

        private PageVM GetPage()
        {
            var formName = _configuration?.GetSection("FormsConfiguration:PostCompletionFeedbackForm").GetValue<string>("Name");
            var version = _configuration?.GetSection("FormsConfiguration:PostCompletionFeedbackForm").GetValue<string>("Version");

            try
            {
                var form = string.IsNullOrEmpty(version) ?
                    _formService.GetLatestFormByName(formName).Result :
                    _formService.FindByNameAndVersion(formName, version).Result;

                return form.Pages.FirstOrDefault() ?? null;
            }
            catch
            {
                return null;
            }
        }

        private Task SendEmailNotificationAsync(PageVM submission)
        {
            if (submission == null)
            {
                throw new ArgumentNullException(nameof(submission));
            }

            return SendEmailNotificationInternalAsync(submission);
        }

        private async Task SendEmailNotificationInternalAsync(PageVM submission)
        {
            var phase = _configuration.GetSection("EmailNotification:PostCompletionFeedbackEmail").GetValue<string>("Phase");
            var emailTemplateId = _configuration.GetSection("EmailNotification:PostCompletionFeedbackEmail").GetValue<string>("EmailTemplateId");
            var emailAddress = _configuration.GetSection("EmailNotification:PostCompletionFeedbackEmail").GetValue<string>("ServiceSupportEmailAddress");
            var fieldMappings = _configuration.GetSection("EmailNotification:PostCompletionFeedbackEmail:FieldMappings").Get<IEnumerable<EmailFieldMapping>>();

            var ableToTellUs = submission?
                .Questions?.FirstOrDefault(x => x.QuestionId.Equals(fieldMappings.FirstOrDefault(y => y.Name == "able-to-tell-us").FormField, StringComparison.OrdinalIgnoreCase))?
                .Answer ?? string.Empty;
            var difficultyLevel = submission?
                .Questions?.FirstOrDefault(x => x.QuestionId.Equals(fieldMappings.FirstOrDefault(y => y.Name == "difficulty-level").FormField, StringComparison.OrdinalIgnoreCase))?
                .Answer ?? string.Empty;
            var moreDetail = submission?
                .Questions?.FirstOrDefault(x => x.QuestionId.Equals(fieldMappings.FirstOrDefault(y => y.Name == "more-detail").FormField, StringComparison.OrdinalIgnoreCase))?
                .Answer ?? string.Empty;

            var personalisation =
                new Dictionary<string, dynamic> {
                    { "service-phase", phase },
                    { fieldMappings.FirstOrDefault(y => y.Name == "able-to-tell-us")?.TemplateField, ableToTellUs },
                    { fieldMappings.FirstOrDefault(y => y.Name == "difficulty-level")?.TemplateField, difficultyLevel },
                    { fieldMappings.FirstOrDefault(y => y.Name == "more-detail")?.TemplateField, moreDetail }
                };

            await _notificationService.NotifyByEmailAsync(
                    emailTemplateId, emailAddress, personalisation, null, null
                ).ConfigureAwait(false);
        }
    }
}
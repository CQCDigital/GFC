using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.WebPages;
using GDSHelpers;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SYE.Helpers.Enums;
using SYE.Models;
using SYE.Repository;
using SYE.Services;
using SYE.ViewModels;

namespace SYE.Controllers
{
    public class HelpController : BaseController
    {
        private readonly ILogger _logger;//keep logger because extra logs are generated in this controller
        private readonly IFormService _formService;
        private readonly IGdsValidation _gdsValidate;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;

        private readonly HashSet<char> _allowedChars = new HashSet<char>(@"1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.,'()?!#&$£%^@*;:+=_-/ ");
        private readonly List<string> _restrictedWords = new List<string> { "javascript", "onblur", "onchange", "onfocus", "onfocusin", "onfocusout", "oninput", "onmouseenter", "onmouseleave",
            "onselect", "onclick", "ondblclick", "onkeydown", "onkeypress", "onkeyup", "onmousedown", "onmousemove", "onmouseout", "onmouseover", "onmouseup", "onscroll", "ontouchstart",
            "ontouchend", "ontouchmove", "ontouchcancel", "onwheel" };

        public HelpController(IServiceProvider service)
        {
            this._logger = service?.GetService<ILogger<HelpController>>() as ILogger;
            this._formService = service?.GetService<IFormService>();
            this._gdsValidate = service?.GetService<IGdsValidation>();
            this._configuration = service?.GetService<IConfiguration>();
            this._notificationService = service?.GetService<INotificationService>();
        }

        [HttpGet("report-a-problem")]
        public IActionResult Feedback([FromHeader(Name = "referer")] string urlReferer)
        {
            try
            {
                var pageViewModel = GetPage();
                if (pageViewModel == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.RPPageLoadJsonError, "Error loading service feedback form. Json form not loaded");
                }

                ViewBag.UrlReferer = urlReferer;

                ViewBag.BackLink = new BackLinkVM { Show = true, Url = urlReferer, Text = _configuration.GetSection("ApplicationSettings:SiteTextStrings").GetValue<string>("BackLinkText")};

                ViewBag.Title = "Report a problem" + _configuration.GetSection("ApplicationSettings:SiteTextStrings").GetValue<string>("SiteTitleSuffix");
                ViewBag.HideHelpLink = true;

                return View(nameof(Feedback), pageViewModel);
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error loading service feedback form.");
                throw ex;
            }
        }

        [HttpPost("report-a-problem"), ActionName("Feedback")]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitFeedback([FromForm(Name = "url-referer")] string urlReferer)
        {
            PageVM pageViewModel = null;
            try
            {
                pageViewModel = GetPage();

                if (pageViewModel == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.RPSubmissionJsonError, "Error submitting service feedback. Json form not loaded");
                }

                urlReferer = _gdsValidate.CleanText(urlReferer, true, _restrictedWords, _allowedChars);

                if (urlReferer.IsEmpty())
                    urlReferer = _configuration.GetSection("ApplicationSettings:GFCUrls").GetValue<string>("StartPage");

                _gdsValidate.ValidatePage(pageViewModel, Request.Form, true, _restrictedWords, _allowedChars);

                if (pageViewModel.Questions.Any(m => m.Validation?.IsErrored == true))
                {
                    var cleanUrlReferer = urlReferer.Replace("feedback-thank-you", "");
                    ViewBag.BackLink = new BackLinkVM { Show = true, Url = cleanUrlReferer, Text = _configuration.GetSection("ApplicationSettings:SiteTextStrings").GetValue<string>("BackLinkText")};
                    ViewBag.UrlReferer = cleanUrlReferer;
                    ViewBag.HideHelpLink = true;
                    ViewBag.Title = "Error: Report a problem" + _configuration.GetSection("ApplicationSettings:SiteTextStrings").GetValue<string>("SiteTitleSuffix");

                    return View(nameof(Feedback), pageViewModel);
                }

                var emailAddress = pageViewModel?
                    .Questions?.FirstOrDefault(x => x.QuestionId.Equals("email-address"))?
                    .Answer ?? string.Empty; 
                
                Task.Run(async () =>
                {
                    await SendEmailNotificationAsync(pageViewModel, urlReferer, emailAddress)
                            .ContinueWith(notificationTask =>
                            {
                                if (notificationTask.IsFaulted)
                                {
                                    _logger.LogError(notificationTask.Exception, "Error sending service feedback email.");
                                }
                            })
                            .ConfigureAwait(false);
                });

                return RedirectToAction(nameof(FeedbackThankYou), new { urlReferer });
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error submitting service feedback");
                throw ex;
            }
        }

        [Route("feedback-thank-you")]
        public IActionResult FeedbackThankYou(string urlReferer)
        {
            if (urlReferer.IsEmpty())
                urlReferer = _configuration.GetSection("ApplicationSettings:GFCUrls").GetValue<string>("StartPage");

            ViewBag.Title = "You've sent your feedback" + _configuration.GetSection("ApplicationSettings:SiteTextStrings").GetValue<string>("SiteTitleSuffix");

            ViewBag.HideHelpLink = true;

            return View(new RedirectVM{Url = urlReferer});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Continue(RedirectVM model)
        {
            return Redirect(model.Url);
        }

        private PageVM GetPage()
        {
            var formName = _configuration?.GetSection("FormsConfiguration:ServiceFeedbackForm").GetValue<string>("Name");
            var version = _configuration?.GetSection("FormsConfiguration:ServiceFeedbackForm").GetValue<string>("Version");

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

        private Task SendEmailNotificationAsync(PageVM submission, string urlReferer, string externalRecipient = null)
        {
            if (submission == null)
            {
                throw new ArgumentNullException(nameof(submission));
            }

            if (!string.IsNullOrWhiteSpace(externalRecipient))
            {
                SendEmailNotificationExternalAsync(externalRecipient).Wait();
            }

            return SendEmailNotificationInternalAsync(submission, urlReferer);
        }

        private async Task SendEmailNotificationInternalAsync(PageVM submission, string urlReferer)
        {
            var phase = _configuration.GetSection("EmailNotification:FeedbackEmail").GetValue<string>("Phase");
            var emailTemplateId = _configuration.GetSection("EmailNotification:FeedbackEmail").GetValue<string>("EmailTemplateId");
            var emailAddress = _configuration.GetSection("EmailNotification:FeedbackEmail").GetValue<string>("ServiceSupportEmailAddress");
            var fieldMappings = _configuration.GetSection("EmailNotification:FeedbackEmail:FieldMappings").Get<IEnumerable<EmailFieldMapping>>();

            var feedbackMessage = submission?
                .Questions?.FirstOrDefault(x => x.QuestionId.Equals(fieldMappings.FirstOrDefault(y => y.Name == "message").FormField, StringComparison.OrdinalIgnoreCase))?
                .Answer ?? string.Empty;
            var feedbackUserName = submission?
                .Questions?.FirstOrDefault(x => x.QuestionId.Equals(fieldMappings.FirstOrDefault(y => y.Name == "name").FormField, StringComparison.OrdinalIgnoreCase))?
                .Answer ?? string.Empty;
            var feedbackUserEmailAddress = submission?
                .Questions?.FirstOrDefault(x => x.QuestionId.Equals(fieldMappings.FirstOrDefault(y => y.Name == "email").FormField, StringComparison.OrdinalIgnoreCase))?
                .Answer ?? string.Empty;

            var personalisation =
                new Dictionary<string, dynamic> {
                    { "service-phase", phase },
                    { "banner-clicked-on-page", urlReferer },
                    { fieldMappings.FirstOrDefault(y => y.Name == "message")?.TemplateField, feedbackMessage },
                    { fieldMappings.FirstOrDefault(y => y.Name == "name")?.TemplateField, feedbackUserName },
                    { fieldMappings.FirstOrDefault(y => y.Name == "email")?.TemplateField, feedbackUserEmailAddress }
                };

            await _notificationService.NotifyByEmailAsync(
                    emailTemplateId, emailAddress, personalisation, null, null
                ).ConfigureAwait(false);
        }

        public async Task SendEmailNotificationExternalAsync(string emailAddress)
        {
            var emailTemplateId = _configuration.GetSection("EmailNotification:FeedbackEmailExternal").GetValue<string>("EmailTemplateId");

            await _notificationService.NotifyByEmailAsync(
                emailTemplateId, emailAddress, null, null, null
            ).ConfigureAwait(false);

        }
    }
}
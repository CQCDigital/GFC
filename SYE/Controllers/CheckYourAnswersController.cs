using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SYE.Helpers;
using SYE.Models;
using SYE.Models.SubmissionSchema;
using SYE.Repository;
using SYE.Services;
using SYE.Helpers.Enums;
using SYE.Helpers.Extensions;

namespace SYE.Controllers
{
    public class CheckYourAnswersController : BaseController
    {
        private const string _pageId = "CheckYourAnswers";
        private readonly ILogger _logger;
        private readonly ISubmissionService _submissionService;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly IDocumentService _documentService;
        private readonly ISessionService _sessionService;
        private readonly IPageHelper _pageHelper;

        public CheckYourAnswersController(IServiceProvider service)
        {
            _logger = service?.GetRequiredService<ILogger<CheckYourAnswersController>>() as ILogger;
            _sessionService = service?.GetRequiredService<ISessionService>() ?? null;
            _submissionService = service.GetRequiredService<ISubmissionService>();
            _configuration = service?.GetRequiredService<IConfiguration>();
            _notificationService = service.GetRequiredService<INotificationService>();
            _documentService = service.GetRequiredService<IDocumentService>();
            _pageHelper = service.GetRequiredService<IPageHelper>();
        }

        [HttpGet, Route("form/check-your-answers")]
        public IActionResult Index()
        {
            var lastPage = _sessionService.GetLastPage();
            _sessionService.SetLastPage("form/check-your-answers");

            try
            {
                _sessionService.ClearChangeModeRedirectId();
                var formVm = _sessionService.GetFormVmFromSession();
                if (formVm == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.CYAFormNullError, "Error with user session. formVm is null.");
                }
                if ((_sessionService.GetUserSession().LocationName) == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.CYALocationNullError, "Error with user session. Location is null.");
                }
                //check if the user answered the required questions to show this page
                var pageVm = formVm.Pages.Where(p => p.PageId == _pageId).FirstOrDefault();
                if (!_pageHelper.CheckPageHistory(pageVm, lastPage, true, _sessionService, null))
                {
                    return GetCustomErrorCode(EnumStatusCode.CYASubmissionHistoryError, "Error with user submission. Page history not found: Id='" + _pageId + "'");
                }

                var vm = new CheckYourAnswersVm
                {
                    FormVm = formVm,
                    SendConfirmationEmail = true,
                    LocationName = _sessionService.GetUserSession().LocationName,
                    PageHistory = _sessionService.GetNavOrder()
                };

                ViewBag.Title = "Check your answers" + _configuration.GetSection("ApplicationSettings:SiteTextStrings").GetValue<string>("SiteTitleSuffix");

                return View(vm);
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error loading FormVM.");
                throw ex;
            }

        }


        [HttpPost, Route("form/check-your-answers")]
        [PreventDuplicateRequest]
        [ValidateAntiForgeryToken]
        public IActionResult Index(CheckYourAnswersVm vm)
        {
            try
            {
                var formVm = _sessionService.GetFormVmFromSession();
                if (formVm == null)
                {
                    return GetCustomErrorCode(EnumStatusCode.CYASubmissionFormNullError, "Error submitting service feedback. Null or empty formVm.");
                }

                var reference = _submissionService.GenerateUniqueUserRefAsync().Result.ToString();
                //var reference = _submissionService.GenerateSnowmakerUserRefAsync().Result.ToString();

                if (string.IsNullOrWhiteSpace(reference))
                {
                    return GetCustomErrorCode(EnumStatusCode.CYASubmissionReferenceNullError, "Error submitting feedback!  Null or empty submission Id");
                }
                if (int.Parse(reference) == 0)
                {
                    return GetCustomErrorCode(EnumStatusCode.CYASubmissionReferenceNullError, "Error submitting feedback!  zero submission Id");
                }

                var submission = GenerateSubmission(formVm, reference);
                submission.SubmissionId = reference;
                submission = _submissionService.CreateAsync(submission).Result;

                if (vm?.SendConfirmationEmail == true && !string.IsNullOrWhiteSpace(reference))
                {
                    var fieldMappings = _configuration
                        .GetSection("EmailNotification:ConfirmationEmail:FieldMappings")
                        .Get<IEnumerable<EmailFieldMapping>>();

                    var feedbackUserName = submission?
                        .Answers?
                        .FirstOrDefault(x => x.QuestionId.Equals(fieldMappings.FirstOrDefault(y => y.Name == "name")?.FormField, StringComparison.OrdinalIgnoreCase))?
                        .Answer ?? string.Empty;

                    var emailAddress = submission?
                        .Answers?
                        .FirstOrDefault(x => x.QuestionId.Equals(fieldMappings.FirstOrDefault(y => y.Name == "email")?.FormField, StringComparison.OrdinalIgnoreCase))?
                        .Answer ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(emailAddress))
                    {

                        var locationId = submission?.LocationId;
                        var locationName = submission?.LocationName;
                        var submissionId = submission?.Id;

                        Task.Run(async () =>
                        {
                            await SendEmailNotificationAsync(feedbackUserName, emailAddress, locationId, locationName, submissionId, reference)
                                    .ContinueWith(notificationTask =>
                                    {
                                        if (notificationTask.IsFaulted)
                                        {
                                            _logger.LogError(notificationTask.Exception, $"Error sending confirmation email with submission id: [{reference}].");
                                        }
                                    })
                                    .ConfigureAwait(false);
                        });
                    }
                }

                HttpContext.Session.Clear();
                TempData.Clear();//clear any residual items
                HttpContext.Session.SetString("ReferenceNumber", reference);

                //Reset this flag so the cookie banner does not show on the confirmation page
                _sessionService.SetCookieFlagOnSession("true");
                _sessionService.SetLastPage("form/check-your-answers");

                return RedirectToAction("Index", "Confirmation");
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error submitting feedback!");
                throw ex;
            }
        }

        private SubmissionVM GenerateSubmission(FormVM formVm, string gfcReference)
        {
            var submissionVm = new SubmissionVM
            {
                Version = formVm.Version,
                Id = Guid.NewGuid().ToString(),
                DateCreated = new DateTime().GetLocalDateTime(),
                FormName = formVm.FormName,
                ProviderId = HttpContext.Session.GetString("ProviderId"),
                LocationId = HttpContext.Session.GetString("LocationId"),
                LocationName = HttpContext.Session.GetString("LocationName"),
                SubmissionId = gfcReference
            };
            var answers = new List<AnswerVM>();

            var pageHistory = _sessionService.GetNavOrder();
            foreach (var page in formVm.Pages.Where(m => pageHistory.Contains(m.PageId)).OrderBy(m => pageHistory.IndexOf(m.PageId)))
            {
                answers.AddRange(page.Questions.Where(m => !string.IsNullOrEmpty(m.Answer))
                    .Select(question => new AnswerVM
                    {
                        PageId = page.PageId,
                        QuestionId = question.QuestionId,
                        Question = string.IsNullOrEmpty(question.Question) ? page.PageName.StripHtml() : question.Question.StripHtml(),
                        Answer = question.Answer.StripHtml(),
                        DocumentOrder = question.DocumentOrder
                    }));
            }

            submissionVm.Answers = answers;

            submissionVm.Base64Attachment = _documentService.CreateSubmissionDocument(submissionVm);
            submissionVm.Status = "Saved";

            return submissionVm;
        }

        private async Task SendEmailNotificationAsync(string fullName, string emailAddress, string locationId, string locationName, string submissionId, string submissionReference)
        {
            var emailTemplateId = string.Empty;
            if (string.IsNullOrWhiteSpace(locationId.Replace("0", "")))
            {
                emailTemplateId = _configuration.GetSection("EmailNotification:ConfirmationEmail").GetValue<string>("WithoutLocationEmailTemplateId");
            }
            else
            {
                emailTemplateId = _configuration.GetSection("EmailNotification:ConfirmationEmail").GetValue<string>("WithLocationEmailTemplateId");
            }

            var greetingTemplate = _configuration.GetSection("EmailNotification:ConfirmationEmail").GetValue<string>("GreetingTemplate");
            var clientReferenceTemplate = _configuration.GetSection("EmailNotification:ConfirmationEmail").GetValue<string>("ClientReferenceTemplate");
            var emailReplyToId = _configuration.GetSection("EmailNotification:ConfirmationEmail").GetValue<string>("ReplyToAddressId");

            var greeting = string.Format(greetingTemplate, fullName);
            var clientReference = string.Format(clientReferenceTemplate, locationId, submissionId);

            var personalisation =
                new Dictionary<string, dynamic> {
                    { "greeting", greeting }, { "location", locationName }, {"reference number", submissionReference ?? string.Empty }
                };


            if (!string.IsNullOrEmpty(emailAddress))
            {
                await _notificationService.NotifyByEmailAsync(
                    emailTemplateId, emailAddress, personalisation, clientReference, emailReplyToId
                ).ConfigureAwait(false);
            }

        }

    }
}
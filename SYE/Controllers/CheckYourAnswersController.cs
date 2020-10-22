using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SYE.Helpers;
using SYE.Models;
using SYE.Models.Enum;
using SYE.Models.SubmissionSchema;
using SYE.Repository;
using SYE.Services;
using SYE.Helpers.Enums;
using SYE.Helpers.Extensions;
using SYE.ViewModels;

namespace SYE.Controllers
{
    public class CheckYourAnswersController : BaseController
    {
        private const string _pageId = "CheckYourAnswers";
        private readonly ILogger<CheckYourAnswersController> _logger;
        private readonly ISubmissionService _submissionService;
        private readonly ICosmosService _cosmosService;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly IDocumentService _documentService;
        private readonly ISessionService _sessionService;
        private readonly IPageHelper _pageHelper;

        public CheckYourAnswersController(ILogger<CheckYourAnswersController> logger, ISubmissionService submissionService,
                                          ICosmosService cosmosService, INotificationService notificationService,
                                          IDocumentService documentService, ISessionService sessionService,
                                          IPageHelper pageHelper, IConfiguration configuration)
        {
            _logger = logger;
            _submissionService = submissionService;
            _cosmosService = cosmosService;
            _notificationService = notificationService;
            _documentService = documentService;
            _sessionService = sessionService;
            _pageHelper = pageHelper;
            _configuration = configuration;
        }


        [Microsoft.AspNetCore.Mvc.HttpGet, Microsoft.AspNetCore.Mvc.Route("form/check-your-answers")]
        public IActionResult Index()
        {
            var lastPage = _sessionService.GetLastPage();

            if (lastPage != null && lastPage.Contains("you-have-sent-your-feedback"))
            {
                return GetCustomErrorCode(EnumStatusCode.FormPageAlreadySubmittedError, "Error with user action. Feedback already submitted");
            }

            _sessionService.SetLastPage("form/check-your-answers");

            try
            {
                _sessionService.ClearChangeModeRedirectId();
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
                //check if the user answered the required questions to show this page
                var locationName = _sessionService.GetUserSession().LocationName;
                var defaultLocation = _configuration.GetSection("ApplicationSettings:SiteTextStrings").GetValue<string>("DefaultServiceName");
                var serviceNotFoundPage = _configuration.GetSection("ApplicationSettings").GetValue<string>("ServiceNotFoundPage");
                var formStartPage = _configuration.GetSection("ApplicationSettings").GetValue<string>("FormStartPage");
                var serviceNotFound = locationName.Equals(defaultLocation);
                var pageVm = formVm.Pages.FirstOrDefault(p => p.PageId == _pageId);
                if (!serviceNotFound)
                {
                    //a location has been selected but we may have a service not found visit that we need to remove
                    //or it will be displayed
                    _sessionService.RemoveFromNavOrder(serviceNotFoundPage);
                }

                if (!_pageHelper.CheckPageHistory(pageVm, lastPage, true, _sessionService, null, serviceNotFoundPage, formStartPage, serviceNotFound))
                {
                    //user jumps between pages
                    return GetCustomErrorCode(EnumStatusCode.CYASubmissionHistoryError, "Error with user submission. Page history not found: Id='" + _pageId + "'");
                }

                var vm = new CheckYourAnswersVm
                {
                    FormVm = formVm,
                    SendConfirmationEmail = true,
                    LocationName = _sessionService.GetUserSession().LocationName,
                    PageHistory = _sessionService.GetNavOrder()
                };

                //Setting up variables to ensure we can show the 'another charity' answer if the custom charity is blank.
                var charityQuestionPage = formVm.Pages.FirstOrDefault(p => p.PageId == _configuration
                    .GetSection("ApplicationSettings:PageIdStrings")
                    .GetValue<String>("TellUsWhichCharityPage"));

                var tellUsWhichCharityQuestion = _configuration
                    .GetSection("ApplicationSettings:QuestionStrings:TellUsWhichCharityQuestion")
                    .GetValue<String>("id");

                var anotherCharityAnswer = _configuration
                    .GetSection("ApplicationSettings:QuestionStrings:TellUsWhichCharityQuestion")
                    .GetValue<String>("AnotherCharityAnswer");

                var customCharityQuestion = _configuration
                    .GetSection("ApplicationSettings:QuestionStrings:CustomCharityQuestion")
                    .GetValue<String>("id");

                var anotherCharityFlag = charityQuestionPage
                    .Questions.FirstOrDefault(q => q.QuestionId == tellUsWhichCharityQuestion)?
                    .Answer == anotherCharityAnswer;

                var customCharityBlankFlag = string.IsNullOrEmpty(charityQuestionPage
                    .Questions.FirstOrDefault(q => q.QuestionId == customCharityQuestion)?
                    .Answer);

                ViewBag.Title = "Check your answers" + _configuration.GetSection("ApplicationSettings:SiteTextStrings").GetValue<string>("SiteTitleSuffix");

                ViewBag.TellUsWhichCharityQuestion = tellUsWhichCharityQuestion;
                ViewBag.AnotherCharityFlag = anotherCharityFlag;
                ViewBag.CustomCharityBlankFlag = customCharityBlankFlag;

                return View(vm);
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error loading FormVM.");
                throw ex;
            }

        }


        [Microsoft.AspNetCore.Mvc.HttpPost, Microsoft.AspNetCore.Mvc.Route("form/check-your-answers")]
        [PreventDuplicateRequest]
        [Microsoft.AspNetCore.Mvc.ValidateAntiForgeryToken]
        public IActionResult Index(CheckYourAnswersVm vm)
        {
            try
            {
                var formVm = _sessionService.GetFormVmFromSession();
                if (formVm == null)
                {
                    //session timeout does this
                    return GetCustomErrorCode(EnumStatusCode.CYASubmissionFormNullError, "Error submitting service feedback. Null or empty formVm.");
                }


                // Create first instance of the record, with no gfc_id or word doc
                var submission = GenerateSubmission(formVm, "", "");
                submission = _submissionService.CreateAsync(submission).Result;

                // Get the new Id
                var documentId = _cosmosService.GetDocumentId(submission.Id);
                var seed = _configuration.GetSection("SubmissionDocument").GetValue<int>("DatabaseSeed");

                if (documentId == 0)
                {
                    return GetCustomErrorCode(EnumStatusCode.CYASubmissionReferenceNullError, "Error submitting feedback! Null or empty DocumentId");
                }

                var reference = (seed + documentId).ToString();

                // Update GFC Id, this line is needed for the Word Doc             
                submission.SubmissionId = reference;

                // Create the Word Doc
                var base64Doc = _documentService.CreateSubmissionDocument(submission);

                // Update our model with the new id and word doc
                submission.SubmissionId = reference;
                submission.Base64Attachment = base64Doc;
                submission.Status = SubmissionStatus.Saved.ToString();

                // Update cosmos with our updated record
                submission = _submissionService.UpdateAsync(submission.Id, submission).Result;



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

                //Collate information for confirmation page
                var toldServiceQuestion = _configuration
                    .GetSection("ApplicationSettings:QuestionStrings:ToldServiceQuestion")
                    .GetValue<string>("id");
                var goodBadFeedbackQuestion = _configuration
                    .GetSection("ApplicationSettings:QuestionStrings:GoodBadFeedbackQuestion")
                    .GetValue<string>("id");
                var emailQuestion = _configuration
                    .GetSection("ApplicationSettings:QuestionStrings:EmailQuestion")
                    .GetValue<string>("id");
                var phoneNumberQuestion = _configuration
                    .GetSection("ApplicationSettings:QuestionStrings:PhoneNumberQuestion")
                    .GetValue<string>("id");

                var formalComplaint = _configuration
                    .GetSection("ApplicationSettings:QuestionStrings:ToldServiceQuestion")
                    .GetValue<string>("MadeFormalComplaintAnswer");
                var toldNoComplaint = _configuration
                    .GetSection("ApplicationSettings:QuestionStrings:ToldServiceQuestion")
                    .GetValue<string>("ToldServiceNoComplaintAnswer");
                var goodExperience = _configuration
                    .GetSection("ApplicationSettings:QuestionStrings:GoodBadFeedbackQuestion")
                    .GetValue<string>("GoodFeedbackAnswer");

                var toldServiceAnswer = formVm.GetQuestion(toldServiceQuestion).Answer;

                HttpContext.Session.SetString("OnlyGoodFeedback", formVm.GetQuestion(goodBadFeedbackQuestion).Answer == goodExperience ? "true" : "false");
                HttpContext.Session.SetString("SubmittedEmail", string.IsNullOrEmpty(formVm.GetQuestion(emailQuestion).Answer) ? "false": "true");
                HttpContext.Session.SetString("SubmittedPhoneNumber", string.IsNullOrEmpty(formVm.GetQuestion(phoneNumberQuestion).Answer) ? "false" : "true");
                HttpContext.Session.SetString("AnsweredToldServiceQuestion", !string.IsNullOrWhiteSpace(toldServiceAnswer) ? "true" : "false");
                HttpContext.Session.SetString("MadeComplaint", toldServiceAnswer == formalComplaint ? "true" : "false");

                return RedirectToAction("Index", "Confirmation");
            }
            catch (Exception ex)
            {
                ex.Data.Add("GFCError", "Unexpected error submitting feedback!");
                throw ex;
            }
        }

        private SubmissionVM GenerateSubmission(FormVM formVm, string gfcReference, string base64Doc)
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
                SubmissionId = gfcReference,
                Status = SubmissionStatus.Created.ToString()
            };
            var answers = new List<AnswerVM>();

            var pageHistory = _sessionService.GetNavOrder();
            foreach (var page in formVm.Pages.Where(m => pageHistory.Contains(m.PageId) && m.RemoveFromSubmission == false).OrderBy(m => pageHistory.IndexOf(m.PageId)))
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

            submissionVm.Base64Attachment = base64Doc;

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
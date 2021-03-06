﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SYE.Helpers.Enums;
using SYE.Services;
using SYE.ViewModels;

namespace SYE.Controllers
{
    public class ConfirmationController : BaseController
    {
        private readonly ISessionService _sessionService;
        private IOptions<ApplicationSettings> _config;
        public ConfirmationController(ISessionService sessionService, IOptions<ApplicationSettings> config)
        {
            _sessionService = sessionService;
            _config = config;
        }

        [HttpGet, Route("form/you-have-sent-your-feedback")]
        public IActionResult Index(string id)
        {
            var lastPage = _sessionService.GetLastPage();
            _sessionService.SetLastPage("form/you-have-sent-your-feedback");
            if (lastPage == null || !lastPage.Contains("check-your-answers"))
            {
                return GetCustomErrorCode(EnumStatusCode.ConfirmationPageOutOfSequence, "Confirmation Page hit out of sequence");
            }

            ViewBag.Reference = HttpContext.Session.GetString("ReferenceNumber");
            ViewBag.Title = "You’ve sent your feedback" + _config.Value.SiteTextStrings.SiteTitleSuffix;

            ViewBag.OnlyGoodFeedback = HttpContext.Session.GetString("OnlyGoodFeedback");
            ViewBag.SubmittedEmail = HttpContext.Session.GetString("SubmittedEmail");
            ViewBag.SubmittedPhoneNumber = HttpContext.Session.GetString("SubmittedPhoneNumber");
            ViewBag.AnsweredToldServiceQuestion = HttpContext.Session.GetString("AnsweredToldServiceQuestion");
            ViewBag.MadeComplaint = HttpContext.Session.GetString("MadeComplaint");

            return View();
        }
    }
}
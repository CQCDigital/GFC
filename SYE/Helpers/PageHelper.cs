using GDSHelpers.Models.FormSchema;
using Microsoft.Extensions.Options;
using SYE.Services;
using SYE.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SYE.Models;

namespace SYE.Helpers
{
    public interface IPageHelper
    {
        string GetPreviousPage(PageVM currentPage, ISessionService sessionService, IOptions<ApplicationSettings> config,
            IUrlHelper url, bool serviceNotFound);

        bool CheckPageHistory(PageVM pageVm, string urlReferer, bool checkAnswers, ISessionService sessionService,
            string externalStartPage);
        bool HasNextQuestionBeenAnswered(HttpRequest request, FormVM formVm, PageVM pageVm);
        bool HasAnswerChanged(HttpRequest request, IEnumerable<QuestionVM> questions);
        bool HasPathChanged(HttpRequest request, IEnumerable<QuestionVM> questions);
    }
    [LifeTime(Models.Enums.LifeTime.Scoped)]
    public class PageHelper : IPageHelper
    {
        public string GetPreviousPage(PageVM currentPage, ISessionService sessionService, IOptions<ApplicationSettings> config, IUrlHelper url, bool serviceNotFound)
        {
            var form = sessionService.GetFormVmFromSession();
            var serviceNotFoundPage = config.Value.ServiceNotFoundPage;
            var startPage = config.Value.FormStartPage;
            var targetPage = config.Value.DefaultBackLink;
            var searchUrl = sessionService.GetSearchUrl();

            //Get all the back options for the current page
            var previousPageOptions = currentPage.PreviousPages?.ToList();

            //Check if we dealing with one of the start pages
            if (serviceNotFound && currentPage.PageId == serviceNotFoundPage)
                return searchUrl;

            if (!serviceNotFound && currentPage.PageId == startPage)
                return searchUrl;

            if (serviceNotFound && currentPage.PageId == startPage)
                return url.Action("Index", "Form", new { id = serviceNotFoundPage });

            //Check if we only have 1 option
            if (previousPageOptions.Count() == 1) return url.Action("Index", "Form", new { id = previousPageOptions.FirstOrDefault()?.PageId });

            //Get all the questions in the FormVM
            var questions = form.Pages.SelectMany(m => m.Questions).ToList();

            //Loop through each option and return the pageId when 
            foreach (var pageOption in previousPageOptions)
            {
                var answer = questions.FirstOrDefault(m => m.QuestionId == pageOption.QuestionId)?.Answer;
                if (pageOption.Answer == answer)
                    return url.Action("Index", "Form", new { id = pageOption.PageId });
            }

            return targetPage;
        }

        public bool CheckPageHistory(PageVM pageVm, string urlReferer, bool checkAnswers, ISessionService sessionService, string externalStartPage)
        {            
            if (string.IsNullOrEmpty(urlReferer))
            {
                //direct hit
                return false;
            }

            var pageOk = false;

            if (checkAnswers)// from check your answers only
            {
                //check history
                var pageHistory = sessionService.GetNavOrder();
                var pageIdsToCheck = pageVm.PreviousPages.Select(m => m.PageId).ToList();

                pageOk = (pageIdsToCheck.All(x => pageHistory.Contains(x)));

                if (pageOk)
                {
                    //the required pages have been visited
                    //check if the visited pages have actually been answered?
                    var formVm = sessionService.GetFormVmFromSession();
                    //get all answered questions
                    var answeredPageIds = formVm.Pages
                        .Where(pg => pg.Questions.Any(q => !string.IsNullOrWhiteSpace(q.Answer)))
                        .Select(p => p.PageId);

                    pageOk = pageIdsToCheck.All(x => answeredPageIds.Contains(x));
                }
            }
            else
            {
                var previousPages = pageVm.PreviousPages.Select(m => m.PageId).ToList();
                previousPages.Add("check-your-answers");
                previousPages.Add("search/results");
                previousPages.Add("select-location");
                previousPages.Add("report-a-problem");
                previousPages.Add("feedback-thank-you");

                previousPages.Add(pageVm.PageId);

                if (!string.IsNullOrWhiteSpace(externalStartPage))
                {
                    previousPages.Add(externalStartPage);//we only need the external start page when called from the form controller
                }

                previousPages.Add(pageVm.NextPageId);

                if (pageVm.PathChangeQuestion?.NextPageId != null)
                {
                    previousPages.Add(pageVm.PathChangeQuestion.NextPageId);
                }
                
                foreach (var q in pageVm.Questions)
                {
                    previousPages.AddRange(from np in q.AnswerLogic ?? Enumerable.Empty<AnswerLogicVM>() select np.NextPageId);
                }

                if (previousPages.Any(item => urlReferer.Contains(item)))
                {
                    pageOk = true;
                }

            }
            return pageOk;
        }
        public bool HasAnswerChanged(HttpRequest request, IEnumerable<QuestionVM> questions)
        {
            var changed = true;
            foreach (var question in questions)
            {
                if (request.Form.ContainsKey(question.QuestionId))
                {
                    StringValues newAnswer;
                    request.Form.TryGetValue(question.QuestionId, out newAnswer);
                    changed = (newAnswer.ToString() != question.Answer);
                    if (changed) break;
                }
            }

            return changed;
        }

        public bool HasPathChanged(HttpRequest request, IEnumerable<QuestionVM> questions)
        {
            var pathChange = false;
            //get new answer
            var newAnswer = GetNewAnswer(request, questions);

            var originalAnswer = questions.FirstOrDefault()?.Answer;

            //test logic for potentially changing between two paths
            if (string.IsNullOrWhiteSpace(originalAnswer))
            {
                //no original answer so this is a new path
                pathChange = true;
            }
            else
            {
                var question = questions.FirstOrDefault();

                if (question.AnswerLogic != null)
                {
                    string originalNextPage =
                        (question.AnswerLogic.Where(an => an.Value == originalAnswer).FirstOrDefault() == null
                            ? string.Empty
                            : question.AnswerLogic.Where(an => an.Value == originalAnswer)
                                .FirstOrDefault().NextPageId);
                    string newNextPage =
                        (question.AnswerLogic.Where(an => an.Value == newAnswer).FirstOrDefault() == null
                            ? string.Empty
                            : question.AnswerLogic.Where(an => an.Value == newAnswer)
                                .FirstOrDefault().NextPageId);
                    pathChange = (originalNextPage != newNextPage);
                }
            }

            return pathChange;
        }
        /// <summary>
        /// returns true if the next question in the path has been answered
        /// </summary>
        /// <param name="formVm"></param>
        /// <param name="pageVm"></param>
        /// <returns></returns>
        public bool HasNextQuestionBeenAnswered(HttpRequest request, FormVM formVm, PageVM pageVm)
        {
            var isAnswered = false;
            var newAnswer = GetNewAnswer(request, pageVm.Questions);
            if (!string.IsNullOrWhiteSpace(newAnswer))
            {
                var question = pageVm.Questions.FirstOrDefault();
                string nextPageId = string.Empty;
                if (question.AnswerLogic != null)
                {
                    nextPageId =
                        (question.AnswerLogic.Where(an => an.Value == newAnswer).FirstOrDefault() == null
                            ? string.Empty
                            : question.AnswerLogic.Where(an => an.Value == newAnswer)
                                .FirstOrDefault().NextPageId);
                }

                if (string.IsNullOrWhiteSpace(nextPageId))
                {
                    nextPageId = pageVm.NextPageId;
                }
                var nextPageVm = formVm.Pages.Where(p => p.PageId == nextPageId).FirstOrDefault();
                isAnswered = !string.IsNullOrWhiteSpace(nextPageVm.Questions.FirstOrDefault().Answer);
            }

            return isAnswered;
        }

        private string GetNewAnswer(HttpRequest request, IEnumerable<QuestionVM> questions)
        {
            //get new answer
            var newAnswer = string.Empty;
            foreach (var question in questions)
            {
                if (request.Form.ContainsKey(question.QuestionId))
                {
                    StringValues answer;
                    request.Form.TryGetValue(question.QuestionId, out answer);
                    newAnswer = answer.ToString();
                    break;
                }
            }

            return newAnswer;
        }

    }
}

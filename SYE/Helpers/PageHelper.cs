using GDSHelpers.Models.FormSchema;
using Microsoft.Extensions.Options;
using SYE.Services;
using SYE.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Math;
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

        bool CheckPageHistory(PageVM pageVm, string urlReferer, bool checkAnswers, ISessionService sessionService, string externalStartPage, string serviceNoteFoundPage, string formStartPage, bool serviceNotFound);
        bool HasNextQuestionBeenAnswered(HttpRequest request, FormVM formVm, PageVM pageVm);
        bool HasAnswerChanged(HttpRequest request, IEnumerable<QuestionVM> questions);
        bool IsQuestionAnswered(HttpRequest request, IEnumerable<QuestionVM> questions);
        bool HasPathChanged(HttpRequest request, IEnumerable<QuestionVM> questions);
        string GetNextPageIdFromPage(FormVM formVm, string pageId);
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
            var cqcStart = config.Value.GFCUrls.StartPage;

            //Get all the back options for the current page
            var previousPageOptions = currentPage.PreviousPages?.ToList();

            //Check if we dealing with one of the start pages
            if (currentPage.PageId == serviceNotFoundPage)
                return searchUrl;

            if (currentPage.PageId == startPage)
                return cqcStart;

            //Check if we only have 1 option
            if (previousPageOptions.Count() == 1)
                return url.Action("Index", "Form", new { id = previousPageOptions.FirstOrDefault()?.PageId });

            //Get all the questions and formData in the FormVM
            var questions = form.Pages.SelectMany(m => m.Questions).ToList();
            var formData = form.SubmissionData?.ToList();

            //Loop through each option and return the pageId when 
            foreach (var pageOption in previousPageOptions)
            {
                //If the 'question' can't be found in the question list, try the formData list as well.
                var answer = questions.FirstOrDefault(m => m.QuestionId == pageOption.QuestionId)?.Answer
                             ?? formData?.FirstOrDefault(d => d.Id == pageOption.QuestionId)?.Value;
                             

                //If we're looking at answers to the Search question, set answer here instead:
                if (pageOption.QuestionId == "search")
                {
                    answer = serviceNotFound ? "locationNotFound" : "locationFound";
                }

                //If we match answer
                if (pageOption.Answer == answer)
                    //|| (string.IsNullOrEmpty(pageOption.QuestionId) && string.IsNullOrEmpty(pageOption.Answer))) //Looked at handling option to send user to 'default' page this way; not implemented.
                {
                    //If we're sending the user back to 'search', we can send them back to their search results if they picked a location, or to the 'tell-us-which-service' page
                    if (pageOption.PageId == "search")
                    {
                        //If user did not pick a location
                        if (serviceNotFound)
                            return url.Action("Index", "Form", new { id = "tell-us-which-service" });

                        //If user picked a service on CQC site, search cannot have been the previous page ==> go to next previousPage
                        var fromCqc = form.SubmissionData?.FirstOrDefault(x => x.Id == "LocationFromCqcFlag")?.Value;
                        if (fromCqc != null)
                        {
                            if (string.IsNullOrWhiteSpace(searchUrl))
                            {
                                //cannot go back to search so go to start
                                return url.Action("Index", "Form", new { id = startPage });
                            }
                            continue;//don't think we need this
                        }

                        //If we get here, return user to their search results
                        return searchUrl;
                    }
                    return url.Action("Index", "Form", new { id = pageOption.PageId });
                }
            }

            return targetPage;
        }

        public bool CheckPageHistory(PageVM pageVm, string urlReferer, bool checkAnswers, ISessionService sessionService, string externalStartPage, string serviceNoteFoundPage, string formStartPage, bool serviceNotFound)
        {            
            if (string.IsNullOrEmpty(urlReferer))
            {
                //direct hit
                return false;
            }

            var pageOk = false;
            var formVm = sessionService.GetFormVmFromSession();
            if (checkAnswers)// from check your answers only
            {
                //check history
                var pageHistory = sessionService.GetNavOrder();
                var pageIdsToCheck = pageVm.PreviousPages.Select(m => m.PageId).ToList();
                if (serviceNotFound)
                {
                    //swap out search with service not found
                    pageIdsToCheck.Remove("search");
                    pageIdsToCheck.Add(serviceNoteFoundPage);
                }
                pageOk = (pageIdsToCheck.All(x => pageHistory.Contains(x)));
            }
            else
            {
                var previousPages = pageVm.PreviousPages.Select(m => m.PageId).ToList();
                previousPages.Add("check-your-answers");
                previousPages.Add("search/results");
                previousPages.Add("search/find-a-service");
                previousPages.Add("select-location");
                previousPages.Add("report-a-problem");
                previousPages.Add("feedback-thank-you");
                previousPages.Add("start-questions");
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

                if (previousPages.Any(urlReferer.Contains))
                {
                    pageOk = true;
                }
            }
            if (pageOk)
            {
                pageOk = CheckPathToStart(pageVm.PageId, formVm, serviceNoteFoundPage, formStartPage, serviceNotFound);
            }

            return pageOk;
        }

        /// <summary>
        /// this is a recursive method to track back down the question path ensuring each question is valid
        /// The method looks for previous page of the passed page id and looks for a valid answer.
        /// It then recursively calls itself with the previous page id
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="formVm"></param>
        /// <param name="formStartPage"></param>
        /// <param name="serviceNotFound"></param>
        /// <param name="serviceNoteFoundPage"></param>
        /// <returns></returns>
        private bool CheckPathToStart(string pageId, FormVM formVm, string serviceNoteFoundPage, string formStartPage, bool serviceNotFound)
        {
            //var startPageId = serviceNotFound ? serviceNoteFoundPage : formStartPage;
            var startPageId = formStartPage;
            var returnBool = false;

            if (pageId == startPageId)
            {
                //there's no previous page
                return true;
            }

            //look for previous pages pointing to this page
            var previousPathchangePages = new List<PageVM>();
            var previousLogicPages = new List<PageVM>();
            
            var previousPages = formVm.Pages.Where(p => p.NextPageId == pageId).ToList();


            //see if this page has a changePath object that may be the calling page
            var thisPage = formVm.Pages.FirstOrDefault(p => p.PageId == pageId);
            if (thisPage?.PathChangeQuestion != null)
            {
                var question = formVm.Pages.SelectMany(p => p.Questions.Where(q => q.QuestionId == thisPage.PathChangeQuestion.QuestionId)).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(question?.Answer))
                {
                    if (question.Answer == thisPage.PathChangeQuestion.Answer)
                    {
                        var changePathPage = formVm.Pages.FirstOrDefault(p => p.Questions.Any(q => q.QuestionId == thisPage.PathChangeQuestion.QuestionId));

                        return changePathPage != null && CheckPathToStart(changePathPage.PageId, formVm, serviceNoteFoundPage, formStartPage, serviceNotFound);
                    }
                    else
                    {
                        //if the answer in any of the previous pages answer logic is the same as this answer then
                        //remove it as it doesn't apply to this path
                        foreach (var page in from page in previousPages from qstn in page.Questions where qstn.AnswerLogic.Any(al => al.Value == question.Answer) select page)
                        {
                            previousPages.Remove(page);
                            break;
                        }
                    }
                }
            }

            SetupPreviousPagesLists(pageId, formVm, ref previousPages, ref previousLogicPages, ref previousPathchangePages);

            //go through all potential previous pages and check for answers
            foreach (var page in previousPathchangePages.OrderByDescending(p=>p.DisplayOrder))
            {
                var pathChangeQuestion = page.PathChangeQuestion;
                //get the relevant question
                var question = formVm.Pages.SelectMany(p => p.Questions.Where(q => q.QuestionId == pathChangeQuestion.QuestionId)).FirstOrDefault();

                if (question != null && question.Answer == pathChangeQuestion.Answer && (page.Questions.Any(q => !string.IsNullOrWhiteSpace(q.Answer))))
                {
                    //ok we're valid with this question
                    return CheckPathToStart(page.PageId, formVm, serviceNoteFoundPage, formStartPage, serviceNotFound);
                }
            }

            foreach (var page in previousLogicPages.OrderByDescending(p => p.DisplayOrder))
            {
                var questions = page.Questions.Where(q => q.AnswerLogic != null).Where(a => a.AnswerLogic.Any(x => x.NextPageId == pageId)).ToList();
                foreach (var question in questions)
                {
                    foreach (var answerLogic in question.AnswerLogic)
                    {
                        if (answerLogic.Value == question.Answer)
                        {
                            //ok we're valid with this question
                            return CheckPathToStart(page.PageId, formVm, serviceNoteFoundPage, formStartPage, serviceNotFound);
                        }
                    }
                }
            }
            //no valid answer logic questions take us here
            foreach (var page in previousPages.OrderByDescending(p => p.DisplayOrder))
            {
                var answerValid = false;
                //If a valid previous page for this page is the serviceNotFound page, but the user did find a service, they will never have visited the serviceNotFound page ==> skip this page (by setting answerValid to 'true')
                if (page.PageId == serviceNoteFoundPage && (serviceNotFound == false))
                {
                    //location was found so ignore this
                    answerValid = true;
                }
                else
                {
                    //If the page had any required questions...
                    if (page.Questions.Any(q => q.Validation.Required.IsRequired.Equals(true)))
                    {
                        //...the page is valid if at least one question has an answer.
                        if (page.Questions.Any(q => !string.IsNullOrWhiteSpace(q.Answer)))
                        {
                            answerValid = true;
                        }
                    }
                    else
                    {
                        //Otherwise this is an information page (or only has optional questions) - so doesn't need answering
                        answerValid = true;
                    }
                }

                if (answerValid)
                {
                    returnBool = CheckPathToStart(page.PageId, formVm, serviceNoteFoundPage, formStartPage, serviceNotFound);
                    break;
                }
            }

            return returnBool;
        }

        private void SetupPreviousPagesLists(string pageId, FormVM formVm, ref List<PageVM> previousPages, ref List<PageVM> previousLogicPages, ref List<PageVM> previousPathchangePages)
        {
            var invalidPreviousPages = new List<PageVM>();

            if (previousPages.Count > 1)
            {
                //there is more then one entry to this page
                foreach (var page in previousPages)
                {
                    foreach (var page2 in previousPages.Where(p => p.PageId != page.PageId))
                    {
                        foreach (var question in page2.Questions.Where(q => q.AnswerLogic != null))
                        {
                            foreach (var al in question.AnswerLogic)
                            {
                                if (al.NextPageId == page.PageId)
                                {
                                    invalidPreviousPages.Add(al.Value == question.Answer ? page2 : page);
                                }
                            }
                        }
                    }
                }
            }
            //remove invalid previous pages
            foreach (var page in invalidPreviousPages)
            {
                previousPages.Remove(page);
            }
            //see if there are any previous pages with logic answers pointing to this page
            foreach (var pge in formVm.Pages)
            {
                var questions = pge.Questions.Where(q => q.AnswerLogic != null).Where(a => a.AnswerLogic.Any(x => x.NextPageId == pageId)).ToList();
                if (questions.Count > 0)
                {
                    previousLogicPages.Add(pge);
                }

                if (pge.PathChangeQuestion != null)
                {
                    if (pge.PathChangeQuestion.NextPageId == pageId)
                    {
                        previousPathchangePages.Add(pge);
                    }
                }
            }

            var thisPage = formVm.Pages.FirstOrDefault(p => p.PageId == pageId);
            if (!string.IsNullOrWhiteSpace(thisPage?.NextPageReferenceId))
            {
                var referencePage = formVm.Pages.FirstOrDefault(p => p.PageId == thisPage.NextPageReferenceId);
                if (! previousPages.Contains(referencePage))
                {
                    previousPages.Add(referencePage);
                }
            }
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
        public bool IsQuestionAnswered(HttpRequest request, IEnumerable<QuestionVM> questions)
        {
            var questionAnswered = false;
            foreach (var question in questions)
            {
                if (request.Form.ContainsKey(question.QuestionId))
                {
                    StringValues newAnswer;
                    request.Form.TryGetValue(question.QuestionId, out newAnswer);
                    questionAnswered = !(string.IsNullOrWhiteSpace(newAnswer.ToString()));
                    if (questionAnswered) break;
                }
            }

            return questionAnswered;
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

        public string GetNextPageIdFromPage(FormVM formVm, string pageId)
        {
            var nextPageId = string.Empty;
            var referencePage = formVm.Pages.FirstOrDefault(p => p.PageId == pageId);
            if (referencePage?.Questions != null)
            {
                nextPageId = referencePage.NextPageId;
                foreach (var question in referencePage.Questions)
                {
                    foreach (var answerLogic in question.AnswerLogic)
                    {
                        if (answerLogic.Value == question.Answer)
                        {
                            nextPageId = answerLogic.NextPageId;
                        }
                    }
                }
            }

            return nextPageId;
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

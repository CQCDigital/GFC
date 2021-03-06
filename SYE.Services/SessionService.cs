﻿using System;
using System.Collections.Generic;
using System.Linq;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SYE.Models;
using SYE.Models.Enums;

namespace SYE.Services
{
    public interface ISessionService
    {
        string GetSessionId();
        void ClearNavOrder();
        void UpdateNavOrder(string currentPage, string serviceNotFoundPageId = "");
        void RemoveFromNavOrder(string pageToRemove);
        void UpdateNavOrderAtRedirectTrigger(string currentPage, string redirectTriggerPage);
        void RemoveNavOrderSectionFrom(string fromPage, string toPage);
        void RemoveNavOrderFrom(string fromPage);
        List<string> GetNavOrder();
        PageVM GetPageById(string pageId, bool notFoundFlag);
        FormVM LoadLatestFormIntoSession(Dictionary<string, string> replacements);
        void SetUserSessionVars(UserSessionVM vm);
        UserSessionVM GetUserSession();
        void SaveFormVmToSession(FormVM vm);
        void SaveFormVmToSession(FormVM vm, Dictionary<string, string> replacements);
        DataItemVM GetFormData(string id);
        void UpdateFormData(IEnumerable<DataItemVM> dataItems);
        FormVM GetFormVmFromSession();
        void UpdatePageVmInFormVm(PageVM vm);
        void SaveUserSearch(string search);
        string GetUserSearch();
        void SaveSearchUrl(string searchUrl);
        string GetSearchUrl();
        void SetCookieFlagOnSession(string cookieAccepted);
        string GetCookieFlagFromSession();
        void SetRedirectionCookie(string userRedirected);
        string GetRedirectionCookie();
        void ClearSession();
        string PageForEdit { get; set; }
        bool ChangedLocationMode { get; set; }
        void SaveChangeModeRedirectId(string redirectPageId);
        string GetChangeModeRedirectId();
        void ClearChangeModeRedirectId();
        void SaveChangeMode(string changeMode);
        string GetChangeMode();
        void ClearChangeMode();
        void SetLastPage(string lastPage);
        string GetLastPage();
        string PageOrder { get; set; }
        string CheckboxClick { get; set; }
    }

    [LifeTime(LifeTime.Scoped)]
    public class SessionService : ISessionService
    {
        private readonly IFormService _formService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        const string schemaKey = "sye_form_schema";

        public SessionService(IFormService formService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _formService = formService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }


        public string GetSessionId()
        {
            var context = _httpContextAccessor.HttpContext;
            return context.Session.Id;
        }

        public void UpdateNavOrder(string currentPage, string serviceNotFoundPageId)
        {
            var userSession = GetUserSession();

            if (userSession.NavOrder == null)
            {
                //First page
                userSession.NavOrder = new List<string> { currentPage };
            }
            else
            {
                //If we've not been here before add the page
                if (!userSession.NavOrder.Contains(currentPage))
                {
                    // If the current page is the service not found page
                    // make sure we add it straight after the search entry
                    if (currentPage.Equals(serviceNotFoundPageId, StringComparison.OrdinalIgnoreCase))
                    {
                        if (userSession.NavOrder.Contains("Search", StringComparer.OrdinalIgnoreCase))
                        {
                            var searchPos = userSession.NavOrder.FindIndex(m => m.Equals("Search", StringComparison.OrdinalIgnoreCase));
                            userSession.NavOrder.Insert(searchPos + 1, currentPage);
                        }
                        else
                        {
                            userSession.NavOrder.Add(currentPage);
                        }
                    }
                    else {
                        userSession.NavOrder.Add(currentPage);
                    }                    
                }
                else
                {
                    //been here before......now editing
                    PageForEdit = currentPage;
                }
            }

            SetUserSessionVars(userSession);
            EnsureNavOrder();
        }

        public void RemoveFromNavOrder(string pageToRemove)
        {
            var userSession = GetUserSession();

            if (userSession.NavOrder != null)
            {
                //If we've not been here before add the page
                if (userSession.NavOrder.Contains(pageToRemove))
                {
                    userSession.NavOrder.Remove(pageToRemove);
                }
            }

            SetUserSessionVars(userSession);

        }

        /// <summary>
        /// Insert current page into the nav order before the redirectTrigger page
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="redirectTriggerPage"></param>
        public void UpdateNavOrderAtRedirectTrigger(string currentPage, string redirectTriggerPage)
        {
            if (currentPage != redirectTriggerPage)
            {
                var userSession = GetUserSession();

                //add the current page to the nav order if it doesn't exist
                if (!userSession.NavOrder.Contains(currentPage))
                {
                    var index = userSession.NavOrder.IndexOf(redirectTriggerPage);
                    //Handling edge cases where index is missing i.e. the redirectTriggerPage isn't in the NavOrder:
                    // => manually re-specify indexTo, defaulting to an arbitrarily large number to remove all subsequent pages
                    if (index == -1)
                    {
                        switch (currentPage)
                        {
                            case ("contact-information"):
                                index = userSession.NavOrder.IndexOf("did-you-hear-about-this-form-from-a-charity");
                                break;
                        }
                    }

                    //Only update the navOrder if we now have an appropriate trigger page
                    if (index != -1)
                    {
                        userSession.NavOrder.Insert(index, currentPage);
                        SetUserSessionVars(userSession);
                        EnsureNavOrder();
                    }
                }
            }
        }

        private void EnsureNavOrder()
        {
            var newNavOrder = new List<string>();
            var userSession = GetUserSession();

            if (userSession.NavOrder != null)
            {
                newNavOrder.AddRange(this.PageOrder.Split(",").Where(pageId => userSession.NavOrder.Contains(pageId)));
                userSession.NavOrder = newNavOrder;
                SetUserSessionVars(userSession);
            }
        }
        public List<string> GetNavOrder()
        {
            var userSession = GetUserSession();
            return userSession.NavOrder ?? new List<string>();
        }
        public void ClearNavOrder()
        {
            var context = _httpContextAccessor.HttpContext;
            context.Session.Remove("NavOrder");
        }

        public PageVM GetPageById(string pageId, bool notFoundFlag)
        {
            var formVm = GetFormVmFromSession();

            if (string.IsNullOrWhiteSpace(pageId))
            {
                return notFoundFlag
                    ? formVm.Pages.FirstOrDefault()
                    : formVm.Pages.FirstOrDefault(x => x.PageId != formVm.Pages.First().PageId);
            }

            return formVm.Pages.Any(x => x.PageId == pageId)
                ? formVm.Pages.FirstOrDefault(m => m.PageId == pageId)
                : null;
        }

        public FormVM LoadLatestFormIntoSession(Dictionary<string, string> replacements)
        {
            string formName = _configuration.GetSection("FormsConfiguration:ServiceForm").GetValue<string>("Name");
            string version = _configuration.GetSection("FormsConfiguration:ServiceForm").GetValue<string>("Version");
            var context = _httpContextAccessor.HttpContext;
            string sessionVersion = context.Session.GetString("FormVersion");

            if (!string.IsNullOrWhiteSpace(sessionVersion))
            {
                version = sessionVersion;
            }

            var form = string.IsNullOrEmpty(version) ?
                _formService.GetLatestFormByName(formName).Result :
                _formService.FindByNameAndVersion(formName, version).Result;

            var json = JsonConvert.SerializeObject(form);

            if (replacements?.Count > 0)
            {
                foreach (var item in replacements)
                {
                    json = json.Replace(item.Key, item.Value);
                }
            }

            var formVm = JsonConvert.DeserializeObject<FormVM>(json);
            SaveFormVmToSession(formVm);

            return formVm;
        }

        public void SetUserSessionVars(UserSessionVM vm)
        {
            var context = _httpContextAccessor.HttpContext;
            context.Session.SetString("ProviderId", vm.ProviderId ?? "");
            context.Session.SetString("LocationId", vm.LocationId ?? "");
            context.Session.SetString("LocationName", vm.LocationName ?? "");

            if (vm.NavOrder != null)
            {
                var pageList = string.Join(",", vm.NavOrder.ToArray<string>());
                context.Session.SetString("NavOrder", pageList ?? "");
            }
        }

        public UserSessionVM GetUserSession()
        {
            var context = _httpContextAccessor.HttpContext;
            var userSessionVm = new UserSessionVM
            {
                ProviderId = context.Session.GetString("ProviderId"),
                LocationId = context.Session.GetString("LocationId"),
                LocationName = context.Session.GetString("LocationName")
            };

            if (!string.IsNullOrEmpty(context.Session.GetString("NavOrder")))
                userSessionVm.NavOrder = context.Session.GetString("NavOrder").Split(',').ToList();

            return userSessionVm;
        }

        public void SaveFormVmToSession(FormVM vm)
        {
            var context = _httpContextAccessor.HttpContext;
            context.Session.SetString(schemaKey, JsonConvert.SerializeObject(vm));
        }

        public void SaveFormVmToSession(FormVM vm, Dictionary<string, string> replacements)
        {
            var context = _httpContextAccessor.HttpContext;
            var json = JsonConvert.SerializeObject(vm);

            if (replacements?.Count > 0)
            {
                foreach (var item in replacements)
                {
                    json = json.Replace(item.Key, item.Value);
                }
            }
            context.Session.SetString(schemaKey, json);
        }

        public DataItemVM GetFormData(string id)
        {
            var formVm = GetFormVmFromSession();
            return formVm?.SubmissionData?.FirstOrDefault(x => x.Id == id) ?? null;
        }

        /// <summary>
        /// Adds a data item to the form with the specified values, or replaces it if it already exists
        /// </summary>
        /// <param name="dataItems">List of DataItemVMs to update - contains string fields ID, Notes, and Value</param>
        public void UpdateFormData(IEnumerable<DataItemVM> dataItems)
        {
            var formVm = GetFormVmFromSession();

            //Create new local object from the dataItems in the form
            var dataItemsInForm = formVm.SubmissionData != null ? formVm.SubmissionData.ToList() : new List<DataItemVM>();

            foreach (var dataItem in dataItems)
            {
                //Update this local dataItems list
                var index = dataItemsInForm.FindIndex(x => x.Id == dataItem.Id);
                if (index != -1) //i.e. the data item already exists
                {
                    dataItemsInForm[index] = dataItem;
                }
                else
                {
                    dataItemsInForm.Add(dataItem);
                }
            }

            //Assign the new dataItems back to the formVm
            formVm.SubmissionData = dataItemsInForm;
            SaveFormVmToSession(formVm);
        }

        public FormVM GetFormVmFromSession()
        {
            var context = _httpContextAccessor.HttpContext;
            var json = context.Session.GetString(schemaKey);
            return json == null ? default(FormVM) : JsonConvert.DeserializeObject<FormVM>(json);
        }

        public void UpdatePageVmInFormVm(PageVM vm)
        {
            var formVm = GetFormVmFromSession();
            var currentPage = formVm.Pages.FirstOrDefault(m => m.PageId == vm.PageId)?.Questions;
            foreach (var question in vm.Questions)
            {
                var q = currentPage.FirstOrDefault(m => m.QuestionId == question.QuestionId);
                q.Answer = question.Answer;
            }
            SaveFormVmToSession(formVm);
        }

        public void SaveUserSearch(string search)
        {
            var context = _httpContextAccessor.HttpContext;
            context.Session.SetString("Search", search);
        }

        public string GetUserSearch()
        {
            var context = _httpContextAccessor.HttpContext;
            return context.Session.GetString("Search");
        }
        public void SaveSearchUrl(string searchUrl)
        {
            var context = _httpContextAccessor.HttpContext;
            context.Session.SetString("SearchUrl", searchUrl);
        }
        public string GetSearchUrl()
        {
            var context = _httpContextAccessor.HttpContext;
            return context.Session.GetString("SearchUrl");
        }

        public void SetCookieFlagOnSession(string cookieAccepted)
        {
            var context = _httpContextAccessor.HttpContext;
            context.Session.SetString("CookieAccepted", cookieAccepted);
        }

        public string GetCookieFlagFromSession()
        {
            var context = _httpContextAccessor.HttpContext;
            return context.Session.GetString("CookieAccepted");
        }

        public void SetRedirectionCookie(string userRedirected)
        {
            var context = _httpContextAccessor.HttpContext;
            context.Session.SetString("UserRedirected", userRedirected);
        }

        public string GetRedirectionCookie()
        {
            var context = _httpContextAccessor.HttpContext;
            return context.Session.GetString("UserRedirected");
        }

        public void ClearSession()
        {
            var context = _httpContextAccessor.HttpContext;
            context.Session.Remove("ProviderId");
            context.Session.Remove("LocationId");
            context.Session.Remove("LocationName");
            context.Session.Remove("NavOrder");
            context.Session.Remove("SearchUrl");
            context.Session.Remove("ChangeModeRedirectId");
            context.Session.Remove("PageForEdit");
            context.Session.Remove("ChangeMode");
            context.Session.Remove("LastPage");
            context.Session.Remove("PageOrder");

            //context.Session.Clear();
        }
        public void RemoveNavOrderFrom(string fromPage)
        {
            var userSession = GetUserSession();
            var newNav = new List<string>();

            var index = userSession.NavOrder.IndexOf(fromPage);
            foreach (var page in userSession.NavOrder)
            {
                if (userSession.NavOrder.IndexOf(page) <= index) newNav.Add(page);
            }

            //Update the users navigation history
            userSession.NavOrder = newNav;
            SetUserSessionVars(userSession);
            EnsureNavOrder();
        }
        public void RemoveNavOrderSectionFrom(string fromPage, string toPage)
        {
            var userSession = GetUserSession();
            var newNav = new List<string>();

            var indexFrom = userSession.NavOrder.IndexOf(fromPage);
            var indexTo = userSession.NavOrder.IndexOf(toPage);

            //Handling edge cases where indexTo is missing: manually re-specify indexTo, defaulting to an arbitrarily large number to remove all subsequent pages
            if (indexTo == -1)
            {
                switch (fromPage)
                {
                    case ("can-we-contact-you"):
                        indexTo = userSession.NavOrder.IndexOf("did-you-hear-about-this-form-from-a-charity");
                        break;
                    default:
                        indexTo = 999;
                        break;
                }
            }
            
            foreach (var page in userSession.NavOrder)
            {
                if (userSession.NavOrder.IndexOf(page) <= indexFrom || userSession.NavOrder.IndexOf(page) >= indexTo)
                    newNav.Add(page);
            }

            //Update the users navigation history
            userSession.NavOrder = newNav;
            SetUserSessionVars(userSession);
            EnsureNavOrder();
        }

        public void SaveChangeModeRedirectId(string redirectPageId)
        {
            var context = _httpContextAccessor.HttpContext;
            context.Session.SetString("ChangeModeRedirectId", redirectPageId);
        }

        public string GetChangeModeRedirectId()
        {
            var context = _httpContextAccessor.HttpContext;
            return context.Session.GetString("ChangeModeRedirectId");
        }

        public void ClearChangeModeRedirectId()
        {
            var context = _httpContextAccessor.HttpContext;
            context.Session.Remove("ChangeModeRedirectId");
        }
        public string GetChangeMode()
        {
            var context = _httpContextAccessor.HttpContext;
            return context.Session.GetString("ChangeMode");
        }
        public void SaveChangeMode(string changeMode)
        {
            var context = _httpContextAccessor.HttpContext;
            context.Session.SetString("ChangeMode", changeMode);
        }

        public void ClearChangeMode()
        {
            var context = _httpContextAccessor.HttpContext;
            context.Session.Remove("ChangeMode");
        }

        public string PageForEdit
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                return context.Session.GetString("PageForEdit");
            }
            set
            {
                var context = _httpContextAccessor.HttpContext;
                context.Session.SetString("PageForEdit", value);
            }
        }
        public bool ChangedLocationMode
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                var val = context.Session.GetString("ChangedLocationMode");
                return !string.IsNullOrWhiteSpace(val) && bool.Parse(val);
            }
            set
            {
                var context = _httpContextAccessor.HttpContext;
                context.Session.SetString("ChangedLocationMode", value.ToString());
            }
        }

        public void SetLastPage(string lastPage)
        {
            var context = _httpContextAccessor.HttpContext;
            context.Session.SetString("LastPage", lastPage);
        }

        public string GetLastPage()
        {
            var context = _httpContextAccessor.HttpContext;
            return context.Session.GetString("LastPage");
        }

        public string PageOrder
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                return context.Session.GetString("PageOrder");
            }
            set
            {
                var context = _httpContextAccessor.HttpContext;
                context.Session.SetString("PageOrder", value);
            }
        }

        public string CheckboxClick
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                return context.Session.GetString("CheckboxClick");
            }
            set
            {
                var context = _httpContextAccessor.HttpContext;
                context.Session.SetString("CheckboxClick", value);
            }
        }
    }

}

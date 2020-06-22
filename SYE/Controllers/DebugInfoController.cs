using System;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SYE.Helpers;
using SYE.Services;
using SYE.ViewModels;
using SYE.ViewModels.Debug;

namespace SYE.Controllers
{

    public class DebugInfoController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly ISessionService _sessionSvc;
        private readonly IOptions<ApplicationSettings> _config;

        public DebugInfoController(IHostingEnvironment env, ISessionService sessionSvc, IOptions<ApplicationSettings> config)
        {
            _env = env;
            _sessionSvc = sessionSvc;
            _config = config;
        }

        public IActionResult Index(string id = "")
        {
            if (_env.IsLocal() == false) return PartialView("_NoAccess");

            
            var vm = new DebugInfoVm();
            var currentSession = _sessionSvc.GetUserSession();
            var serviceNotFound = currentSession.LocationName?.Equals(_config.Value.SiteTextStrings.DefaultServiceName) ?? false;

            vm.UserSession = currentSession;
            vm.NavOrder = currentSession?.NavOrder;
            vm.ServiceNotFound = serviceNotFound;
            vm.CurrentPage = string.IsNullOrEmpty(id) ? null : _sessionSvc.GetPageById(id, serviceNotFound);
            vm.ChangeMode = _sessionSvc.GetChangeMode();

            return PartialView("_Debug", vm);
        }
    }
}
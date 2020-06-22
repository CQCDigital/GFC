using Microsoft.AspNetCore.Mvc;
using SYE.Services;

namespace SYE.Controllers
{
    public class SessionController : Controller
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [Route("we-have-reset-this-form")]
        public IActionResult Index()
        {
            _sessionService.ClearSession();

            return View();
        }
    }
}
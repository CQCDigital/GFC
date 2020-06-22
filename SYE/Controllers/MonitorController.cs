using System;
using Microsoft.AspNetCore.Mvc;
using SYE.Services;

namespace SYE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonitorController : ControllerBase
    {
        private readonly ISessionService _sessionSvc;

        public MonitorController(ISessionService sessionSvc)
        {
            _sessionSvc = sessionSvc;
        }

        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            return Ok();
        }

        [HttpGet("Log")]
        public IActionResult Log(long ticks, string url, string action)
        {
            var myDate = new DateTime(ticks);
            var sessionId = _sessionSvc.GetSessionId();

            return Ok();
        }

    }

}
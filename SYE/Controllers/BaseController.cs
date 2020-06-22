using Microsoft.AspNetCore.Mvc;
using SYE.Helpers.Enums;
using SYE.Models.Response;

namespace SYE.Controllers
{
    public abstract class BaseController : Controller
    {
        public IActionResult GetCustomErrorCode(EnumStatusCode enumStatusCode, string message)
        {
            return new StatusResult((int)enumStatusCode, message, Response.HttpContext);
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using SYE.ViewModels;

namespace SYE.Controllers
{
    public class HowWeUseYourInformationController : Controller
    {
        [Route("how-we-handle-information")]
        public IActionResult Index()
        {
            ViewData["HomeLink"] = false;
            return View();
        }
    }
}
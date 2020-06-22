using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SYE.ViewModels;

namespace SYE.Controllers
{
    public class AccessibilityController : Controller
    {
        [Route("accessibility")]
        public IActionResult Index()
        {
            ViewData["HomeLink"] = false;
            return View();
        }
    }
}
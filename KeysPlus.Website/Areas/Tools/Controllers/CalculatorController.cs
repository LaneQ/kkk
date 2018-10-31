using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KeysPlus.Website.Areas.Tools.Controllers
{
    [Authorize]
    public class CalculatorController : Controller
    {
        // GET: Tools/Calculator

        public ActionResult InvesmentCalculator()
        {
            return View();
        }
    }
}
using KeysPlus.Data;
using KeysPlus.Website.Areas.Companies.Models;
using KeysPlus.Website.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Runtime.Remoting.Channels;
using KeysPlus.Website.Areas.Tools;
using KeysPlus.Service.Models;
using KeysPlus.Service.Services;
//using CompanyViewModel = KeysPlus.Service.Models.CompanyViewModel;

namespace KeysPlus.Website.Areas.Companies.Controllers
{
    [Authorize]
    public class OnboardingController : Controller
    {
        // GET: Companies/Onboarding
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewServiceProvider (CompanyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = User.Identity.Name;
                var login = AccountService.GetLoginByEmail(user);
                var result = CompanyService.AddNewCompany(model, login, Request.Files);
                return Json(new { Success = result.IsSuccess, Errors = result.ErrorMessage?? "" });
               
            }
            else
            {
                return Json(new { Success = false});
            }
            
        }
        
        
    }
}
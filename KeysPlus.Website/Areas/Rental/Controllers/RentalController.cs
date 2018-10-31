using KeysPlus.Data;
using KeysPlus.Service.Models;
using KeysPlus.Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KeysPlus.Website.Areas.Rental.Controllers
{
    public class RentalController : Controller
    {
        KeysEntities db = new KeysEntities();
        public ActionResult Index()
        {
           
            return View();
        }

        [HttpGet]
        public ActionResult GetSchoolInfo(FindSchoolsModel model)
        {
            model.SchoolTypeCommon = "All";
            model.DistanceBounding = 2;
            var data = DataService.ExecuteStoredProcedure<FindSchoolsResultModel>("sp_FindNearestSchool", model);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTransportInfo(FindTransportModel model)
        {
            model.Mode = "All";
            model.LocationType = "All";
            model.DistanceBounding = 1;
            var data = DataService.ExecuteStoredProcedure<FindTransportsResultModel>("sp_FindNearestTransportStop", model);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

    }
}
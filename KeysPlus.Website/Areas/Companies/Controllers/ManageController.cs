using KeysPlus.Data;
using KeysPlus.Service.Models;
using KeysPlus.Service.Services;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using KeysPlus.Website.Areas.Tools;
using Microsoft.Ajax.Utilities;
using PagedList;
using System.Web.Routing;

namespace KeysPlus.Website.Areas.Companies.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        // GET: Companies/Manage
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MyQuotes(QuotesSearchViewModel model)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            if (String.IsNullOrWhiteSpace(model.SortOrder))
            {
                model.SortOrder = "Latest Date";
            }
            if (String.IsNullOrWhiteSpace(model.Status))
            {
                model.Status = "opening";
            }
            var res = CompanyService.GetJobQuotes(model, login);
            model.PagedInput = new PagedInput
            {
                ActionName = "MyQuotes",
                ControllerName = "Manage",
                PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, SearchString = model.SearchString, Status = model.Status })
            };
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString });
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Lowest Amount", ActionName = "MyQuotes", RouteValues = rvr.AddRouteValue("SortOrder", "Lowest Amount") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Highest Amount", ActionName = "MyQuotes", RouteValues = rvr.AddRouteValue("SortOrder", "Highest Amount") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Date", ActionName = "MyQuotes", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Date") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Date", ActionName = "MyQuotes", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Date") });
            model.SortOrders = sortOrders;
            model.SearchCount = res.SearchCount;
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            model.PageCount = res.Items.PageCount;
            model.Items = res.Items;
            model.EditUrl = "/Companies/Manage/EditJobQuote";
            model.DeleteUrl = "/Companies/Manage/DeleteJobQuote";
            TempData["CurrentLink"] = "MyQuotes";
            return View(model);
        }
    
        [HttpPost]
        public JsonResult DeleteJobQuote(int id)
        {
            using (var db = new KeysEntities())
            {
                var delJob = db.JobQuote.FirstOrDefault(x => x.Id == id);
                var mediaFiles = delJob.JobQuoteMedia.Where(y => y.JobQuoteId == id);
                if (delJob != null && mediaFiles != null)
                {
                    delJob.Status = "delete";
                    db.JobQuoteMedia.RemoveRange(mediaFiles);
                    mediaFiles.ToList().ForEach( x => { MediaService.RemoveMediaFile(x.FileName); });
                    db.SaveChanges();
                    return Json(new { Success = true, Message = "JobQuote SuccessFully Deleted!" });
                }
                else
                {
                    return Json(new { Success = false, Message = "Delete UnSuccessFul " });
                }

            }
        }
        [HttpPost]
        public JsonResult EditJobQuote(JobQuoteModel quoteModel)
        {
            var userName = User.Identity.Name;
            var status = true;
            var message = "";
            if (String.IsNullOrEmpty(userName))
            {
                return Json(new { Success = false });
            }
            var files = Request.Files;
            var login = AccountService.GetLoginByEmail(userName);          
            var result = CompanyService.EditMarketJobQuote(quoteModel, login, files);          
            return Json(new
            {
                Success = status,
                Message = message,
                Data = quoteModel
            });
        }
    }
}

    

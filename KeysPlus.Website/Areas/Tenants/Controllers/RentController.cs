using KeysPlus.Service.Models;
using KeysPlus.Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace KeysPlus.Website.Areas.Tenants
{
    [Authorize]
    public class RentController : Controller
    {
        // GET: Tenants/Rent
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Inspections(TenantInspectionSearchModel model)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            if (String.IsNullOrWhiteSpace(model.SortOrder))
            {
                model.SortOrder = "Latest First";
            }
            var res = TenantService.GetAllInspections(model,login);
            
            model.PagedInput = new PagedInput
            {
                ActionName = "Inspections",
                ControllerName = "Rent",
                PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, SearchString = model.SearchString })
            };
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString });
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest First", ActionName = "Inspections", RouteValues = rvr.AddRouteValue("SortOrder", "Latest First") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest First", ActionName = "Inspections", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest First") });
            sortOrders.Add(new SortOrderModel { SortOrder = "High Progress", ActionName = "Inspections", RouteValues = rvr.AddRouteValue("SortOrder", "High Progress") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Low Progress", ActionName = "Inspections", RouteValues = rvr.AddRouteValue("SortOrder", "Low Progress") });
            model.SortOrders = sortOrders;
            model.SearchCount = res.SearchCount;
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            model.PageCount = res.Items.PageCount;
            model.Items = res.Items;
            model.EditUrl = "/Tenants/Home/EditInspection";
            return View(model);
        }
    }
}
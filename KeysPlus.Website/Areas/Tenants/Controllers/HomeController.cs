using KeysPlus.Service.Models;
using KeysPlus.Service.Services;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
//using KeysPlus.Website.Areas.Tenants.Models;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using KeysPlus.Data;
using System.Collections.Generic;
using KeysPlus.Website.Areas.Tools;
using Microsoft.Ajax.Utilities;
using System.Web.Routing;

namespace KeysPlus.Website.Areas.Tenants.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        KeysEntities db = new KeysEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Onboarding()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);

            var props = db.TenantProperty.Where(x => x.TenantId == login.Id && (x.IsActive ?? true));
            var propIds = props.Select(x => x.PropertyId);

            //ApplicationStatusId {1 : Applied, 2 : Accepted, 3: Declined}
            var rentApps = db.RentalApplication.Where(x => x.PersonId == login.Id && x.IsActive);
            var newApps = rentApps.Where(x => x.ApplicationStatusId == 1 && !(x.IsViewedByOwner ?? false)).Count();
            var acceptedApps = rentApps.Where(x => x.ApplicationStatusId == 2).Count();
            var pendingApps = rentApps.Where(x => x.ApplicationStatusId == 1 && (x.IsViewedByOwner ?? false)).Count();
            var declinedApps = rentApps.Where(x => x.ApplicationStatusId == 3).Count();

            var landlordreqs = db.PropertyRequest.Where(x => propIds.Contains(x.Property.Id) && x.IsActive && !x.ToOwner && x.ToTenant);
            var newLandlordReqs = landlordreqs.Where(x => !x.IsViewed).Count();
            var viewedLandLordReqs = landlordreqs.Where(x => x.IsViewed).Count();

            var tenRequests = db.PropertyRequest.Where(x => propIds.Contains(x.Property.Id) && x.IsActive && x.ToOwner && !x.ToTenant);//Total
            var newRequests = tenRequests.Where(x => x.RequestStatusId == 1).Count();
            var acceptedRequests = tenRequests.Where(x => x.RequestStatusId == 2).Count();
            var rejRequests = tenRequests.Where(x => x.RequestStatusId == 5).Count();


            var model = new TenantDashBoardModel
            {
                TenantRentalDashboardData = TenantService.GetRentalInfo(login.Id),
                RentAppsDashboardData = new TenantRentAppDashboardModel
                {
                    NewItems = newApps,
                    Accepted = acceptedApps,
                    //Pending = pendingApps,
                    Rejected = declinedApps,
                },
                LandLordRequestDashboardData = new LandLordRequestDashboardModel
                {
                },
                TenantRequestDashboardData = new TenantRequestDashboardModel
                {
                    Current = newRequests,
                    Accepted = acceptedRequests,
                    Rejected = rejRequests
                    //Pending = tenRequests.Count() - (newRequests + acceptedRequests),
                }

            };
            return View(model);
        }

        public ActionResult SendRequest(string returnUrl, int? type)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var rentalProperties = TenantService.GetRentalInfo(login.Id).ToList();
            //model.ForEach(x => x.AddressString = x.PropertyAddress.Number + " " + x.PropertyAddress.Street + ", " + x.PropertyAddress.Suburb + ", " + x.PropertyAddress.City + ", " + x.PropertyAddress.PostCode);
            ViewBag.ReturnUrl = returnUrl;
            //ViewBag.requestTypes = PropertyService.GetRequestTypes().Where(x => x.Id != 3).ToList(); // Tenant cannot request for inspection.
            var requestTypes = PropertyService.GetRequestTypes().Where(x => x.Id != 3).ToList(); // Tenant cannot request for inspection.
            if (type.HasValue)
            {
                requestTypes = requestTypes.Where(x => x.Id == type).ToList();
            }
            var model = new TenantSendRequestModel
            {
                RentalProperties = rentalProperties,
                RequestTypes = requestTypes,
                ReturnUrl = returnUrl,
            };
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        public ActionResult UpdatePhoto()
        {
            return Json("");
        }

        [HttpGet]
        public JsonResult PaymentDate(int id,int tid)
        {
            var d = TenantService.GetPaymentDate(id, tid);
            return Json(new { date=d }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Onboarding(TenantDetailsModel model, HttpPostedFileBase imageFile)
        {
            var fs = Request.Files;
            var isAjax = Request.IsAjaxRequest();
            //try
            //{
                if (ModelState.IsValid)
                {
                    var userName = User.Identity.Name;
                    if (string.IsNullOrEmpty(userName))
                    {
                        return Json(new { Success = false, ErrorMsg = "User does not exist!" });
                    }
                    var login = AccountService.GetLoginByEmail(userName);
                    var tenant = login == null ? null : TenantService.GetTenantByLogin(login);
                    var detailResult = TenantService.SaveDetails(model, login);
                    if (detailResult.IsSuccess)
                    {
                        var files = Request.Files;
                        var mediaResult = TenantService.AddTenantMediaFiles(Request.Files, tenant, Server.MapPath("~/images"));
                        if (isAjax)
                        {
                            return Json(new { Success = true });
                        }
                        else return RedirectToAction("Index");

                    }
                    else
                    {
                        return Json(new { Success = false });
                    }
                }
                return View(model);
            //}
            //catch (Exception ex)
            //{

            //    Console.WriteLine("excepion"+ex.InnerException);
            //    return View(model);

            //}
        }

        public ActionResult MyRentals(MyRentalsSearchModel model)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            if (TenantService.IsLoginATenant(login))
            {
                if (String.IsNullOrWhiteSpace(model.SortOrder))
                {
                    model.SortOrder = "Latest Date";
                }

                var data = TenantService.GetMyRentals(login.Id).AsQueryable();

                switch (model.SortOrder)
                {
                    case "Latest First":
                        data = data.OrderBy(s => s.CreatedOn);
                        break;
                    case "Earliest First":
                        data = data.OrderByDescending(s => s.CreatedOn);
                        break;
                    case "Due Date Nearest":
                        data = data.OrderBy(s => s.NextPaymenDate);
                        break;
                    case "Due Date Farthest":
                        data = data.OrderByDescending(s => s.NextPaymenDate);
                        break;
                    default:
                        data = data.OrderByDescending(s => s.CreatedOn);
                        break;
                }

                var allItems = data.ToPagedList(model.Page, 10);

                if (!String.IsNullOrWhiteSpace(model.SearchString))
                {
                    SearchTool searchTool = new SearchTool();
                    int searchType = searchTool.CheckDisplayType(model.SearchString);
                    string formatString = searchTool.ConvertString(model.SearchString);
                    switch (searchType)
                    {
                        case 1:
                            data = data.Where(x =>
                                (x.AddressString != null && x.AddressString.ToLower().EndsWith(formatString))
                               || (x.Model.Landlordname != null && x.Model.Landlordname.ToLower().EndsWith(formatString))
                               || (x.Model.LandlordPhone != null && x.Model.LandlordPhone.ToLower().EndsWith(formatString)));
                            break;
                        case 2:
                            data = data.Where(x =>
                                (x.AddressString != null && x.AddressString.ToLower().StartsWith(formatString))
                              || (x.Model.Landlordname != null && x.Model.Landlordname.ToLower().StartsWith(formatString))
                              || (x.Model.LandlordPhone != null && x.Model.LandlordPhone.ToLower().StartsWith(formatString)));
                            break;
                        case 3:
                            data = data.Where(x =>
                                (x.AddressString != null && x.AddressString.ToLower().Contains(formatString))
                               || (x.Model.Landlordname != null && x.Model.Landlordname.ToLower().Contains(formatString))
                               || (x.Model.LandlordPhone != null && x.Model.LandlordPhone.ToLower().Contains(formatString)));
                            break;
                    }
                }

                var items = data.ToPagedList(model.Page, 10);
                var count = items.Count;
                items.ForEach(x => x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties()));

                var result = new SearchResult { SearchCount = items.Count, Items = count == 0 ? allItems : items, NoResultFound = (count == 0) };
                model.PagedInput = new PagedInput
                {
                    ActionName = "MyRentals",
                    ControllerName = "Home",
                    PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, SearchString = model.SearchString })
                };

                var rvr = new RouteValueDictionary(new { SearchString = model.SearchString });
                var sortOrders = new List<SortOrderModel>();
                sortOrders.Add(new SortOrderModel { SortOrder = "Latest First", ActionName = "MyRentals", RouteValues = rvr.AddRouteValue("SortOrder", "Latest First") });
                sortOrders.Add(new SortOrderModel { SortOrder = "Earliest First", ActionName = "MyRentals", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest First") });
                sortOrders.Add(new SortOrderModel { SortOrder = "Due Date Nearest", ActionName = "MyRentals", RouteValues = rvr.AddRouteValue("SortOrder", "Due Date Nearest") });
                sortOrders.Add(new SortOrderModel { SortOrder = "Due Date Farthest", ActionName = "MyRentals", RouteValues = rvr.AddRouteValue("SortOrder", "Due Date Farthest") });
                model.SortOrders = sortOrders;

                model.SearchCount = result.SearchCount;
                model.NoResultFound = result.NoResultFound;
                if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
                model.PageCount = result.Items.PageCount;
                model.Items = result.Items;

                ViewBag.RequestTypes = RentalService.GetRequestTypes().Where(x => x.Id != 3).ToList();
                TempData["CurrentLink"] = "MyRentals";
                return View(model);
            }
            return View("Error");

        }

        public JsonResult IsProfileComplete()
        {
            var user = User.Identity.Name;
            var isComplete = TenantService.IsProfileComplete(user);
            return Json(new { Success = isComplete });
        }

        [HttpPost]
        public JsonResult EditTenancyApplication(RentalApplicationModel model)
        {

            if (ModelState.IsValid)
            {
                var files = Request.Files;
                var userName = User.Identity.Name;
                if (String.IsNullOrEmpty(userName))
                {
                    return Json(new { Success = false, ErrorMsg = "User not exixt!" });
                }
                var login = AccountService.GetLoginByEmail(userName);
                var result = RentalService.EditRentallApllication(model, login, Request.Files);
                return result.IsSuccess ? Json(new { Success = true , MediaFiles = result.NewObject as List<MediaModel>}) : Json(new { Success = false, ErrorMsg = result.ErrorMessage });
            }
            else return Json(new { Success = false });


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteTenancyApplication(int id)
        {
            var files = Request.Files;
            var userName = User.Identity.Name;
            var result = RentalService.DeleteRentallApllication(id);
            return result.IsSuccess ? Json(new { Success = true }) : Json(new { Success = false, ErrorMsg = result.ErrorMessage });
        }

        public JsonResult UpdateRequest(RequestModel model)
        {

            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);

            var files = Request.Files;
            var result = PropertyService.UpdateRequest(model, login, Request.Files);
           
            if (result.IsSuccess)
            {
                return Json(new { Success = result.IsSuccess });
            }
            
            return Json(new { Success = false, Message = result.ErrorMessage });
           
        }
        [HttpPost]
        public JsonResult DeleteRequest(int id)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var request = PropertyService.GetPropertyRequestById(id);
            if (request == null) return Json(new { Success = false, Message = "No record found!" });
            request.IsActive = false;
            var result = PropertyService.UpdateRequest(request, login, Request.Files, Server.MapPath("~/images"));
            if (result.IsSuccess)
            {
                return Json(new
                {
                    Success = true,
                    Message = "IsDeleted",
                    Updated = true,
                });
            }
            else
            {
                return Json(new { Success = false, Message = result.ErrorMessage });
            }
        }

        [HttpGet]
        public ActionResult LandLordRequests(LandlordRequestsSearchModel model)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            if (String.IsNullOrWhiteSpace(model.SortOrder))
            {
                model.SortOrder = "Latest First";
            }
            if (model.RequestStatus == null)
            {
                model.RequestStatus = PropertyRequestStatus.Submitted;
            }
            var res = TenantService.GetLandlordRequests(login, model);
            
            model.PagedInput = new PagedInput
            {
                ActionName = "LandlordRequests",
                ControllerName = "Home",
                PagedLinkValues = new RouteValueDictionary(new
                {
                    PropId = model.PropId,
                    SortOrder = model.SortOrder,
                    SearchString = model.SearchString,
                    ReturnUrl = model.ReturnUrl,
                    RequestStatus = model.RequestStatus
                }),

            };
            model.InputValues = new List<SearchInput>() {
                new SearchInput {Name = "PropertyId", Value = model.PropId.ToString()},
                new SearchInput { Name = "RequestStatus", Value = model.RequestStatus.ToString()}
            };
            
            var rvr = new RouteValueDictionary(new { PropId = model.PropId, SearchString = model.SearchString, ReturnUrl = model.ReturnUrl });
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest First" , ActionName = "LandLordRequests", RouteValues= rvr.AddRouteValue("SortOrder", "Latest First") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest First", ActionName = "LandLordRequests", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest First") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Request Status", ActionName = "LandLordRequests", RouteValues = rvr.AddRouteValue("SortOrder", "Request Status") });
            model.SortOrders = sortOrders;
            model.SearchCount = res.SearchCount;
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            model.PageCount = res.Items.PageCount;
            model.Items = res.Items;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AcceptLandlordRequest(int requestId)
        {

            var user = User.Identity.Name;
            var files = Request.Files;
            var login = AccountService.GetLoginByEmail(user);
            var result = TenantService.AcceptLandlordRequest(requestId);
            if (result.IsSuccess)
            {
                return Json(new { Success = true, Message = "Request accepted successfull!" });
            }
            else
            {
                return Json(new { Success = false, ErrorMsg = result.ErrorMessage });
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddInspection(InspectionModel model)
        {

            var user = User.Identity.Name;
            var files = Request.Files;
            var login = AccountService.GetLoginByEmail(user);
            if (ModelState.IsValid)
            {
                var result = TenantService.AddInspection(model, login, Request.Files);
                if (result.IsSuccess)
                {
                    return Json(new { Success = true, Message = "Sucessfully Replied to the Property Owner's Request", Posted = true });
                }
                else
                {
                    return Json(new { Success = false, ErrorMsg = result.ErrorMessage });
                }
            }
            return Json(new { Success = false, ErrorMsg = "Invalid fields" });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditInspection (InspectionModel model)
        {
            var user = User.Identity.Name;
            var files = Request.Files;
            var login = AccountService.GetLoginByEmail(user);
            if (ModelState.IsValid)
            {
                var result = TenantService.EditInspection(model, login, Request.Files);
                if (result.IsSuccess)
                {
                    var media = result.NewObject as List<MediaModel>;
                    return Json(new { Success = true, Message = "Sucessfully Replied to the Property Owner's Request", MediaFiles = result.NewObject as List<MediaModel>});
                }
                else
                {
                    return Json(new { Success = false, ErrorMsg = result.ErrorMessage });
                }
            }
            return Json(new { Success = false, ErrorMsg = "Invalid fields" });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeclineRequest(int id, string reason)
        {
            var user = User.Identity.Name;            
            var login = AccountService.GetLoginByEmail(user);
            if (ModelState.IsValid)
            {
                var result = TenantService.DeclineRequest(new RequestModel {Id = id, Reason = reason }, login);
                if (result.IsSuccess)
                {
                    return Json(new { Success = true, Message = "Request has been declined!" });
                }
                else
                {
                    return Json(new { Success = false, ErrorMsg = result.ErrorMessage });
                }
            }
            return Json(new { Success = false, ErrorMsg = "Invalid fields" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AcceptRequest(int id)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            if (ModelState.IsValid)
            {
                var result = TenantService.AcceptRequest(new RequestModel { Id = id}, login);
                if (result.IsSuccess)
                {
                    return Json(new { Success = true, Message = "Request has been accepted!" });
                }
                else
                {
                    return Json(new { Success = false, ErrorMsg = result.ErrorMessage });
                }
            }
            return Json(new { Success = false, ErrorMsg = "Invalid fields" });
        }


        [HttpGet]
        public JsonResult GetRequest(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new { Success = false, JsonRequestBehavior.AllowGet });
            }
            var propertyRequest = TenantService.GetRequestById(id.Value);
            if (propertyRequest == null)
            {
                return Json(new { Success = false, JsonRequestBehavior.AllowGet });
            }


            return Json(new { Success = true, data = propertyRequest }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult MyRequests(MyRequestSearchModel model)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            if (String.IsNullOrWhiteSpace(model.SortOrder))
            {
                model.SortOrder = "Latest Request";
            }
            if(model.RequestStatus == null)
            {
                model.RequestStatus = PropertyRequestStatus.Submitted;
            }
            var data = TenantService.GetAllRequests(model, login);
            model.PagedInput = new PagedInput
            {
                ActionName = "MyRequests",
                ControllerName = "Home",
                PagedLinkValues = new RouteValueDictionary(new {
                    SortOrder = model.SortOrder,
                    SearchString = model.SearchString,
                    PropertyId = model.PropertyId,
                    RequestStatus = model.RequestStatus
                })
            };
            model.InputValues = new List<SearchInput>() {
                new SearchInput {Name = "PropertyId", Value = model.PropertyId.ToString()},
                new SearchInput { Name = "RequestStatus", Value = model.RequestStatus.ToString()}
            };
            
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString , PropertyId = model.PropertyId, RequestStatus = model.RequestStatus});
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Request", ActionName = "MyRequests", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Request") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Request", ActionName = "MyRequests", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Request") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Requested Type", ActionName = "MyRequests", RouteValues = rvr.AddRouteValue("SortOrder", "Requested Type") });
            model.SortOrders = sortOrders;
            model.SearchCount = data.SearchCount;
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            model.PageCount = data.Items.PageCount;
            model.Items = data.Items;
            model.EditUrl = "/Tenants/Home/UpdateRequest";
            model.DeleteUrl = "/Tenants/Home/DeleteRequest";
            TempData["CurrentLink"] = "MyRentApps";
            return View(model);
        }

        public ActionResult MyRentalApplications(RentalAppSearchModel model)
        {
            var user = User.Identity.Name;
            var login  = AccountService.GetLoginByEmail(user);
            if (String.IsNullOrWhiteSpace(model.SortOrder))
            {
                model.SortOrder = "Latest Date";
            }
            if (model.RentalStatus == null)
            {
                model.RentalStatus = Service.Models.RentalApplicationStatus.Applied;
            }
            var res = TenantService.GetAllRentApplications(model, login);

            model.PagedInput = new PagedInput
            {
                ActionName = "MyRentalApplications",
                ControllerName = "Home",
                PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, SearchString = model.SearchString, RentalStatus = model.RentalStatus })
            };
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString, RentalStatus = model.RentalStatus });
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Lowest Rent", ActionName = "MyRentalApplications", RouteValues = rvr.AddRouteValue("SortOrder", "Lowest Rent") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Highest Rent", ActionName = "MyRentalApplications", RouteValues = rvr.AddRouteValue("SortOrder", "Highest Rent") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Date", ActionName = "MyRentalApplications", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Date") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Date", ActionName = "MyRentalApplications", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Date") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Available", ActionName = "MyRentalApplications", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Available") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Available", ActionName = "MyRentalApplications", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Available") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Accepted First", ActionName = "MyRentalApplications", RouteValues = rvr.AddRouteValue("SortOrder", "Accepted First") });
            model.SortOrders = sortOrders;
            model.SearchCount = res.SearchCount;
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            model.PageCount = res.Items.PageCount;
            model.Items = res.Items;
            model.EditUrl = "/Tenants/Home/EditTenancyApplication";
            model.DeleteUrl = "/Tenants/Home/DeleteTenancyApplication";
            TempData["CurrentLink"] = "MyRentApps";
            return View(model);
        }

        public JsonResult GetRequestTypes()
        {
            var result = PropertyService.GetRequestTypes();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
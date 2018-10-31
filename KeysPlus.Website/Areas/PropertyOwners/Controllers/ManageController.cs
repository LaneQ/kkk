using KeysPlus.Data;
using KeysPlus.Service.Models;
using KeysPlus.Service.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using KeysPlus.Website.Areas.Tools;
using System.Web.Routing;
using Microsoft.Ajax.Utilities;

namespace KeysPlus.Website.Areas.PropertyOwners.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private KeysEntities db = new KeysEntities();
        [HttpPost]
        public async Task<ActionResult> SendInvitationEmailToTenant(AddTenantToPropertyModel model)
        {
            var user = User.Identity.Name;
            if (String.IsNullOrEmpty(user))
            {
                return Json(new { Success = false, ErrorMsg = "Invalid user!" });
            }
            var owner = AccountService.GetLoginByEmail(user);
            if (owner == null)
            {
                return Json(new { Success = false, ErrorMsg = "Can not find current user!" });
            }
            var ownerPerson = AccountService.GetPersonByLoginId(owner.Id);
            var property = PropertyService.GetPropertyById(model.PropertyId);
            if (property == null)
            {
                return Json(new { Success = false, ErrorMsg = "Can not find property!" });
            }
            var nvc = new NameValueCollection();
            nvc.Set("TenantEmail", model.TenantEmail);
            nvc.Set("PropertyId", model.PropertyId.ToString());
            nvc.Set("StartDate", model.StartDate.ToString());
            nvc.Set("EndDate", model.EndDate.ToString());
            nvc.Set("PaymentFrequencyId", model.PaymentFrequencyId.ToString());
            nvc.Set("PaymentAmount", model.PaymentAmount.ToString());
            string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "Account/RegisterToBeTenant", UtilService.ToQueryString(nvc));
            string subject = "Property Community: Invitation to register";
            string body =
               "Hello !<br />"
               + $"{ownerPerson.FirstName} has added you to be a tenant in his/her property and invited you to register at Property Community.<br />"
               + "Please <a target='_blank' href=" + url + "> Click Here </a> to register<br />";
            MailMessage msg = new MailMessage()
            {
                Subject = subject,
                SubjectEncoding = System.Text.Encoding.UTF8,
                Body = body,
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true
            };
            msg.To.Add(model.TenantEmail);
            try
            {
                await EmailService.SendAsync(msg);
                return Json(new { Success = true, Status = "Await response" });
            }
            catch (Exception ex)
            {
                return Json(new { Sucess = false, msg = ex.ToString() });
            }
        }

        public async Task<ActionResult> SendCreateAccountEmailToTenant(AddTenantToPropertyModel model, LoginViewModel loginModel)   
        {
            var user = User.Identity.Name;
            if (String.IsNullOrEmpty(user))
            {
                return Json(new { Success = false, ErrorMsg = "Invalid user!" });
            }
            var owner = AccountService.GetLoginByEmail(user);
            if (owner == null)
            {
                return Json(new { Success = false, ErrorMsg = "Can not find current user!" });
            }
            var ownerPerson = AccountService.GetPersonByLoginId(owner.Id);
            var property = PropertyService.GetPropertyById(model.PropertyId);
            if (property == null)
            {
                return Json(new { Success = false, ErrorMsg = "Can not find property!" });
            }
            var nvc = new NameValueCollection();
            string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "Account/Login");
            string subject = "Property Community: Invitation to register";
            string body =
               "Hello !<br />"
               + $"{ownerPerson.FirstName} has added you to be a tenant in his/her property at Property Community.<br />"
               + "Your account details are as follow :<br />"
               + $"User name : {loginModel.UserName}<br />"
               +$"Password : {loginModel.Password}"
               + "Please <a target='_blank' href=" + url + "> Click Here </a> sign in.<br />";
            MailMessage msg = new MailMessage()
            {
                Subject = subject,
                SubjectEncoding = System.Text.Encoding.UTF8,
                Body = body,
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true
            };
            msg.To.Add(model.TenantEmail);
            try
            {
                await EmailService.SendAsync(msg);
                return Json(new { Success = true, Status = "Await response" });
            }
            catch (Exception ex)
            {
                return Json(new { Sucess = false, msg = ex.ToString() });
            }
        }
        public ActionResult EditPropFinance(string returnUrl, int? propId)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var properties = PropertyService.GetPropertiesAndAddress(login.Id, propId).ToList() ;
            properties.ForEach(x => x.AddressString = x.Address.ToAddressString());
            var model = new PropDataModel
            {
                ReturnUrl = returnUrl,
                Properties = properties
            };
            return View(model);
        }

        [HttpGet]
        public ActionResult Inspections(POInspectionsSearchModel model)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            model.EditUrl = "/PropertyOwners/Manage/EvaluateInspection";
            model.DeleteUrl = "/PropertyOwners/Manage/DeleteInspection";
            if (String.IsNullOrWhiteSpace(model.SortOrder))
            {
                model.SortOrder = "Latest Date";
            }
            var props = db.OwnerProperty.Where(x => x.OwnerId == login.Id && x.Property.IsActive);
            var propsId = props.Select(x => x.Property.Id);
            var data = db.Inspection.Where(x => propsId.Contains(x.PropertyId) && x.IsActive == true).Select(x => new
            {
                Model = new InspectionModel
                {
                    Id = x.Id,
                    PropertyId = x.PropertyId,
                    StatusId = x.StatusId,
                    Message = x.Message,
                    Reason = x.Reason,
                    PercentDone = x.PercentDone,
                    RequestId = x.RequestId,
                    IsViewed = x.IsViewed ?? false,
                    IsUpdated = x.IsUpdated,
                    OwnerUpdate = x.OwnerUpdate,
                    MediaFiles = x.InspectionMedia.Select(y => new MediaModel
                    {
                        Id = y.Id,
                        NewFileName = y.NewFileName,
                        OldFileName = y.OldFileName,
                    }).ToList(),
                   LandlordMedia = x.PropertyRequest.PropertyRequestMedia.Select(k => new MediaModel1
                    {
                        Id = k.Id,
                        NewFileName = k.NewFileName,
                        OldFileName = k.OldFileName,
                        Status = "load",
                    }).ToList()
                },
                Address = new AddressViewModel
                {
                    AddressId = x.Property.Address.AddressId,
                    Number = x.Property.Address.Number.Replace(" ", ""),
                    Street = x.Property.Address.Street.Trim(),
                    City = x.Property.Address.City.Trim(),
                    Suburb = x.Property.Address.Suburb.Trim() ?? "",
                    PostCode = x.Property.Address.PostCode.Replace(" ", "")
                },
                PropertyAddress = x.Property.Address.Number.Replace(" ", "") + " " + x.Property.Address.Street.Trim() + " " + x.Property.Address.City.Trim() + " " + x.Property.Address.Suburb.Trim() + " " + x.Property.Address.PostCode.Replace(" ", ""),
                Status = x.InspectionStatus.Name,
                CreatedOn = x.CreatedOn,
                DueDate = x.PropertyRequest.DueDate,
            });
            var allItems = data.OrderByDescending(x => x.CreatedOn).ToPagedList(model.Page, 10);
            switch (model.SortOrder)
            {
                case "Highest Progress":
                    data = data.OrderByDescending(s => s.Model.PercentDone);
                    break;
                case "Lowest Progress":
                    data = data.OrderBy(s => s.Model.PercentDone);
                    break;
                case "Earliest Date":
                    data = data.OrderBy(s => s.CreatedOn);
                    break;
                case "Latest Date":
                    data = data.OrderByDescending(s => s.CreatedOn);
                    break;
                default:
                    data = data.OrderByDescending(s => s.CreatedOn);
                    break;
            }
            if (!String.IsNullOrWhiteSpace(model.SearchString))
            {
                SearchTool searchTool = new SearchTool();
                int searchType = searchTool.CheckDisplayType(model.SearchString);
                string formatString = searchTool.ConvertString(model.SearchString);
                switch (searchType)
                {
                    case 1:
                        data = data.Where(x => x.Model.Message.ToLower().EndsWith(formatString)
                                            || x.PropertyAddress.ToLower().EndsWith(formatString));
                        break;
                    case 2:
                        data = data.Where(x => x.Model.Message.ToLower().StartsWith(formatString)
                                            || x.PropertyAddress.ToLower().StartsWith(formatString));
                        break;
                    case 3:
                        data = data.Where(x => x.Model.Message.ToLower().Contains(formatString)
                                            || x.PropertyAddress.ToLower().Contains(formatString));
                        break;
                }
            }
            var items = data.ToPagedList(model.Page, 10);
            var count = items.Count;
            items = count == 0 ? allItems : items;
            items.ToList().ForEach(x => x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties()));
            items.ToList().ForEach(x => x.Model.LandlordMedia.ForEach(y => y.InjectMediaModelViewProperties1()));

                var result = new SearchResult { SearchCount = items.Count, Items = count == 0 ? allItems : items };
            model.PagedInput = new PagedInput
            {
                ActionName = "Inspections",
                ControllerName = "Manage",
                PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, SearchString = model.SearchString })
            };
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString });
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Highest Progress", ActionName = "Inspections", RouteValues = rvr.AddRouteValue("SortOrder", "Highest Progress") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Lowest Progress", ActionName = "Inspections", RouteValues = rvr.AddRouteValue("SortOrder", "Lowest Progress") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Date", ActionName = "Inspections", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Date") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Date", ActionName = "Inspections", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Date") });
            model.SortOrders = sortOrders;
            model.SearchCount = result.SearchCount;
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            model.PageCount = result.Items.PageCount;
            model.Items = result.Items;
            
            return View(model);
        }

        [HttpPost]
        public JsonResult EvaluateInspection(InspectionModel model)
        {
            var request = db.Inspection.FirstOrDefault(x => x.Id == model.Id);
            if (request == null) return Json(new { Success = false, Message = "No record found!" });
           
            if (ModelState.IsValid)
            {
                request.StatusId = model.StatusId;
                request.Reason = model.Reason;
                request.OwnerUpdate = true;

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Json(new { Success = false, ErrorMsg = ex.ToString() });
                }
            }
            else
            {
                return Json(model);
            }

            return Json(new { Success = true, Message = "Evaluation Submitted Successfully" });
        }

        [HttpPost]
        public JsonResult DeleteInspection(int id)
        {
            var request = db.Inspection.FirstOrDefault(x => x.Id == id);
            if (request == null) return Json(new { Success = false, Message = "No record found!" });

            if (ModelState.IsValid)
            {
                try
                {
                    request.IsActive = false;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Json(new { Success = false, ErrorMsg = ex.ToString() });
                }
            }
            else
            {
                return Json(new { Success = false, ErrorMsg = "Invalid Fields." });
            }

            return Json(new { Success = true, Message = "Inspection Deleted Successfully" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InspectionViewed(int inspectionId)
        {
            var user = User.Identity.Name;
            if (String.IsNullOrEmpty(user))
            {
                return Json(new { Success = false, Msg = "User not found!" });
            }
            var result = PropertyService.UpdateInspectionRequest(inspectionId);

            return Json(new { Success = result.IsSuccess, Msg = result.ErrorMessage });
        }

        public ActionResult AcceptedJobs(POJobSearchModel model)
        {
            var login = AccountService.GetLoginByEmail(User.Identity.Name);
            if (String.IsNullOrWhiteSpace(model.SortOrder))
            {
                model.SortOrder = "Latest Date";
            }
            var data = db.Job.Where(x => x.OwnerId == login.Id && x.JobStatusId != 5 && x.JobStatusId !=6 && x.JobStatusId != 1)
                .Select(x => new
                {
                    Model = new JobModel
                    {
                        Id = x.Id,
                        PropertyId = x.PropertyId,
                        ProviderId = x.ProviderId,
                        JobStartDate = x.JobStartDate,
                        JobEndDate = x.JobEndDate,
                        JobStatusId = x.JobStatusId,
                        JobRequestId = x.JobRequestId,
                        PercentDone = x.PercentDone??0,
                        Note = x.Note,
                        JobDescription = x.JobDescription,
                        AcceptedQuote = x.AcceptedQuote,
                        OwnerUpdate = x.OwnerUpdate,
                        ServiceUpdate = x.ServiceUpdate,
                        MediaFiles = x.JobMedia.Select(y => new MediaModel { Id = y.Id, NewFileName = y.NewFileName, OldFileName = y.OldFileName }).ToList(),
                    },
                    ProviderCompanyName = x.ServiceProvider != null ? x.ServiceProvider.Company.Name : "Myself",
                    JobStatus = x.ServiceProviderJobStatus.Name,
                    CreatedOn = x.CreatedOn,
                    Address = new AddressViewModel
                    {
                        AddressId = x.Property.Address.AddressId,
                        CountryId = x.Property.Address.CountryId,
                        Number = x.Property.Address.Number.Replace(" ", ""),
                        Street = x.Property.Address.Street.Trim(),
                        City = x.Property.Address.City.Trim(),
                        Suburb = x.Property.Address.Suburb.Trim() ?? "",
                        PostCode = x.Property.Address.PostCode.Replace(" ", ""),
                    },
                    PropertyAddress = (x.Property.Address.Number.Replace(" ", "")) + " " +
                                          (x.Property.Address.Street.Trim() ?? "") + " " +
                                          (x.Property.Address.Suburb.Trim() ?? "") + " " +
                                          (x.Property.Address.City.Trim() ?? "") + "-" +
                                          (x.Property.Address.PostCode.Replace(" ", "")),
                    IsDIY = x.ProviderId == null,
                });
            var allItems = data.OrderBy(x => x.Model.PercentDone).ToPagedList(model.Page, 2);
            allItems.ToList().ForEach(x => x.Model.MediaFiles.ToList().ForEach(y => y.InjectMediaModelViewProperties()));
            switch (model.SortOrder)
            {
                case "Low Progress":
                    data = data.OrderBy(x => x.Model.PercentDone);
                    break;
                case "High Progress":
                    data = data.OrderByDescending(x => x.Model.PercentDone);
                    break;
                case "Low Budget":
                    data = data.OrderBy(x => x.Model.AcceptedQuote);
                    break;
                case "High Budget":
                    data = data.OrderByDescending(x => x.Model.AcceptedQuote);
                    break;
                case "Earliest Date":
                    data = data.OrderBy(x => x.CreatedOn);
                    break;
                case "Latest Date":
                    data = data.OrderByDescending(s => s.CreatedOn);
                    break;
                default:
                    data = data.OrderBy(x => x.CreatedOn);
                    break;
            }
            if (!String.IsNullOrWhiteSpace(model.SearchString))
            {
                SearchUtil searchTool = new SearchUtil();
                int searchType = searchTool.CheckDisplayType(model.SearchString);
                string formatString = searchTool.ConvertString(model.SearchString);
                data = data.Where(x => x.PropertyAddress.ToLower().Contains(formatString)
                                       || x.ProviderCompanyName.ToLower().Contains(formatString)
                                       || x.Model.AcceptedQuote.ToString().ToLower().Contains(formatString)
                                       || x.Model.JobDescription.ToLower().Contains(formatString)
                                       || x.JobStatus.ToLower().Contains(formatString)
                                       );
            };
            var items = data.ToPagedList(model.Page, 10);
            items.ToList().ForEach(x => x.Model.MediaFiles.ToList().ForEach(y => y.InjectMediaModelViewProperties()));
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            var sortOrders = new List<SortOrderModel>();
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString });
            sortOrders.Add(new SortOrderModel { SortOrder = "Low Progress", ActionName = "AcceptedJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Low Progress") });
            sortOrders.Add(new SortOrderModel { SortOrder = "High Progress", ActionName = "AcceptedJobs", RouteValues = rvr.AddRouteValue("SortOrder", "High Progress") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Low Budget", ActionName = "AcceptedJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Low Budget") });
            sortOrders.Add(new SortOrderModel { SortOrder = "High Budget", ActionName = "AcceptedJobs", RouteValues = rvr.AddRouteValue("SortOrder", "High Budget") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Date", ActionName = "AcceptedJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Date") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Date", ActionName = "AcceptedJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Date") });
            //sortOrders.Add((SortOrderModel)Activator.CreateInstance(typeof(SortOrderModel), "Latest Date", "AcceptedJobs", rvr.AddRouteValue("SortOrder", "Latest Date")));
            model.SortOrders = sortOrders;
            model.PagedInput = new PagedInput
            {
                ActionName = "AcceptedJobs",
                ControllerName = "Manage",
                PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, SearchString = model.SearchString })
            };
            model.PageCount = items.Count == 0 ? allItems.PageCount: items.PageCount;
            model.SearchCount = items.Count;
            model.Items = items.Count == 0 ? allItems : items;
            model.EditUrl = "/Jobs/Home/UpdateJobStatus";
            model.DeleteUrl = "/Jobs/Home/DeleteJob";
            return View(model);
        }

        public ActionResult MyMarketJobs(MarketJobSearchModel model)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            model.IsOwnerView = true;
            if (model.SortOrder == null)
            {
                model.SortOrder = "Latest Listing";
            }
            var result = JobService.GetAllMarketJobs(model, login);
            model.PagedInput = new PagedInput
            {
                ActionName = "MyMarketJobs",
                ControllerName = "Manage",
                PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, SearchString = model.SearchString })
            };
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString });
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Highest New Quotes", ActionName = "MyMarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Highest New Quotes") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Lowest New Quotes", ActionName = "MyMarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Lowest New Quotes") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Title", ActionName = "MyMarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Title") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Title(Desc)", ActionName = "MyMarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Title(Desc)") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Lowest Budget", ActionName = "MyMarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Lowest Budget") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Highest Budget", ActionName = "MyMarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Highest Budget") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Listing", ActionName = "MyMarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Listing") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Listing", ActionName = "MyMarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Listing") });
            model.SortOrders = sortOrders;
            model.SearchCount = result.SearchCount;
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            model.PageCount = result.Items.PageCount;
            model.Items = result.Items;
            model.EditUrl = "/Jobs/Home/EditJobFromMarket";
            model.DeleteUrl = "/Jobs/Home/RemoveJobFromMarket";
            TempData["CurrentLink"] = "MarketJobsOwner";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AcceptTenantRequest(int requestId)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var res = await PropertyOwnerService.AcceptTenantRequest(requestId, login);
            return Json(new { Success = res.IsSuccess, Msg = res.ErrorMessage});
        }

        public ActionResult RentalPaymentTracking()
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var paymentTrackings = PropertyOwnerService.GetRenalPaymentTracking(login);
            return View(paymentTrackings);
        }
        protected override void Dispose(bool disposing)
        {
            db?.Dispose();
            base.Dispose(disposing);
        }
    }
}
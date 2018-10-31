using KeysPlus.Data;
using KeysPlus.Service.Models;
using KeysPlus.Website.Areas.Tools;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using KeysPlus.Service.Services;
using System.Collections.Specialized;
using System.Web.Routing;

namespace KeysPlus.Website.Areas.Jobs.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public enum JobStatus
        {
            Open = 1,
            Pending = 2,
            In_Process = 3,
            Finished = 4,
            Cancelled = 5,
            Deleted = 6
        }
        private KeysEntities db = new KeysEntities();
        private List<string> imgExtensions = new List<string> { ".jpg", ".png", ".gif", ".jpeg" };
        private List<string> docExtensions = new List<string> { "*.pdf", "*.doc " };
        
        [HttpGet]
        public JsonResult GetMarketJob(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new { Success = false, JsonRequestBehavior.AllowGet });
            }
            var jobRequest = JobService.GetMarketJobById(id.Value);
            if (jobRequest == null)
            {
                return Json(new { Success = false, JsonRequestBehavior.AllowGet});
            }

            jobRequest.MediaFiles.ForEach( x => x.InjectMediaModelViewProperties());

            return Json(new { Success= true, data = jobRequest }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult MarketJobs(MarketJobSearchModel model)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            model.IsOwnerView = false;
            if(model.SortOrder == null)
            {
                model.SortOrder = "Latest Listing";
            }
            var result= JobService.GetAllMarketJobs(model, login);
            model.PagedInput = new PagedInput
            {
                ActionName = "MarketJobs",
                ControllerName = "Home",
                PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, SearchString = model.SearchString })
            };
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString });
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Title", ActionName = "MarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Title") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Title(Desc)", ActionName = "MarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Title(Desc)") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Lowest Budget", ActionName = "MarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Lowest Budget") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Highest Budget", ActionName = "MarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Highest Budget") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Listing", ActionName = "MarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Listing") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Listing", ActionName = "MarketJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Listing") });
            model.IsUserServiceSupply = CompanyService.IsUserServiceSupplier(user);
            model.IsProfileComplete = CompanyService.IsProfileComplete(login);
            model.SortOrders = sortOrders;
            model.SearchCount = result.SearchCount;
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            model.PageCount = result.Items.PageCount;
            model.Items = result.Items;
            TempData["CurrentLink"] = "MarketJobs";
            return View(model);
        }

        public JsonResult GetPropertyByUserId()
        {
            // This should be filtered by user id

            var properties = db.OwnerProperty.Where(x => x.Person.Login.UserName == User.Identity.Name && x.Property.IsActive).
                Select(p => new PropertyViewModel
                {
                    Id = p.Id,
                    Name = p.Property.Name
                }).AsEnumerable();

            return Json(properties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveJob(JobModel jobViewModel)
        {
            var user = User.Identity.Name;
            var files = Request.Files;
            var login = AccountService.GetLoginByEmail(user);
            if (ModelState.IsValid)
            {
                var result = JobService.AddDIYJob(jobViewModel, login, Request.Files);
                if (result.IsSuccess)
                {
                    return Json(new { Success = true, Message = "Job Created ", Posted = true });
                }
                else
                {
                    return Json(new { Success = false, ErrorMsg = result.ErrorMessage });
                }
            }
            else
            {
                return Json(new { Success = false, ErrorMsg = "Invalid fields" });
            }
        }


        [HttpPost]
        public JsonResult DeleteTenantJobRequest(int jobRequestId)
        {
            var job = db.TenantJobRequest.Find(jobRequestId);
            if (job == null && job.JobStatusId == 1)
            {
                job.JobStatusId = 5;
                try
                {
                    db.SaveChanges();
                    return Json(new { success = true, Message = "SuccessFully Deleted!" });
                }
                catch (Exception)
                {
                    return Json(new { success = false });
                }

            }
            else
            {
                return Json(new { success = false, Message = "Record not Deleted!" });
            }
        }

        [HttpPost]
        public JsonResult RemoveJobFromMarket(int marketJobId)
        {
            var result = JobService.RemoveMarketJob(marketJobId);
            return result.IsSuccess ? Json(new { Success = true }) : Json(new { Success = false, ErrorMsg = result.ErrorMessage });
        }


        [HttpPost]
        public JsonResult RemoveJobFromMarketDIY(JobModel job)
        {
            var user = User.Identity.Name;

            var login = AccountService.GetLoginByEmail(user);
            var files = Request.Files;
            if (job.JobRequestId.HasValue)
            {
                var removeResult = JobService.RemoveMarketJob(job.JobRequestId.Value);
            }
            var result = JobService.AddJob(job, files, login);
            return result.IsSuccess ? Json(new { Success = true }) : Json(new { Success = false, ErrorMsg = result.ErrorMessage });
        }

        [HttpPost]
        public JsonResult EditJobFromMarket(MarketJobModel model)
        {
            if (ModelState.IsValid)
            {
                var files = Request.Files;//.MediaFiles;// Request.Files;
                var userName = User.Identity.Name;
                if (String.IsNullOrEmpty(userName))
                {
                    return Json(new { Success = false, ErrorMsg = "User not exixt!" });
                }
                var login = AccountService.GetLoginByEmail(userName);
                var result = JobService.EditMarketJob(model, login, Request.Files);
                return result.IsSuccess ? Json(new { Success = true }) : Json(new { Success = false, ErrorMsg = result.ErrorMessage });
            }
            else return Json(new { Success = false });

        }

       
        public ActionResult AddNewJob(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var user = User.Identity.Name;
            var id = AccountService.GetLoginByEmail(user).Id;
            var allProperties = new List<PropertyViewModel>();
            var properties = PropertyService.GetAllPropertiesByOwner(id);
            foreach (var property in properties)
            {
                var address = PropertyService.GetAddressById(property.AddressId);
                if (address != null)
                {
                    if (address.Street != "" && address.Suburb != "" && address.City != "")
                    {
                        var propertyVm = new PropertyViewModel
                        {
                            Id = property.Id,
                            Name = property.Name,
                            AddressString = address.Number + " " + address.Street + ", " + address.Suburb + ", " + address.City + ", " + address.PostCode
                        };
                        allProperties.Add(propertyVm);
                    }
                }
            }
            ViewBag.Properties = allProperties;
            return View();
        }

        [HttpPost]
        public ActionResult AddJobToMarket(JobMarketModel marketJob)
        {
            var data = marketJob;
            var files = Request.Files;

            var user = User.Identity.Name;

            var login = AccountService.GetLoginByEmail(user);
            var result = JobService.AddPropertyOwnerMarketJob(marketJob, login, Request.Files, Server.MapPath("~/images"));
            if (result.IsSuccess)
            {
                return Json(new { Success = true, Msg = "Added!", result = "Redirect", url = Url.Action("Index", "PropertyOwners") });

            }
            else
            {
                return Json(new { Success = false, Msg = result.ErrorMessage, redirect = "Redirect", url = Url.Action("Index", "PropertyOwners") });
            }
        }


        [HttpPost]
        public JsonResult StartJob(int jobId)
        {
            var job = db.Job.Find(jobId);
            if (job != null)
            {
                job.JobStatusId = 3;
                job.JobStartDate = DateTime.Now;
                job.UpdatedBy = User.Identity.Name;
                job.UpdatedOn = DateTime.Now;
                try
                {
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception)
                {

                    return Json(new { success = false });
                }
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult FinishJob(int jobId)
        {
            var job = db.Job.Find(jobId);
            if (job != null)
            {
                job.JobStatusId = 4;
                job.JobEndDate = DateTime.Now;
                job.UpdatedBy = User.Identity.Name;
                job.UpdatedOn = DateTime.Now;
                try
                {
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception)
                {

                    return Json(new { success = false });
                }
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult DeleteJob (int id)
        {
            using (var db = new KeysEntities())
            {
                var delJob = db.Job.FirstOrDefault(x => x.Id == id);
                if (delJob != null)
                {
                    delJob.JobStatusId = 1;
                    db.SaveChanges();
                    return Json(new { Success = true, Message = "Job Delete Successfully!" });
                }
                else
                {
                    return Json(new { Success = false, Message = "Job Delete UnSuccessfully!" });

                }
            }
           
        }

        [HttpPost]
        public JsonResult UpdateJobStatus(JobModel model)
        {
            var files = Request.Files;
            var job = db.Job.Find(model.Id);
            bool hasNewFile = false;
            if (job != null)
            {

                if (model.PercentDone == 100)
                {
                    job.JobStatusId = 4; // finished
                }
                // if service supplier edits percentDone from 100% back to less than 100% status is changed from finshed back to in process.
                if (0 <= model.PercentDone && model.PercentDone < 100) 
                {
                    job.JobStatusId = 3; // in process
                }
                job.PercentDone = model.PercentDone;
                job.JobDescription = model.JobDescription;
                job.Note = model.Note;
                job.JobEndDate = DateTime.Now;
                job.UpdatedBy = User.Identity.Name;
                job.UpdatedOn = DateTime.Now;
                if (model.FilesRemoved.Count()> 0) { 
                    model.FilesRemoved.ForEach(x => {
                        var j = db.JobMedia.Find(x);
                        db.JobMedia.Remove(j);
                        MediaService.RemoveMediaFile(j.NewFileName);
                    });
                }
                var mf = MediaService.SaveMediaFiles(files, 5);
                if (mf.IsSuccess)
                {
                    var mediaFiles = mf.NewObject as List<MediaModel>;
                    if (mediaFiles != null)
                    {
                        hasNewFile = true;
                        mediaFiles.ForEach(x => job.JobMedia.Add(new JobMedia
                        {
                            NewFileName = x.NewFileName,
                            OldFileName = x.OldFileName,
                            IsActive = true,
                            PropertyId = job.PropertyId,
                        }));
                    }
                }

                try
                {
                    db.SaveChanges();
                    var mFiles = new List<MediaModel>();
                    if (hasNewFile)
                    {
                        mFiles = job.JobMedia.Select(x => new MediaModel
                        {
                            Id = x.Id,
                            NewFileName = x.NewFileName,
                            OldFileName = x.OldFileName,
                            Data = Url.Content("~/documents/" + x.NewFileName),
                            MediaType = MediaService.GetMediaType(x.NewFileName),
                            Size = MediaService.GetFileSize(x.NewFileName)
                        }).ToList();
                    }
                    
                    return Json(new { success = true , MediaFiles = mFiles });
                }
                catch (Exception ex)
                {

                    return Json(new { success = false });
                }
            }
            return Json(new { success = false });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<JsonResult> SaveJobQuote(QuoteModel jobQuoteViewModel)
        {
            if (ModelState.IsValid)
            {
                var files = Request.Files;
                var user = User.Identity.Name;
                var login = AccountService.GetLoginByEmail(user);
                var result = JobService.AddJobQuote(jobQuoteViewModel, login, Request.Files);
                #region


                if (result.IsSuccess)
                {
                    var propertyOwner = JobService.GetOwnerDetails(jobQuoteViewModel.JobRequestId);
                    var propertyOwnerLogin = AccountService.GetLoginById(propertyOwner.Id);

                    var property = db.TenantJobRequest.Where(x => x.Id == jobQuoteViewModel.JobRequestId).Select(x => x.Property).FirstOrDefault();
                    var nvc = new NameValueCollection();
                    nvc.Add("marketJobId", jobQuoteViewModel.JobRequestId.ToString());
                    string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "/Jobs/Home/GetJobQuotes", UtilService.ToQueryString(nvc));
                    SendGridEmailModel mail = new SendGridEmailModel
                    {
                        RecipentName = propertyOwner.FirstName,
                        ButtonText = "",
                        ButtonUrl = url,
                        Address = property.Address.ToAddressString(),
                        RecipentEmail = propertyOwnerLogin.Email,
                       JobTitle = jobQuoteViewModel.Title ??"No Title",
                    };
                    await EmailService.SendEmailWithSendGrid(EmailType.NewQuoteEmail, mail);
                }

                #endregion
                return Json(new { Success = result.IsSuccess, ErrorMsg = result.ErrorMessage ?? "" });
            }
            return Json(new { success = false });
        }
        
        public ActionResult NewQuotesCount()
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var quoteCount = JobService.GetNewQuotesCount(login);
            return PartialView(quoteCount);
        }

        public ActionResult GetJobQuotes(QuotesSearchViewModel model)
        {
            if (model.SortOrder == null)
            {
                model.SortOrder = "Latest Listing";
            }
            var data = db.JobQuote.Where(x => x.Status.ToLower() == "opening")
                .Select(x => new {
                    Model = new JobQuoteModel
                    {
                        Id = x.Id,
                        JobRequestId = x.JobRequestId,
                        Amount = x.Amount,
                        Status = x.Status,
                        Note = x.Note,
                        IsViewed = x.IsViewed,
                        MediaFiles = x.JobQuoteMedia.Select(y => new MediaModel
                        {
                            Id = y.Id,
                            OldFileName = y.FileName.Substring(36),
                            NewFileName = y.FileName,
                        }).ToList(),
                    },
                    CreatedOn = x.CreatedOn,
                    ProviderName = x.ServiceProvider != null ? x.ServiceProvider.Person != null ? x.ServiceProvider.Person.FirstName : "" : "",
                    CompanyName = x.ServiceProvider != null ? x.ServiceProvider.Company != null ? x.ServiceProvider.Company.Name : "" : "",
                });
            if (model.MarketJobId.HasValue)
            {
                data = data.Where(x => x.Model.JobRequestId == model.MarketJobId);
                var mJob = db.TenantJobRequest.FirstOrDefault( x => x.Id == model.MarketJobId);
                model.MarketJob = mJob.MapTo<MarketJobModel>();
            }
            var allItems = data.OrderByDescending(x => x.CreatedOn).ToPagedList(model.Page, 10);
            switch (model.SortOrder)
            {
                case "Company (A-Z)":
                    data = data.OrderBy(s => s.CompanyName);
                    break;
                case "Company (Z-A)":
                    data = data.OrderByDescending(s => s.CompanyName);
                    break;
                case "Lowest Amount":
                    data = data.OrderBy(s => s.Model.Amount);
                    break;
                case "Highest Amount":
                    data = data.OrderByDescending(s => s.Model.Amount);
                    break;

                default:
                    data = data.OrderByDescending(s => s.Model.Amount);
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
                        data = data.Where(x => x.Model.Amount.ToString() == formatString
                                       || x.CompanyName.ToLower().StartsWith(formatString)
                                       || x.Model.Note.ToLower().StartsWith(formatString));
                        break;
                    case 2:
                        data = data.Where(x => x.Model.Amount.ToString() == formatString
                                       || x.CompanyName.ToLower().EndsWith(formatString)
                                       || x.Model.Note.ToLower().EndsWith(formatString));
                        break;
                    case 3:
                        double number;
                        if (Double.TryParse(formatString, out number))
                        {
                            data = data.Where(y => double.Parse(y.Model.Amount.ToString().Split(',')[0]) <= double.Parse(formatString.Split(',')[0]));
                        }
                        else
                        {
                            data = data.Where(y => y.Model.Note.ToLower().Contains(formatString));
                        }
                        break;
                }
            }
            var items = data.ToPagedList(model.Page, 10);
            var count = items.Count;
            items = count == 0 ? allItems : items;
            items.ToList().ForEach(x => x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties()));
            var result = new SearchResult { SearchCount = items.Count, Items = count == 0 ? allItems : items };
            model.PagedInput = new PagedInput
            {
                ActionName = "GetJobQuotes",
                ControllerName = "Home",
                PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, SearchString = model.SearchString , MarketJobId = model.MarketJobId})
            };
            var formInputs = new List<SearchInput>();
            formInputs.Add(new SearchInput { Name = "MarketJobId", Value = model.MarketJobId.ToString() });
            model.InputValues = formInputs;
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString , MarketJobId = model.MarketJobId });
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Company (A-Z)", ActionName = "GetJobQuotes", RouteValues = rvr.AddRouteValue("SortOrder", "Name") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Company (Z-A)", ActionName = "GetJobQuotes", RouteValues = rvr.AddRouteValue("SortOrder", "Company (Z-A)") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Lowest Amount", ActionName = "GetJobQuotes", RouteValues = rvr.AddRouteValue("SortOrder", "Lowest Amount") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Highest Amount", ActionName = "GetJobQuotes", RouteValues = rvr.AddRouteValue("SortOrder", "Higest Amount") });
            model.SortOrders = sortOrders;
            model.SearchCount = result.SearchCount;
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            model.PageCount = result.Items.PageCount;
            model.Items = result.Items;
            var currentJob = db.TenantJobRequest.Where(x => x.Id == model.MarketJobId).FirstOrDefault();
            model.Address = currentJob.Property.Address?.ToAddressString() ?? "";
            return View(model);
        }

        #region Helper functions
        private string getAddress(Address address)
        {
            if (address != null)
            {
                return address.Street + " " + address.Suburb + " " + address.City + " " + address.Country.Name;
            }
            return "";
        }

        private string getName(Person person)
        {
            if (person != null)
            {
                if (person.MiddleName != null && person.MiddleName.Trim().Length > 0)
                {
                    return person.FirstName + " " + person.MiddleName + " " + person.LastName;
                }
                else
                {
                    return person.FirstName + " " + person.LastName;
                }
            }
            return "";
        }

        public ViewResult AdvanceSearch()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AdvanceSearch(string sortOrder, int? page, AdvancedMarketJobSearchModel advancedSearch)
        {
            TempData["AdvancedSearch"] = advancedSearch;
            return RedirectToAction("AdvancedSearchResult");
        }

        [HttpGet]
        public ActionResult AdvancedSearchResult(string sortOrder,string currentFilter, int? page)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var IsUserServiceSupplier = CompanyService.IsUserServiceSupplier(login.UserName);
            bool isOwner = false;
            AdvancedMarketJobSearchModel model = (AdvancedMarketJobSearchModel)TempData["AdvancedSearch"] ?? new AdvancedMarketJobSearchModel();
            model.Page = page ?? 1;
            if (model.Suburb == null) { model.Suburb = ""; }
            if (model.SuburbList == null) { model.SuburbList = new List<string>(); }
            if (model.Title == null) { model.Title = ""; }


            string newFilter = currentFilter;
            if (!String.IsNullOrEmpty(currentFilter))
            {
                Dictionary<string, string> keyValuePairs = currentFilter.Split(',')
                                    .Select(value => value.Split('='))
                                    .ToDictionary(pair => pair[0], pair => pair[1]);
                if (keyValuePairs["Title"] != "")
                {
                    model.Title = keyValuePairs["Title"];
                }
                if (keyValuePairs["MaxBudget"] != "")
                {
                    model.MaxBudget = Convert.ToInt32(keyValuePairs["MaxBudget"]);
                }
                if (keyValuePairs["MinBudget"] != "")
                {
                    model.MinBudget = Convert.ToInt32(keyValuePairs["MinBudget"]);
                }
                for (int j = 0; keyValuePairs.ContainsKey(("Suburb" + j)); j++)
                {
                    model.SuburbList.Add(keyValuePairs["Suburb" + j]);
                }

            }
            else
            {
               newFilter = "MaxBudget=" + model.MaxBudget.ToString() + ","
                                      + "MinBudget=" + model.MinBudget
                                      + "," + "Title=" + model.Title.ToString();
                for (int i = 0; i < model.SuburbList.Count; i++)
                {
                    newFilter = newFilter + ",Suburb" + i + "=" + model.SuburbList[i];
                }
            }
            var jobs = JobService.AdvancedSearchMarketJob(model,sortOrder);            
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString, ReturnUrl = model.ReturnUrl });
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest First", ActionName = "AdvancedSearchResult", RouteValues = rvr.AddRouteValue("SortOrder", "Latest First") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest First", ActionName = "AdvancedSearchResult", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest First") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Request Status", ActionName = "AdvancedSearchResult", RouteValues = rvr.AddRouteValue("SortOrder", "Request Status") });
            model.SortOrders = sortOrders;
            model.SearchCount = jobs.SearchCount;
            TempData["CurrentLink"] = isOwner ? "MarketJobsOwner" : "MarketJobs";
            if (jobs.SearchCount == 0)
            {
                ViewBag.SearchCount = 0;
            }
            model.PagedInput = new PagedInput
            {
                ActionName = "AdvancedSearchResult",
                ControllerName = "Home",
                PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, currentFilter = newFilter, ReturnUrl = model.ReturnUrl })
            };
            if (String.IsNullOrWhiteSpace(currentFilter)) model.Page = 1;
            model.PageCount =jobs.Items.PageCount;
            model.Items = jobs.Items;
            ViewBag.IsOwnerView = isOwner;
            ViewBag.IsUserServiceSupply = CompanyService.IsUserServiceSupplier(user);
            ViewBag.IsProfileComplete = CompanyService.IsProfileComplete(login);
            model.IsUserServiceSupply = CompanyService.IsUserServiceSupplier(user);
            model.IsProfileComplete = CompanyService.IsProfileComplete(login);
            return View(model);
        }
        #endregion
    }
}

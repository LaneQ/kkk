using KeysPlus.Service.Models;
using KeysPlus.Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KeysPlus.Data;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.Claims;
using System.Web.Security;
using KeysPlus.Website.Models;
using System.Configuration;
using TableDependency.SqlClient;
using System.Data.SqlClient;

namespace KeysPlus.Website.Controllers
{
    public class HomeController : Controller
    {
        KeysEntities db = new KeysEntities();

        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                //return RedirectToAction("Dashboard", "Home", new { area = "PropertyOwners" });
                return RedirectToAction("Dashboard", "Home", new { area = "" }); //Bug Fix #2031
            }

            return RedirectToAction("Login","Account");
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        [Authorize]
        public ActionResult Dashboard()
        {
            var user = User.Identity.Name;
            //var gp = User as GenericPrincipal;
            //var roles = gp.Claims
            //         .Where(c => c.Type == ClaimTypes.Role)
            //         .Select(c => c.Value);
            var login = AccountService.GetLoginByEmail(user);
            var isOwner = PropertyOwnerService.IsLoginOwner(login);
            var isTenant = TenantService.IsLoginATenant(login);
            var isServiceSupplier = CompanyService.IsServiceSupplier(login);
            var isPropertyManager = PropertyOwnerService.IsLoginPropertyManager(login);
            var tenant = db.Tenant.FirstOrDefault(x => x.Id == login.Id);
            ViewBag.IsOwner = false;
            ViewBag.IsTenant = false;
            ViewBag.IsServiceSupplier = false;
            ViewBag.IsPropertyManager = false;

            if (isOwner)
            {
                ViewBag.isOwner = true;
            }

            if (isTenant)
            {
                ViewBag.IsTenant = true;

                if (tenant != null && tenant.IsCompletedPersonalProfile == false)
                {
                    return RedirectToAction("Onboarding", "Home", new { area = "Tenants" });
                }
            }

            if (isServiceSupplier)
            {
                ViewBag.IsServiceSupplier = true;
            }

            if (isPropertyManager)
            {
                ViewBag.IsPropertyManager = true;
            }
            //if (tenant.IsCompletedPersonalProfile == false)
            //{
            //    return RedirectToAction("Onboarding", "Home", new { area = "Tenants" });
            //}

            TempData["CurrentLink"] = "Dashboard";
            return View();
        }


        public ActionResult TenantLogin()
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            ViewBag.IsOwner = false;
            ViewBag.IsTenant = true;
            ViewBag.IsServiceSupplier = false;
            ViewBag.IsPropertyManager = false;
            ViewBag.IsCompletedPersonalProfile = true;
            TempData["CurrentLink"] = "Dashboard";

            return View("Dashboard");
        }



        public ActionResult PODashboard()
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var props = PropertyService.GetAllPropertiesByOwner(login.Id);
            var totalProps = props.Count();
            //var ownerOccupied = db.OwnerProperty.Where(x => x.OwnerId == login.Id && x.Property.IsActive && (x.Property.IsOwnerOccupied ?? false)).Count();
            var tenantOccupied = db.OwnerProperty.Where(x => x.OwnerId == login.Id && x.Property.IsActive 
                                && x.Property.TenantProperty.Where(y => y.IsActive ?? false).Count() > 0).Count();
            
            var vacant = totalProps - (tenantOccupied);

            var propIds = props.Select(x => x.Id);
            var rentApps = db.RentalApplication.Where(x => propIds.Contains(x.RentalListing.PropertyId) && x.IsActive);
            var newApps = rentApps.Where(x => x.ApplicationStatusId == 1 && !(x.IsViewedByOwner ?? false)).Count();
            var approvedApps = rentApps.Where(x => x.ApplicationStatusId == 2).Count();
            var pendingApps = rentApps.Where(x => (x.IsViewedByOwner ?? false) && x.ApplicationStatusId == 1).Count();
            var declinedApps = rentApps.Where(x => x.ApplicationStatusId == 3).Count();

            var jobs = db.Job.Where(x => propIds.Contains(x.PropertyId) && x.JobStatusId != 5 && x.JobStatusId != 6);
            var newJobs = jobs.Where(x => x.PercentDone == 0).Count();
            var progressedJobs = jobs.Where(x => x.PercentDone > 0 && x.PercentDone < 100).Count();
            var finishedJobs = jobs.Where(x => x.PercentDone == 100).Count();

            var tenRequests = db.PropertyRequest.Where(x => propIds.Contains(x.Property.Id) && x.IsActive && x.ToOwner && !x.ToTenant);
            var newRequests = tenRequests.Where(x => x.RequestStatusId == 1).Count();
            var acceptedRequests = tenRequests.Where(x => x.RequestStatusId == 2).Count();
            var rejRequests = tenRequests.Where(x => x.RequestStatusId == 5).Count();

            var jobQuotes = db.JobQuote.Where(x => propIds.Contains(x.TenantJobRequest.PropertyId) && x.Status.ToLower() != "deleted");
            var newQuotes = jobQuotes.Where(x => x.Status.ToLower() == "opening" && !(x.IsViewed ?? false)).Count();
            var acceptedQuotes = jobQuotes.Where(x => x.Status.ToLower() == "accepted").Count();
            var pendingQuotes = jobQuotes.Where(x => x.Status.ToLower() == "opening" && (x.IsViewed ?? false)).Count();
            var rejectedQuotes = jobQuotes.Where(x => x.Status.ToLower() == "unsuccessful").Count();
            var model = new PODashBoardModel
            {
                PropDashboardData = new PropDashboardModel
                {
                    Occupied = tenantOccupied,
                    Vacant = vacant
                },
                RentAppsDashboardData = new RentAppDashboardModel
                {
                    NewItems = newApps,
                    Approved = approvedApps,
                    Pending = pendingApps,
                    Rejected = declinedApps,
                },
                JobsDashboardData = new JobsDashboardModel
                {
                    NewItems = newJobs,
                    InProgress = progressedJobs,
                    Resolved = finishedJobs,
                },
                RequestDashboardData = new TenantRequestDashboardModel
                {
                    Current = newRequests,
                    Accepted = acceptedRequests,
                    Rejected = rejRequests,
                    //Pending = tenRequests.Count() - (newRequests + acceptedRequests),
                },
                JobQuotesDashboardData = new JobQuotesDashboardModel
                {
                    NewItems = newQuotes,
                    Accepted = acceptedQuotes,
                    Pending = pendingQuotes,
                    Rejected = rejectedQuotes
                }

            };
            return PartialView("_PODashboard", model);
        }

        public ActionResult TenantDashboard(int? steps)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);

            var props = db.TenantProperty.Where(x => x.TenantId == login.Id && x.Property.IsActive && (x.IsActive ?? false));
            var propIds = props.Select(x => x.PropertyId);

            //ApplicationStatusId {1 : Applied, 2 : Accepted, 3: Declined}
            var rentApps = db.RentalApplication.Where(x => x.PersonId == login.Id && x.IsActive);
            var newApps = rentApps.Where(x => x.ApplicationStatusId == 1 && !(x.IsViewedByOwner ?? false)).Count();
            var acceptedApps = rentApps.Where(x => x.ApplicationStatusId == 2).Count();
            var pendingApps = rentApps.Where(x => x.ApplicationStatusId == 1 && (x.IsViewedByOwner ?? false)).Count();
            var declinedApps = rentApps.Where(x => x.ApplicationStatusId == 3).Count();

            // Get Landlord request's statistics
            var landlordreqs = db.PropertyRequest.Where(x => propIds.Contains(x.Property.Id) && x.IsActive && !x.ToOwner && x.ToTenant && x.RecipientId == login.Id);
            var newLandlordRequests = landlordreqs.Where(x => x.RequestStatusId == 1).Count();
            var acceptedLandlordRequests = landlordreqs.Where(x => x.RequestStatusId == 2).Count();
            var rejectedLandlordRequests = landlordreqs.Where(x => x.RequestStatusId == 5).Count();

            var tenRequests = db.PropertyRequest.Where(x => propIds.Contains(x.Property.Id) && x.IsActive && x.ToOwner && !x.ToTenant);
            var newRequests = tenRequests.Where(x => !x.IsViewed).Count();
            var acceptedRequests = tenRequests.Where(x => x.RequestStatusId == 2).Count();
            var rejRequests = tenRequests.Where(x => x.RequestStatusId == 5).Count();
            var tenant = db.Tenant.FirstOrDefault(x => x.Id == login.Id);

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
                    NewItems = newLandlordRequests,
                    Accepted = acceptedLandlordRequests,
                    Rejected = rejectedLandlordRequests
                },
                TenantRequestDashboardData = new TenantRequestDashboardModel
                {
                    Current = newRequests,
                    Accepted = acceptedRequests,
                    Rejected = rejRequests
                }

            };
            model.IntroSteps = steps.HasValue ? steps.Value : 0;
            return PartialView("_TenantDashboard", model);
        }

        public ActionResult ServiceSupplierDashboard()
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            
            // Get Quote statistics.
            var quotes = db.JobQuote.Where(x => x.ProviderId == login.Id);
            var newQuotes = quotes.Where(x => (bool)!x.IsViewed).Count();
            var openingQuotes = quotes.Where(x => x.Status.Equals("opening")).Count();
            var acceptedQuotes = quotes.Where(x => x.Status.Equals("accepted")).Count();
            var rejectedQuotes = quotes.Where(x => x.Status.Equals("unsuccessful")).Count();

            // Get Job statistics.
            var jobs = db.Job.Where(x => x.ProviderId == login.Id);
            var pendingJobs = jobs.Where(x => x.JobStatusId == 2).Count();
            var inprogressJobs = jobs.Where(x => x.JobStatusId == 3).Count(); // JobsStatusId 3 = "in progress"
            var finishedJobs = jobs.Where(x => x.JobStatusId == 4).Count(); // JobsStatusId 4 = "finished"

            var model = new ServiceSupplierDashboardModel
            {
                QuoteDashboardData = new ServiceSupplierQuoteDashboardModel
                {
                    NewItems = newQuotes,
                    Pending = openingQuotes,
                    Accepted = acceptedQuotes,
                    Rejected = rejectedQuotes
                },
                JobDashboardData = new ServiceSupplierJobDashboardModel
                {
                    NewItems = pendingJobs,
                    InProgress = inprogressJobs,
                    Resolved = finishedJobs

                }
            };
            return PartialView("_ServiceSupplierDashboard", model);
        }

        [Authorize]
        public ActionResult SideNavBar()
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var userRoles = AccountService.GetUserRolesbyEmail(user);
            var isOwner = PropertyOwnerService.IsLoginOwner(login);
            var isTenant = TenantService.IsLoginATenant(login);
            var isServiceSupplier = CompanyService.IsServiceSupplier(login);
            var isAdmin = AccountService.IsAdmin(login);
            var model = new SideBarViewModel
            {
                Roles = userRoles,
                IsPropertyOwner = isOwner,
                IsTenant = isTenant,
                IsServiceSupplier = isServiceSupplier,
                IsAdmin=isAdmin
            };
            return PartialView(model);
        }
        [Authorize]
        public ActionResult TopNavBar()
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            if (login == null) return View("Error");
            var userRoles = AccountService.GetUserRolesbyEmail(user);
            var isOwner = PropertyOwnerService.IsLoginOwner(login);
            var isTenant = TenantService.IsLoginATenant(login);
            var isServiceSupplier = CompanyService.IsServiceSupplier(login);
            var isAdmin = AccountService.IsAdmin(login);
            var model = new SideBarViewModel
            {
                Roles = userRoles,
                IsPropertyOwner = userRoles.Contains(4),
                IsTenant = userRoles.Contains(5),
                IsServiceSupplier = userRoles.Contains(6),
                IsAdmin = userRoles.Contains(2),
                IsPropManager = userRoles.Contains(3)
            };
            return PartialView(model);
        }
    }
}
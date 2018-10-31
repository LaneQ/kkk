using KeysPlus.Data;
using System;
using System.Linq;
using System.Web.Mvc;
using KeysPlus.Service.Models;
using KeysPlus.Service.Services;
using PagedList;
using KeysPlus.Website.Areas.Tools;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Web.Routing;

namespace KeysPlus.Website.Areas.Rental.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        KeysEntities db = new KeysEntities();

        public ActionResult Index(RentalListingSearchModel model)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var roles = AccountService.GetUserRolesbyEmail(user);
            var isTenant = roles.Contains(5);
            if (String.IsNullOrWhiteSpace(model.SortOrder))
            {
                model.SortOrder = "Latest Listing";
            }
            model.UserId = login.Id;
            var res = RentalService.GetAllRentalProperties(model);
            model.PagedInput = new PagedInput
            {
                ActionName = "Index",
                ControllerName = "Home",
                PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, SearchString = model.SearchString })
            };
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString });
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Title", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Title") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Title(Desc)", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Title(Desc)") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Lowest Rent", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Lowest Rent") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Highest Rent", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Highest Rent") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Available Date", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Available Date") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Available Date", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Available Date") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Listing", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Listing") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Listing", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Listing") });

            var tenant = TenantService.GetTenantByEmail(user);
            model.IsTenantProfileComplete = tenant?.IsCompletedPersonalProfile ?? false;
            model.IsUserTenant = isTenant;
            model.SortOrders = sortOrders;
            model.SearchCount = res.SearchCount;
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            model.PageCount = res.Items.PageCount;
            model.Items = res.Items;
            TempData["CurrentLink"] = "RentallListing";
            return View(model);
        }

        public async Task<ActionResult> AddRentalApplication(RentalApplicationModel model)
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
                var result = RentalService.AddRentallApllication(model, login, Request.Files);
                if (result.IsSuccess)
                {
                    var propertyOwner = RentalService.GetOwnerDetails(model);
                    var propertyOwnerLogin = AccountService.GetLoginById(propertyOwner.Id);
                    var property = db.Property.FirstOrDefault(x => x.Id == model.PropertyId);
                    var addressString = "";
                    if (property != null)
                    {
                        var address = PropertyService.GetAddressById(property.AddressId);
                        if (address != null)
                        {
                            if (address.Street != "" && address.City != "")
                            {
                                addressString = address.Number + " " + address.Street + ", " + address.Suburb + ", " + address.City + ", " + address.PostCode;
                            }
                        }
                    }
                    //  string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "PropertyOwners/Property/RentalProperties");
                    var nvc = new NameValueCollection();
                    nvc.Add("PropId", model.PropertyId.ToString());
                    nvc.Add("returnUrl", "/PropertyOwners/Property/RentalProperties");
                    string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "/PropertyOwners/Property/AllRentalApplications", UtilService.ToQueryString(nvc));
                    SendGridEmailModel mail = new SendGridEmailModel
                    {
                        RecipentName = propertyOwner.FirstName,
                        ButtonText = "",
                        ButtonUrl = url,
                        RecipentEmail = propertyOwnerLogin.Email,
                        Address = addressString,
                    };
                    await EmailService.SendEmailWithSendGrid(EmailType.NewApplicationEmail, mail);
                }
                return result.IsSuccess ? Json(new { Success = true }) : Json(new { Success = false, ErrorMsg = result.ErrorMessage });
            }
            return Json(new { Success = false });
        }

        public ActionResult SendRequest(string returnUrl, int? propId, int? type)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var properties = PropertyService.GetPropertiesAndAddress(login.Id, propId, true).ToList();
            properties.ForEach(x => x.AddressString = x.Address.ToAddressString());
            var requestTypes = PropertyService.GetRequestTypes().Where(x => x.Id != 1).ToList();
            if (type.HasValue)
            {
                requestTypes = requestTypes.Where( x => x.Id == type).ToList();
            }
            var model = new POSendRequestModel
            {
                AvalableProperties = properties,
                RequestTypes = requestTypes,
                ReturnUrl = returnUrl
            };
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SubmitPropertyRequest(RequestModel model)
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
                var result = PropertyService.AddPropertyRequest(login, model, Request.Files);

                // If request is sent sucessfully send an email 
                if (result.IsSuccess)
                {
                    var nvc = new NameValueCollection();
                    var property = db.Property.First(p => p.Id == model.PropertyId);
                    var address = property.Address.ToAddressString();
                    // Check if Owner or Tenant is sending request and add url button to redirect in email
                    var btnUrl = model.ToOwner ? "/PropertyOwner/Property/TenantRequests" : "/Tenants/Home/LandLordRequests";
                    var url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, btnUrl, UtilService.ToQueryString(nvc));
                    List<Recipient> recipient = null;
                    //Select Owners or Teannts depending on who sent Request.
                    // if email is been sent to owner
                    if (model.ToOwner)
                    {
                        recipient = property.OwnerProperty
                        .Select(x => x.Person)
                        .Select(x => new Recipient
                        {
                            Name = x.FirstName,
                            Email = x.Login.UserName,
                            PersonType = "Tenant"
                        }).ToList();
                    }
                    else
                    {
                        //if email is been sent to all the Tenants 
                        if (model.RecipientId == 0)
                        {
                            recipient = property.OwnerProperty
                            .Select(x => x.Person)
                            .Select(x => new Recipient
                            {
                                Name = x.FirstName,
                                Email = x.Login.UserName,
                                PersonType = "Tenant"
                            }).ToList();
                        }
                        else
                        {
                            // if email is been sent to specific Tenant
                            recipient = new List<Recipient>();
                            recipient.Add(new Recipient
                            {
                                Name = db.Person.First(x => x.Id == model.RecipientId).FirstName,
                                Email = db.Login.First(y => y.Id == model.RecipientId).UserName,
                                PersonType = "Owner"
                            });
                        }
                    }
                    await EmailService.SendEmailToGroup(EmailType.NewRequestEmail, recipient, url, address);
                }
                return result.IsSuccess ? Json(new { Success = true }) : Json(new { Success = false, ErrorMsg = result.ErrorMessage });
            }
            else
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                return Json(new { Success = false, Model = allErrors });
            }
        }

        [HttpGet]
        public ActionResult AdvancedSearch()
        {
            GetPropertyTypes();
            return View();
        }

        private void GetPropertyTypes()
        {
            var propertyType = PropertyService.GetAllProprtyTypes().ToList();
            ViewBag.TypeOfProperty = propertyType;
        }

        [HttpPost]
        public ActionResult AdvancedSearch(string sortOrder, int? page, RentalAdvancedSearchViewModel advancedSearch)
        {
            TempData["AdvancedSearch"] = advancedSearch;
            return RedirectToAction("AdvanceSearchResult", new { sortOrder = sortOrder ?? "Title" });
        }

        [HttpPost]
        public ActionResult AdvanceSearchResult(string sortOrder, int? page, RentalAdvancedSearchViewModel advancedSearch)
        {
            TempData["AdvancedSearch"] = advancedSearch;
            return RedirectToAction("AdvanceSearchResult", new { sortOrder = sortOrder ?? "Title" });
        }

        [HttpGet]
        public ActionResult AdvanceSearchResult(string sortOrder, string currentFilter, int? page)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var dbContext = db.RentalListing;
            var roles = AccountService.GetUserRolesbyEmail(User.Identity.Name);
            var isTenant = roles.Contains(5);
            var tenant = TenantService.GetTenantByEmail(User.Identity.Name);
            GetPropertyTypes();
            IQueryable<RentalListingModel> allRentalProperties = GetAllRentalProperties(sortOrder, isTenant);
            RentalAdvancedSearchViewModel advancedSearch = InitialiazeSuburbListAndPropertyType();
            string newFilter = CreateFilterForPaging(currentFilter, advancedSearch);
            ViewBag.SearchCount = 1;
            IQueryable<RentalListingModel> RentalProperties = ConfigureSorting(sortOrder, ref allRentalProperties);
            allRentalProperties = GetFilteredRentalProperties(allRentalProperties, advancedSearch);

            if (allRentalProperties.Count() == 0)
            {
                ViewBag.SearchCount = 0;
                ViewBag.CurrentFilter = "";
                allRentalProperties = RentalProperties;
            }
            allRentalProperties.ToList().ForEach(x => x.WatchListText = RentalService.GetWatchListStatus(x.Id, login.Id));
            TempData["CurrentLink"] = "RentallListing";
            CreatePaging(page, allRentalProperties, advancedSearch, newFilter);
            advancedSearch.IsUserTenant = isTenant;
            advancedSearch.IsTenantProfileComplete = tenant?.IsCompletedPersonalProfile ?? false;
            return View(advancedSearch);
        }

        private static void CreatePaging(int? page, IQueryable<RentalListingModel> allRentalProperties, RentalAdvancedSearchViewModel advancedSearch, string newFilter)
        {
            advancedSearch.PagedInput = new PagedInput
            {
                ActionName = "AdvanceSearchResult",
                ControllerName = "Home",
                PagedLinkValues = new RouteValueDictionary(new
                {
                    SortOrder = advancedSearch.SortOrder,
                    currentFilter = newFilter,
                    ReturnUrl = advancedSearch.ReturnUrl
                })
            };
            var rvr = new RouteValueDictionary(new { SearchString = advancedSearch.SearchString, ReturnUrl = advancedSearch.ReturnUrl });
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest First", ActionName = "AdvanceSearchResult", RouteValues = rvr.AddRouteValue("SortOrder", "Latest First") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest First", ActionName = "AdvanceSearchResult", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest First") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Request Status", ActionName = "AdvanceSearchResult", RouteValues = rvr.AddRouteValue("SortOrder", "Request Status") });
            advancedSearch.SortOrders = sortOrders;
            advancedSearch.SearchCount = allRentalProperties.Count();
            if (String.IsNullOrWhiteSpace(advancedSearch.SearchString)) advancedSearch.Page = page ?? 1;
            advancedSearch.PageCount = allRentalProperties.ToPagedList(advancedSearch.Page, 12).PageCount;
            advancedSearch.Items = allRentalProperties.ToPagedList(advancedSearch.Page, 12);
        }

        private static IQueryable<RentalListingModel> GetFilteredRentalProperties(IQueryable<RentalListingModel> allRentalProperties, RentalAdvancedSearchViewModel advancedSearch)
        {
            if (advancedSearch.CheckForNull(advancedSearch))
            {
                if (advancedSearch.Address.SuburbList.Count != 0)
                {
                    allRentalProperties = from r in allRentalProperties
                                          where advancedSearch.Address.SuburbList.Contains(r.Address.Suburb)
                                          select r;
                }
                if (advancedSearch.PropertyType.Count != 0)
                {
                    allRentalProperties = from r in allRentalProperties
                                          where advancedSearch.PropertyType.Contains(r.PropertyType)
                                          select r;
                }
                allRentalProperties = from r in allRentalProperties
                                      where r.Title.ToLower().Contains(advancedSearch.Title ?? r.Title.ToLower())
                                         && ((r.Bedrooms <= (advancedSearch.BedroomMax ?? r.Bedrooms)) && (r.Bedrooms >= (advancedSearch.BedroomMin ?? r.Bedrooms)))
                                         && ((r.Bathrooms <= (advancedSearch.BathroomMax ?? r.Bathrooms)) && (r.Bedrooms >= (advancedSearch.BathroomMin ?? r.Bathrooms)))
                                         && ((r.LandSqm <= (advancedSearch.LandSqmMax ?? r.LandSqm)) && (r.LandSqm >= (advancedSearch.LandSqmMin ?? r.LandSqm)))
                                         && ((r.TargetRent <= (advancedSearch.RentMax ?? r.TargetRent)) && (r.TargetRent >= (advancedSearch.RentMin ?? r.TargetRent)))
                                      select r;
            }

            return allRentalProperties;
        }

        private IQueryable<RentalListingModel> ConfigureSorting(string sortOrder,  ref IQueryable<RentalListingModel> allRentalProperties)
        {
            ViewBag.TitleSortParm = sortOrder == "Title" ? "Title_Desc" : "Title";
            ViewBag.RentSortParm = sortOrder == "TargetRent" ? "TargetRent_Desc" : "TargetRent";
            ViewBag.ListOnParm = sortOrder == "ListedDate" ? "ListedDate_Desc" : "ListedDate";

            var rentalProperties = allRentalProperties;
            switch (sortOrder)
            {
                case "Title":
                    allRentalProperties = allRentalProperties.OrderBy(s => s.Title);
                    break;
                case "Title_Desc":
                    allRentalProperties = allRentalProperties.OrderByDescending(s => s.Title);
                    break;

                case "TargetRent_Desc":
                    allRentalProperties = allRentalProperties.OrderByDescending(s => s.TargetRent);
                    break;
                case "TargetRent":
                    allRentalProperties = allRentalProperties.OrderBy(s => s.TargetRent);
                    break;

                case "ListedDate":
                    allRentalProperties = allRentalProperties.OrderBy(s => s.CreatedDate);
                    break;
                case "ListedDate_Desc":
                    allRentalProperties = allRentalProperties.OrderByDescending(s => s.CreatedDate);
                    break;
                default:
                    allRentalProperties = allRentalProperties.OrderByDescending(s => s.CreatedDate);
                    break;
            }

            return rentalProperties;
        }

        private static string CreateFilterForPaging(string currentFilter, RentalAdvancedSearchViewModel advancedSearch)
        {
            var newFilter = currentFilter;

            if (!String.IsNullOrEmpty(currentFilter))
            {
                Dictionary<string, string> keyValuePairs = currentFilter.Split(',')
                                    .Select(value => value.Split('='))
                                    .ToDictionary(pair => pair[0], pair => pair[1]);
                if (keyValuePairs["BedroomMax"] != "")
                {
                    advancedSearch.BedroomMax = Convert.ToInt32(keyValuePairs["BedroomMax"]);
                }
                if (keyValuePairs["BedroomMin"] != "")
                {
                    advancedSearch.BedroomMin = Convert.ToInt32(keyValuePairs["BedroomMin"]);
                }
                if (keyValuePairs["BathroomMax"] != "")
                {
                    advancedSearch.BathroomMax = Convert.ToInt32(keyValuePairs["BathroomMax"]);
                }
                if (keyValuePairs["BathroomMin"] != "")
                {
                    advancedSearch.BathroomMin = Convert.ToInt32(keyValuePairs["BathroomMin"]);
                }
                if (keyValuePairs["RentMax"] != "")
                {
                    advancedSearch.RentMax = Convert.ToDecimal(keyValuePairs["RentMax"]);
                }
                if (keyValuePairs["RentMin"] != "")
                {
                    advancedSearch.RentMin = Convert.ToDecimal(keyValuePairs["RentMin"]);
                }
                if (keyValuePairs["LandSqmMax"] != "")
                {
                    advancedSearch.LandSqmMax = Convert.ToInt32(keyValuePairs["LandSqmMax"]);
                }
                if (keyValuePairs["LandSqmMin"] != "")
                {
                    advancedSearch.LandSqmMin = Convert.ToInt32(keyValuePairs["LandSqmMin"]);
                }
                if (keyValuePairs["Title"] != "")
                {
                    advancedSearch.Title = keyValuePairs["Title"];
                }
                for (int j = 0; keyValuePairs.ContainsKey(("Suburb" + j)); j++)
                {
                    advancedSearch.Address.SuburbList.Add(keyValuePairs["Suburb" + j]);
                }
                for (int j = 0; keyValuePairs.ContainsKey(("PropertyType" + j)); j++)
                {
                    advancedSearch.PropertyType.Add(keyValuePairs["PropertyType" + j]);
                }
            }
            else
            {
                newFilter = "BedroomMax=" + advancedSearch.BedroomMax +
                            ",BedroomMin=" + advancedSearch.BedroomMin +
                            ",BathroomMax=" + advancedSearch.BathroomMax +
                            ",BathroomMin=" + advancedSearch.BathroomMin +
                            ",RentMax=" + advancedSearch.RentMax +
                            ",RentMin=" + advancedSearch.RentMin +
                            ",LandSqmMax=" + advancedSearch.LandSqmMax +
                            ",LandSqmMin=" + advancedSearch.LandSqmMin +
                            ",Title=" + advancedSearch.Title;
                for (int i = 0; i < advancedSearch.Address.SuburbList.Count; i++)
                {
                    newFilter = newFilter + ",Suburb" + i + "=" + advancedSearch.Address.SuburbList[i];
                }
                for (int i = 0; i < advancedSearch.PropertyType.Count; i++)
                {
                    newFilter = newFilter + ",PropertyType" + i + "=" + advancedSearch.PropertyType[i];
                }
            }

            return newFilter;
        }

        private RentalAdvancedSearchViewModel InitialiazeSuburbListAndPropertyType()
        {
            RentalAdvancedSearchViewModel advancedSearch = (RentalAdvancedSearchViewModel)TempData["AdvancedSearch"] ?? new RentalAdvancedSearchViewModel();
            if (advancedSearch.Address == null) { advancedSearch.Address = new AddressViewModel(); }
            if (advancedSearch.Address.SuburbList == null) { advancedSearch.Address.SuburbList = new List<string>(); }
            if (advancedSearch.Address.Suburb != null) { advancedSearch.Address.Suburb = advancedSearch.Address.Suburb.ToLower(); }
            if (advancedSearch.Title != null) { advancedSearch.Title = advancedSearch.Title.ToLower(); }
            if (advancedSearch.PropertyType == null) { advancedSearch.PropertyType = new List<String>(); }

            return advancedSearch;
        }

        private static IQueryable<RentalListingModel> GetAllRentalProperties(string sortOrder, bool isTenant)
        {
            var allRentalProperties = RentalService.GetRentalProperties(null, sortOrder).Select(x => new RentalListingModel
            {
                Id = x.Id,
                PropertyId = x.PropertyId,
                PropertyName = x.Property.Name,
                Title = x.Title,
                Address = new AddressViewModel
                {
                    AddressId = x.Property.Address.AddressId,
                    CountryId = x.Property.Address.CountryId,
                    Number = x.Property?.Address?.Number?.Replace(" ", ""),
                    Street = x.Property.Address?.Street?.Trim(),
                    Region = x.Property.Address?.Region?.Trim(),
                    District = x.Property.Address?.City?.Trim(),
                    Suburb = x.Property.Address.Suburb?.Trim() ?? "",
                    PostCode = x.Property.Address?.PostCode?.Replace(" ", ""),
                    Latitude = x.Property.Address?.Lat,
                    Longitude = x.Property.Address?.Lng
                },
                PropertyAddress = x.Property.Address.Number + " " + x.Property.Address.Street + ", " + x.Property.Address.Suburb + ", " + x.Property.Address.City + " - " + x.Property.Address.PostCode,
                PropertyType = x.Property.PropertyType.Name,
                RentalPaymentType = x.Property.TargetRentType.Name,
                Bedrooms = x.Property.Bedroom ?? 0,
                Bathrooms = x.Property.Bathroom ?? 0,
                ParkingSpaces = x.Property.ParkingSpace ?? 0,
                LandSqm = x.Property.LandSqm ?? 0,
                FloorArea = x.Property.FloorArea ?? 0,
                // PropertyDescription = x.Property.Description,
                RentalDescription = x.Description.Trim(),
                MovingCost = x.MovingCost ?? 0,
                TargetRent = x.TargetRent,
                RentType = x.Property.TargetRentType.Name,
                AvailableDate = x.AvailableDate,
                Furnishing = x.Furnishing,
                IdealTenant = x.IdealTenant,
                OccupantCount = x.OccupantCount ?? 0,
                PetsAllowed = x.PetsAllowed,
                CreatedBy = x.CreatedBy,
                CreatedOn = x.CreatedOn?.ToString("dd/MM/yyyy"),
                CreatedDate = x.CreatedOn,
                UpdatedBy = x.CreatedBy,
                IsTenant = isTenant,
                UpdatedOn = x.UpdatedOn?.ToString("dd/MM/yyyy"),
                RentalFiles = x.RentalListingMedia.Select(y => new MediaModel { NewFileName = y.NewFileName, OldFileName = y.OldFileName, Status = "load" }).ToList()
            }).ToList().AsQueryable();
            return allRentalProperties;
        }

        public JsonResult GetTenantsByPropertyId(int propId)
        {
            var result = PropertyService.GetTenantsByPropId(propId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }





    }

}
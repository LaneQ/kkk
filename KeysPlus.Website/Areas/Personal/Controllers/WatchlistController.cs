using KeysPlus.Data;
using KeysPlus.Service.Models;
using KeysPlus.Service.Services;
using PagedList;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace KeysPlus.Website.Areas.Personal.Controllers
{
    [Authorize]
    public class WatchlistController : Controller
    {
        KeysEntities db = new KeysEntities();
        // GET: Personal/Watchlist
        public ActionResult MarketJobs(WatchlistDisplayModel model)
        {
            var login = AccountService.GetLoginByEmail(User.Identity.Name);
            var userRoles = AccountService.GetUserRolesbyEmail(User.Identity.Name);
            model.IsUserServiceSupply = true;
            model.ItemType = model.ItemType ?? ItemType.MarketJob;
            model = GetMarketJobWatchlist(model, login);
            var tenant = TenantService.GetTenantByEmail(User.Identity.Name);
            model.IsUserTenant = userRoles.Contains(5);
            model.IsTenantProfileComplete = tenant?.IsCompletedPersonalProfile ?? false;
            model.IsProfileComplete = CompanyService.IsProfileComplete(login);
            var data = db.JobWatchList.Where(x => x.PersonId == login.Id && x.IsActive)
        .Select(x => new WatctlistItem<JobMarketModel>
        {
            Market = new MarketJobViewModel
            {
                IsApplyByUser = db.JobQuote.Any(y => y.JobRequestId == x.TenantJobRequest.Id && y.ProviderId == login.Id && y.Status.ToLower() == "opening"),
                IsOwnedByUser = db.TenantJobRequest.FirstOrDefault(y => y.Id == x.TenantJobRequest.Id).OwnerId == login.Id,
            },
            Model = new JobMarketModel
            {
                WatchListId = x.Id,
                Id = x.TenantJobRequest.Id,
                Title = x.TenantJobRequest.Title,
                MaxBudget = x.TenantJobRequest.MaxBudget,
                JobDescription = x.TenantJobRequest.JobDescription,
                PostedDate = x.TenantJobRequest.CreatedOn,
                MediaFiles = x.TenantJobRequest.TenantJobRequestMedia.Select(y => new MediaModel { Id = y.Id, NewFileName = y.NewFileName, OldFileName = y.OldFileName }).ToList()

            },
            Address = new AddressViewModel
            {
                AddressId = x.TenantJobRequest.Property.Address.AddressId,
                CountryId = x.TenantJobRequest.Property.Address.AddressId,
                Number = x.TenantJobRequest.Property.Address.Number,
                Street = x.TenantJobRequest.Property.Address.Street,
                Suburb = x.TenantJobRequest.Property.Address.Suburb,
                Region = x.TenantJobRequest.Property.Address.Region,
                City = x.TenantJobRequest.Property.Address.City,
                PostCode = x.TenantJobRequest.Property.Address.PostCode,
                Longitude = x.TenantJobRequest.Property.Address.Lng,
                Latitude = x.TenantJobRequest.Property.Address.Lat
            }
        });

            var allItems = data.OrderBy(x => x.Model.Title).ToPagedList(model.Page, 2);
            allItems.ToList().ForEach(x => x.Model.MediaFiles.ToList().ForEach(y => y.InjectMediaModelViewProperties()));

            if (string.IsNullOrWhiteSpace(model.SortOrder))
            {
                model.SortOrder = "Title";
            }
            switch (model.SortOrder)
            {
                case "Title":
                    data = data.OrderBy(x => x.Model.Title);
                    break;
                case "Title_Desc":
                    data = data.OrderByDescending(x => x.Model.Title);
                    break;
                case "MaxBudget":
                    data = data.OrderBy(x => x.Model.MaxBudget);
                    break;
                case "MaxBudget_Desc":
                    data = data.OrderByDescending(x => x.Model.MaxBudget);
                    break;
                case "Date_Desc":
                    data = data.OrderByDescending(x => x.Model.PostedDate);
                    break;
                case "Date":
                    data = data.OrderBy(x => x.Model.PostedDate);
                    break;
                default:
                    data = data.OrderByDescending(x => x.Model.Title);
                    break;
            }
            if (!String.IsNullOrWhiteSpace(model.SearchString))
            {
                SearchUtil searchTool = new SearchUtil();
                int searchType = searchTool.CheckDisplayType(model.SearchString);
                string formatString = searchTool.ConvertString(model.SearchString);
                data = data.Where(x => x.Model.Title.ToLower().Contains(formatString)
                                       );
            };

            var items = data.ToPagedList(model.Page, 9);
            items.ToList().ForEach(x => x.Model.MediaFiles.ToList().ForEach(y => y.InjectMediaModelViewProperties()));
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            var sortOrders = new List<SortOrderModel>();
            var rvr = new RouteValueDictionary(new { ItemType = "MarketJob", SearchString = model.SearchString });
            sortOrders.Add(new SortOrderModel { SortOrder = "Title", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Title") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Title_Desc", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Title_Desc") });
            sortOrders.Add(new SortOrderModel { SortOrder = "MaxBudget", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "MaxBudget") });
            sortOrders.Add(new SortOrderModel { SortOrder = "MaxBudget_Desc", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "MaxBudget_Desc") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Date_Desc", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Date_Desc") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Date", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Date") });
            model.SortOrders = sortOrders;
            model.PagedInput = new PagedInput
            {
                ActionName = "Index",
                ControllerName = "Watchlist",
                PagedLinkValues = new RouteValueDictionary(new { ItemType = "MarketJob", SortOrder = model.SortOrder, SearchString = model.SearchString })
            };
            model.PageCount = items.Count == 0 ? allItems.PageCount : items.PageCount;
            model.SearchCount = items.Count;
            model.Items = items.Count == 0 ? allItems : items;

            return View("MarketJobs",model);
        }
        public ActionResult Index(WatchlistDisplayModel model)
        {
            var login = AccountService.GetLoginByEmail(User.Identity.Name);
            var userRoles = AccountService.GetUserRolesbyEmail(User.Identity.Name);
            if (userRoles.Contains(5))
            {
                model.ItemType = model.ItemType ?? ItemType.RentalListing;
                model.IsUserTenant = true;
            }
            if (userRoles.Contains(6))
            {
                model.IsUserServiceSupply = true;
                model.ItemType = model.ItemType ?? ItemType.MarketJob;
            }

            using (var db = new KeysEntities())
            {

                if (model.ItemType == ItemType.RentalListing || model.ItemType == 0)
                {
                    model.ItemType = ItemType.RentalListing;
                    model = GetRentalWatchlist(model, login);
                }
                else if (model.ItemType == ItemType.MarketJob)
                {
                    model = GetMarketJobWatchlist(model, login);
                }
                var tenant = TenantService.GetTenantByEmail(User.Identity.Name);
                model.IsUserTenant = userRoles.Contains(5);
                model.IsTenantProfileComplete = tenant?.IsCompletedPersonalProfile ?? false;
                model.IsProfileComplete = CompanyService.IsProfileComplete(login);

                return View(model);
            }
        }

        public WatchlistDisplayModel GetRentalWatchlist(WatchlistDisplayModel model, Login login)
        {
            var data = db.RentalWatchList.Where(x => x.PersonId == login.Id && x.IsActive)
                .Select(x => new WatctlistItem<RentListingModel>
                {
                    View = new RentListingViewModel
                    {
                        IsOwner = db.OwnerProperty.FirstOrDefault(y => y.PropertyId == x.RentalListing.PropertyId).OwnerId == login.Id,
                        IsApplied = db.RentalApplication.Any(y => y.RentalListingId == x.RentalListing.Id && y.PersonId == login.Id),
                    },

                    Model = new RentListingModel
                    {
                        Id = x.RentalListing.Id,
                        WatchListId = x.Id,
                        MovingCost = x.RentalListing.MovingCost,
                        TargetRent = x.RentalListing.TargetRent,
                        AvailableDate = x.RentalListing.AvailableDate,
                        Furnishing = x.RentalListing.Furnishing,
                        OccupantCount = x.RentalListing.OccupantCount,
                        PetsAllowed = x.RentalListing.PetsAllowed,
                        Title = x.RentalListing.Title,
                        Description = x.RentalListing.Description,
                        PropertyId = x.RentalListing.PropertyId,
                        IdealTenant = x.RentalListing.IdealTenant,
                        IsActive = x.RentalListing.IsActive,
                        RentalStatusId = x.RentalListing.RentalStatusId,
                        MediaFiles = x.RentalListing.RentalListingMedia.Select(y => new MediaModel { Id = y.Id, NewFileName = y.NewFileName, OldFileName = y.OldFileName }).ToList()
                    },
                    Address = new AddressViewModel
                    {
                        Street = x.RentalListing.Property.Address.Street,
                        Suburb = x.RentalListing.Property.Address.Suburb,
                        AddressId = x.RentalListing.Property.Address.AddressId,
                        CountryId = x.RentalListing.Property.Address.AddressId,
                        Number = x.RentalListing.Property.Address.Number,
                        Region = x.RentalListing.Property.Address.Region,
                        City = x.RentalListing.Property.Address.City,
                        PostCode = x.RentalListing.Property.Address.PostCode,
                        Latitude = x.RentalListing.Property.Address.Lat,
                        Longitude = x.RentalListing.Property.Address.Lng
                    },
                    Property = new PropertyViewModel
                    {
                        Bedroom = x.RentalListing.Property.Bedroom,
                        Bathroom = x.RentalListing.Property.Bathroom,
                        FloorArea = x.RentalListing.Property.FloorArea,
                        LandArea = x.RentalListing.Property.LandSqm,
                        ParkingSpace = x.RentalListing.Property.ParkingSpace,
                        CreatedDate = x.RentalListing.Property.CreatedOn,
                        PropertyType = x.RentalListing.Property.PropertyType.Name,
                        RentalPaymentType = x.RentalListing.Property.TargetRentType.Name
                    },
                });

            var allItems = data.OrderBy(x => x.Model.Title).ToPagedList(model.Page, 2);
            allItems.ToList().ForEach(x => x.Model.MediaFiles.ToList().ForEach(y => y.InjectMediaModelViewProperties()));

            if (string.IsNullOrWhiteSpace(model.SortOrder))
            {
                model.SortOrder = "Latest Listing";
            }
            switch (model.SortOrder)
            {
                case "Title":
                    data = data.OrderBy(x => x.Model.Title);
                    break;
                case "Title_Desc":
                    data = data.OrderByDescending(x => x.Model.Title);
                    break;
                case "Highest Rent":
                    data = data.OrderByDescending(x => x.Model.TargetRent);
                    break;
                case "Lowest Rent":
                    data = data.OrderBy(x => x.Model.TargetRent);
                    break;
                case "Latest Avaible":
                    data = data.OrderByDescending(x => x.Model.AvailableDate);
                    break;
                case "Earliest Avaible":
                    data = data.OrderBy(x => x.Model.AvailableDate);
                    break;
                case "Latest Listing":
                    data = data.OrderByDescending(x => x.Property.CreatedDate);
                    break;
                case "Earliest Listing":
                    data = data.OrderBy(x => x.Property.CreatedDate);
                    break;
                default:
                    data = data.OrderByDescending(x => x.Property.CreatedDate);
                    break;
            }
            if (!String.IsNullOrWhiteSpace(model.SearchString))
            {
                SearchUtil searchTool = new SearchUtil();
                int searchType = searchTool.CheckDisplayType(model.SearchString);
                string formatString = searchTool.ConvertString(model.SearchString);
                data = data.Where(x => x.Model.Title.ToLower().Contains(formatString)
                                 || x.Address.City.ToLower().Contains(formatString)
                                 || x.Address.Number.ToLower().Contains(formatString)
                                 || x.Address.PostCode.ToLower().Contains(formatString)
                                 || x.Address.Region.ToLower().Contains(formatString)
                                 || x.Address.Street.ToLower().Contains(formatString)
                                 || x.Address.Suburb.ToLower().Contains(formatString)
                                 || x.Model.AvailableDate.ToString().Contains(formatString)
                                 || x.Model.Description.ToLower().Contains(formatString)
                                       );
            };
            var items = data.ToPagedList(model.Page, 10);
            items.ToList().ForEach(x => x.Model.MediaFiles.ToList().ForEach(y => y.InjectMediaModelViewProperties()));
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            var sortOrders = new List<SortOrderModel>();
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString });
            sortOrders.Add(new SortOrderModel { SortOrder = "Title", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Title") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Title_Desc", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Title_Desc") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Highest Rent", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Highest Rent") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Lowest Rent", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Lowest Rent") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Available", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Available") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Available", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Available") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Listing", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Listing") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Listing", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Listing") });
            model.SortOrders = sortOrders;
            model.PagedInput = new PagedInput
            {
                ActionName = "Index",
                ControllerName = "Watchlist",
                PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, SearchString = model.SearchString })
            };
            model.PageCount = items.Count == 0 ? allItems.PageCount : items.PageCount;
            model.SearchCount = items.Count;
            model.Items = items.Count == 0 ? allItems : items;
            return model;
        }

        private WatchlistDisplayModel GetMarketJobWatchlist(WatchlistDisplayModel model, Login login)
        {
            var data = db.JobWatchList.Where(x => x.PersonId == login.Id && x.IsActive)
                    .Select(x => new WatctlistItem<JobMarketModel>
                    {
                        Market = new MarketJobViewModel
                        {
                            IsApplyByUser = db.JobQuote.Any(y => y.JobRequestId == x.TenantJobRequest.Id && y.ProviderId == login.Id && y.Status.ToLower() == "opening"),
                            IsOwnedByUser = db.TenantJobRequest.FirstOrDefault(y => y.Id == x.TenantJobRequest.Id).OwnerId == login.Id,
                        },
                        Model = new JobMarketModel
                        {
                            WatchListId = x.Id,
                            Id = x.TenantJobRequest.Id,
                            Title = x.TenantJobRequest.Title,
                            MaxBudget = x.TenantJobRequest.MaxBudget,
                            JobDescription = x.TenantJobRequest.JobDescription,
                            PostedDate = x.TenantJobRequest.CreatedOn,
                            MediaFiles = x.TenantJobRequest.TenantJobRequestMedia.Select(y => new MediaModel { Id = y.Id, NewFileName = y.NewFileName, OldFileName = y.OldFileName }).ToList()

                        },
                        Address = new AddressViewModel
                        {
                            AddressId = x.TenantJobRequest.Property.Address.AddressId,
                            CountryId = x.TenantJobRequest.Property.Address.AddressId,
                            Number = x.TenantJobRequest.Property.Address.Number,
                            Street = x.TenantJobRequest.Property.Address.Street,
                            Suburb = x.TenantJobRequest.Property.Address.Suburb,
                            Region = x.TenantJobRequest.Property.Address.Region,
                            City = x.TenantJobRequest.Property.Address.City,
                            PostCode = x.TenantJobRequest.Property.Address.PostCode,
                            Longitude = x.TenantJobRequest.Property.Address.Lng,
                            Latitude = x.TenantJobRequest.Property.Address.Lat
                        }
                    });

            var allItems = data.OrderBy(x => x.Model.Title).ToPagedList(model.Page, 2);
            allItems.ToList().ForEach(x => x.Model.MediaFiles.ToList().ForEach(y => y.InjectMediaModelViewProperties()));

            if (string.IsNullOrWhiteSpace(model.SortOrder))
            {
                model.SortOrder = "Title";
            }
            switch (model.SortOrder)
            {
                case "Title":
                    data = data.OrderBy(x => x.Model.Title);
                    break;
                case "Title_Desc":
                    data = data.OrderByDescending(x => x.Model.Title);
                    break;
                case "MaxBudget":
                    data = data.OrderBy(x => x.Model.MaxBudget);
                    break;
                case "MaxBudget_Desc":
                    data = data.OrderByDescending(x => x.Model.MaxBudget);
                    break;
                case "Date_Desc":
                    data = data.OrderByDescending(x => x.Model.PostedDate);
                    break;
                case "Date":
                    data = data.OrderBy(x => x.Model.PostedDate);
                    break;
                default:
                    data = data.OrderByDescending(x => x.Model.Title);
                    break;
            }
            if (!String.IsNullOrWhiteSpace(model.SearchString))
            {
                SearchUtil searchTool = new SearchUtil();
                int searchType = searchTool.CheckDisplayType(model.SearchString);
                string formatString = searchTool.ConvertString(model.SearchString);
                data = data.Where(x => x.Model.Title.ToLower().Contains(formatString)
                                       );
            };

            var items = data.ToPagedList(model.Page, 9);
            items.ToList().ForEach(x => x.Model.MediaFiles.ToList().ForEach(y => y.InjectMediaModelViewProperties()));
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            var sortOrders = new List<SortOrderModel>();
            var rvr = new RouteValueDictionary(new { ItemType = "MarketJob", SearchString = model.SearchString });
            sortOrders.Add(new SortOrderModel { SortOrder = "Title", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Title") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Title_Desc", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Title_Desc") });
            sortOrders.Add(new SortOrderModel { SortOrder = "MaxBudget", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "MaxBudget") });
            sortOrders.Add(new SortOrderModel { SortOrder = "MaxBudget_Desc", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "MaxBudget_Desc") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Date_Desc", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Date_Desc") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Date", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Date") });
            model.SortOrders = sortOrders;
            model.PagedInput = new PagedInput
            {
                ActionName = "Index",
                ControllerName = "Watchlist",
                PagedLinkValues = new RouteValueDictionary(new { ItemType = "MarketJob", SortOrder = model.SortOrder, SearchString = model.SearchString })
            };
            model.PageCount = items.Count == 0 ? allItems.PageCount : items.PageCount;
            model.SearchCount = items.Count;
            model.Items = items.Count == 0 ? allItems : items;
            return model;
        }

        [HttpPost]
        public ActionResult SaveToWatchlist(SaveWatchlistModel model)
        {
            var user = AccountService.GetLoginByEmail(User.Identity.Name);
            using (var db = new KeysEntities())
            {
                if (model.ItemType == ItemType.RentalListing)
                {
                    var rentalWatchlist = new RentalWatchList
                    {
                        RentalListingId = model.Id,
                        PersonId = user.Id,
                        IsActive = true,
                        CreatedBy = user.UserName,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow,
                    };
                    db.RentalWatchList.Add(rentalWatchlist);
                }
                else if (model.ItemType == ItemType.MarketJob)
                {
                    var jobWatchlist = new JobWatchList
                    {
                        MarketJobId = model.Id,
                        PersonId = user.Id,
                        IsActive = true,
                        CreatedBy = user.UserName,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow,
                    };
                    db.JobWatchList.Add(jobWatchlist);
                }
                else return Json(new { Success = false });
                try
                {
                    db.SaveChanges();
                    return Json(new { Success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { Success = false });
                }
            }
        }
        public async Task<ActionResult> AddRentalApplication(RentalApplicationModel model)
        {
            if (ModelState.IsValid)
            {
                var files = Request.Files;
                var userName = User.Identity.Name;

                if (String.IsNullOrEmpty(userName))
                {
                    return Json(new { Success = false, ErrorMsg = "User not exist!" });
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

        [HttpPost]
        public ActionResult DeleteWatchlist(int Id, ItemType Type)
        {
            var result = new ServiceResponseResult();
            if (Type == ItemType.RentalListing)
            {
                result = PropertyService.DeleteWatchlistItemById(Id);
            }
            if (Type == ItemType.MarketJob)
            {
                result = JobService.DeleteWatchlistItemById(Id);
            }
            if (result.IsSuccess)
                return Json(new
                {
                    Success = true,
                    Message = "IsDeleted",
                    Updated = true,
                });
            else
            {
                return Json(new { Success = false, Message = result.ErrorMessage });
            }
        }

        [HttpPost]
        public ActionResult RemoveFromWatchlist(int Id, ItemType Type)
        {
            var login = AccountService.GetLoginByEmail(User.Identity.Name);
            var result = new ServiceResponseResult();
            if (Type == ItemType.RentalListing)
            {
                result = TenantService.RemoveFromWatchlist(Id, login);
            }
            if (Type == ItemType.MarketJob)
            {
                result = CompanyService.RemoveFromWatchlist(Id, login);
            }
            if (result.IsSuccess)
                return Json(new
                {
                    Success = true,
                    Message = "IsDeleted",
                    Updated = true,
                });
            else
            {
                return Json(new { Success = false, Message = result.ErrorMessage });
            }
        }
    }
}
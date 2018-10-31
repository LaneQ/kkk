using KeysPlus.Data;
using KeysPlus.Service.Models;
using KeysPlus.Service.Services;
using KeysPlus.Website.Areas.Tools;
using KeysPlus.Website.Models;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace KeysPlus.Website.Areas.PropertyOwners.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        KeysEntities db = new KeysEntities();
        public ActionResult Index(POPropSearchModel model)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            if (String.IsNullOrWhiteSpace(model.SortOrder))
            {
                model.SortOrder = "Latest Date";
            }
            var props = PropertyService.GetAllPropertiesByOwner(login.Id);
            var propIds = props.Select(x => x.Id);
            var data = db.OwnerProperty.Where(x => x.OwnerId == login.Id && x.Property.IsActive)
            .Select(x => new PropViewModel
            {
                Model = new PropertyModel
                {
                    Id = x.PropertyId,
                    PropertyTypeId = x.Property.PropertyTypeId,
                    Name = x.Property.Name,
                    Description = x.Property.Description,
                    Bedroom = x.Property.Bedroom,
                    Bathroom = x.Property.Bathroom,
                    LandSqm = x.Property.LandSqm,
                    ParkingSpace = x.Property.ParkingSpace,
                    FloorArea = x.Property.FloorArea,
                    TargetRent = x.Property.TargetRent,
                    TargetRentTypeId = x.Property.TargetRentTypeId,
                    YearBuilt = x.Property.YearBuilt,
                    IsOwnerOccupied = x.Property.IsOwnerOccupied,
                    Address = new AddressViewModel
                    {
                        AddressId = x.Property.Address.AddressId,
                        CountryId = x.Property.Address.CountryId,
                        Number = x.Property.Address.Number.Replace(" ", ""),
                        Street = x.Property.Address.Street.Trim(),
                        City = x.Property.Address.City.Trim(),
                        Suburb = x.Property.Address.Suburb.Trim() ?? "",
                        PostCode = x.Property.Address.PostCode.Replace(" ", ""),
                        Latitude = x.Property.Address.Lat,
                        Longitude = x.Property.Address.Lng,
                    },
                    MediaFiles = x.Property.PropertyMedia.Select(y => new MediaModel { Id = y.Id, NewFileName = y.NewFileName, OldFileName = y.OldFileName }).ToList()
                },
                NewQuotesCount = x.Property.TenantJobRequest.Select(k => k.JobQuote.Where(t => t.Status == "opening")).Count(),
                NewRequestsCount = x.Property.PropertyRequest.Where(y => propIds.Contains(y.Property.Id) && y.IsActive && y.ToOwner && !y.ToTenant).Where(y => y.IsViewed).Count(),
                PropertyAddress = x.Property.Address.Number + " " + x.Property.Address.Street + " " + x.Property.Address.Suburb + " " + x.Property.Address.City + " " + x.Property.Address.PostCode,
                PropertyTypeName = x.Property.PropertyType.Name,
                CreatedOn = x.Property.CreatedOn,
                PurchasePrice = x.Property.PropertyFinance.PurchasePrice,
                CurrentHomeValue = x.Property.PropertyHomeValue.Where(y => y.IsActive == true).FirstOrDefault() != null ? x.Property.PropertyHomeValue.Where(y => y.IsActive == true).FirstOrDefault().Value : x.Property.PropertyFinance.CurrentHomeValue,
                TenantCount = x.Property.TenantProperty.Where(y => y.IsActive ?? true).Count(),
                Mortgage = x.Property.PropertyFinance.Mortgage ?? 0,
            });

            switch (model.SortOrder)
            {
                case "Name":
                    data = data.OrderBy(s => s.Model.Name);
                    break;
                case "Name(Desc)":
                    data = data.OrderByDescending(s => s.Model.Name);
                    break;
                case "Latest Date":
                    data = data.OrderByDescending(s => s.CreatedOn);
                    break;
                case "Earliest Date":
                    data = data.OrderBy(s => s.CreatedOn);
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
                        data = data.Where(x => x.Model.Name.ToLower().EndsWith(formatString)
                                             || x.Model.Address.City.ToLower().EndsWith(formatString)
                                             || x.Model.Address.Suburb.ToLower().EndsWith(formatString));
                        break;
                    case 2:
                        data = data.Where(x => x.Model.Name.ToLower().StartsWith(formatString)
                                              || x.Model.Address.City.ToLower().StartsWith(formatString)
                                              || x.Model.Address.Suburb.ToLower().StartsWith(formatString));
                        break;
                    case 3:
                        data = data.Where(x => x.Model.Name.ToLower().Contains(formatString)
                                                || x.Model.Address.Street.ToLower().Contains(formatString)
                                                || x.Model.Address.City.ToLower().Contains(formatString)
                                               || x.Model.Address.Suburb.ToLower().Contains(formatString)
                                               || x.PropertyAddress.ToLower().Contains(formatString));
                        break;
                }
            }

            var items = data.ToPagedList(model.Page, 10);
            var count = items.Count;
            //items = count == 0 ? allItems : items;
            items.ToList().ForEach(x => x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties()));
            var result = new SearchResult { SearchCount = items.Count, Items = count == 0  ? allItems : items, NoResultFound = (count == 0) };
            //var result = new SearchResult { SearchCount = items.Count, Items = items };
            model.PagedInput = new PagedInput
            {
                ActionName = "Index",
                ControllerName = "Home",
                PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, SearchString = model.SearchString })
            };
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString });
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Name", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Name") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Name(Desc)", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Name(Desc)") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Date", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Date") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Date", ActionName = "Index", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Date") });
            model.SortOrders = sortOrders;
            model.SearchCount = result.SearchCount;
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            model.NoResultFound = model.Page == 1 ? result.NoResultFound : false;
            model.PageCount = result.Items.PageCount;
            model.Items = result.Items;
            model.EditUrl = "/PropertyOwners/Home/Edit";
            model.DeleteUrl = "/PropertyOwners/Home/Delete";
            model.CanListRental = allItems.Any();
            ViewBag.Frequencies = PropertyOwnerService.GetAllPaymentFrequencies();
            ViewBag.PropertyTypes = PropertyService.GetAllProprtyTypes();
            model.IsPropertyIndexPage = !model.IsFinanceDetailPage;
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(PropertyViewModel model)
        {
            var status = true;
            var message = "";
            var data = model;
            var currentUser = db.Person.Where(x => x.Login.UserName == User.Identity.Name).FirstOrDefault();
            var newOwner = new Owners();
            // model.PropertyTypeId = 1;
            var OwnerPerson = db.Owners.Any(n => n.Person.Login.UserName == User.Identity.Name);
            if (OwnerPerson == false)
            {
                newOwner.Person = currentUser;
                db.Owners.Add(newOwner);
                db.SaveChanges();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                status = false;
                message = "Something went wrong, perhaps a field is invalid.";
            }
            else
            {
                var address = model.Address;
                var newProperty = new Property();
                // var newFinancial = new PropertyFinance();
                var newOwnerProperty = new OwnerProperty();
                var newpropModel = new PropertyViewModel();
                var newAddressModel = new AddressViewModel();
                var newAddress = new Address
                {
                    CountryId = address.CountryId,
                    Number = address.Number.Replace(" ", ""),
                    Street = address.Street.Trim(),
                    City = address.City.Trim(),
                    PostCode = address.PostCode.Replace(" ", ""),
                    Suburb = address.Suburb,
                    CreatedBy = HttpContext.User.Identity.Name,
                    CreatedOn = DateTime.Today,
                    UpdatedBy = HttpContext.User.Identity.Name,
                    UpdatedOn = DateTime.Today,
                    Lat = address.Latitude,
                    Lng = address.Longitude,
                    IsActive = true
                };

                db.Address.Add(newAddress);
                db.SaveChanges();

                newAddressModel.AddressId = newAddress.AddressId;
                newAddressModel.CountryId = newAddress.CountryId;
                newAddressModel.Number = newAddress.Number.Replace(" ", "");
                newAddressModel.Street = newAddress.Street.Trim();
                newAddressModel.City = newAddress.City.Trim();
                newAddressModel.Suburb = newAddress.Suburb;
                newAddressModel.PostCode = newAddress.PostCode.Replace(" ", "");
                newAddressModel.Latitude = newAddress.Lat;
                newAddressModel.Longitude = newAddress.Lng;

                newProperty.Address = newAddress;
                newProperty.Name = model.Name;
                newProperty.IsActive = model.IsActive;
                newProperty.Bathroom = model.Bathroom;
                newProperty.Bedroom = model.Bedroom;
                newProperty.CreatedBy = User.Identity.Name;
                newProperty.CreatedOn = DateTime.Today;
                newProperty.UpdatedBy = User.Identity.Name;
                newProperty.UpdatedOn = DateTime.Today;
                newProperty.Description = model.Description;
                newProperty.FloorArea = model.FloorArea;
                newProperty.LandSqm = model.LandArea;
                newProperty.ParkingSpace = model.ParkingSpace;
                newProperty.YearBuilt = model.YearBuilt;
                newProperty.PropertyTypeId = model.PropertyTypeId;
                newProperty.TargetRent = model.PaymentAmount;
                newProperty.TargetRentTypeId = model.TargetRentType;
                newProperty.IsOwnerOccupied = model.IsOwnerOccupied;
                db.Property.Add(newProperty);

                /*
                newFinancial.Property = newProperty;
                newFinancial.PurchasePrice = model.PurchasePrice;
                newFinancial.TotalRepayment = model.TotalRepayment;
                newFinancial.Mortgage = model.Mortgage;
                db.PropertyFinance.Add(newFinancial);
                */

                newOwnerProperty.Property = newProperty;
                newOwnerProperty.OwnershipStatusId = 1;
                newOwnerProperty.Person = currentUser;
                newOwnerProperty.OwnerId = currentUser.Id;
                newOwnerProperty.CreatedBy = User.Identity.Name;
                newOwnerProperty.UpdatedBy = User.Identity.Name;
                newOwnerProperty.PurchaseDate = DateTime.Today;  // Should Create a feild in the Add property page and get value from there
                newOwnerProperty.UpdatedOn = DateTime.Today;
                newOwnerProperty.CreatedOn = DateTime.Today;

                db.OwnerProperty.Add(newOwnerProperty);

                db.SaveChanges();

                newpropModel.Id = newProperty.Id;
                newpropModel.Address = newAddressModel;
                newpropModel.IsActive = newProperty.IsActive;
                newpropModel.CreatedOn = newProperty.CreatedOn.ToString("s", CultureInfo.InvariantCulture);
                newpropModel.Name = newProperty.Name;
                newpropModel.Bathroom = newProperty.Bathroom ?? 0;
                newpropModel.Bedroom = newProperty.Bedroom ?? 0;
                newpropModel.FloorArea = newProperty.FloorArea;
                newpropModel.LandArea = newProperty.LandSqm;
                newpropModel.ParkingSpace = newProperty.ParkingSpace;
                newpropModel.PropertyTypeId = newProperty.PropertyTypeId;
                newpropModel.PropertyType = (newProperty.PropertyTypeId >= 7) ? "Commercial" : "Residential";
                newpropModel.TargetRent = newProperty.TargetRent ?? 0;
                newpropModel.TargetRentType = model.TargetRentType;
                newpropModel.YearBuilt = newProperty.YearBuilt ?? 0;
                newpropModel.Description = newProperty.Description;
                newpropModel.RentalApplications = new List<RentalApplicationsViewModel> { };
                data = newpropModel;
            }

            return Json(new
            {
                success = status,
                message = message,
                data = data
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PropertyModel model)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            if (String.IsNullOrEmpty(user))
            {
                return Json(new { Success = false });
            }
            var files = Request.Files;
            var result = PropertyOwnerService.EditProperty(model, files, login);
            return Json(new
            {
                Success = result.IsSuccess,
                Message = result.ErrorMessage,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            var user = User.Identity.Name;

            var property = db.Property.First(p => p.Id == id);
            if (property == null)
            {
                return Json(new
                {
                    Success = false,
                    message = "The selected property can't be found in the database."
                });
            }
            else
            {
                property.IsActive = false;
                var op = db.OwnerProperty.FirstOrDefault(x => x.PropertyId == id);
                db.OwnerProperty.Remove(op);
                var applicants = property.RentalListing.Where(x => x.IsActive).SelectMany(x => x.RentalApplication)
                    .Where(x => x.IsActive && x.ApplicationStatusId != 3)
                    .Select(x => x.Person).Select(x => new Recipient
                    {
                        Name = x.FirstName,
                        Email = x.Login.UserName
                    }).ToList();
                var nvc = new NameValueCollection();
                var url1 = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "/Tenants/Home/MyRentalApplications", UtilService.ToQueryString(nvc));
                var tenants = property.TenantProperty.Where(x => x.IsActive ?? false)
                    .Select(x => x.Tenant.Person)
                    .Select(x => new Recipient
                    {
                        Name = x.FirstName,
                        Email = x.Login.UserName
                    }).ToList();
                var url2 = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "/Tenants/Home/MyRentals", UtilService.ToQueryString(nvc));
                var serviceSuppliers = property.Job.Where(x => x.JobStatusId != 5 || x.JobStatusId != 6 && x.ProviderId != null)
                    .Select(x => x.ServiceProvider.Person)
                    .Select(x => new Recipient
                    {
                        Name = x.FirstName,
                        Email = x.Login.UserName
                    });
                var url3 = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "/Companies/Home/MyJobs", UtilService.ToQueryString(nvc));
                foreach (var item in property.RentalListing)
                {
                    item.IsActive = false;
                    foreach (var app in item.RentalApplication)
                    {
                        app.ApplicationStatusId = 3;
                    }
                }
                foreach (var item in property.TenantProperty) item.IsActive = (item.IsActive ?? false) ? false : item.IsActive;

                foreach (var item in property.Job) item.JobStatusId = (item.JobStatusId != 5 && item.JobStatusId != 6) ? item.JobStatusId = 5 : item.JobStatusId;
                var address = property.Address.ToAddressString();
                await EmailService.SendEmailToGroup(EmailType.DeletePropertyRentApplicationDeclined, applicants, url1, address);
                await EmailService.SendEmailToGroup(EmailType.DeletePropertyRentalCanceled, tenants, url2, address);
                await EmailService.SendEmailToGroup(EmailType.DeletePropertyJobCanceled, serviceSuppliers, url3, address);
                db.SaveChanges();

                return Json(new
                {
                    Success = true,
                    message = "Property deleted successfully",
                    id = id
                });
            }
        }

        public JsonResult UpdatePhotos(int id)
        {
            var allowedPhotos = 10;
            var message = "Photo added successfully";
            var status = true;
            var photos = Request.Files;
            var propId = id;

            if (photos.Count > allowedPhotos)
            {
                status = false;
                message = $"You can't add more than {allowedPhotos} photos";
            }

            if (photos != null && photos.Count > 0)
            {
                var numberOfPhotos = photos.Count < allowedPhotos ? photos.Count : allowedPhotos;

                List<string> acceptedExtensions = new List<string>
                {
                    ".jpg",
                    ".png",
                    ".gif",
                    ".jpeg"
                };

                for (int i = 0; i < numberOfPhotos; i++)
                {
                    var file = photos[i];

                    var fileExtension = Path.GetExtension(file.FileName);

                    if (fileExtension != null && !acceptedExtensions.Contains(fileExtension.ToLower()))
                    {
                        message = "Supported file types are *.jpg, *.png, *.gif, *.jpeg";
                        status = false;
                        break;
                    }
                    else
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var index = fileName.LastIndexOf(".");
                        var newFileName = fileName.Insert(index, $"{Guid.NewGuid()}");
                        var physicalPath = Path.Combine(Server.MapPath("~/images"), newFileName);

                        db.PropertyMedia.Add(new PropertyMedia()
                        {
                            PropertyId = id,
                            NewFileName = newFileName,
                            OldFileName = fileName
                        });

                        file.SaveAs(physicalPath);
                    }
                }
                db.SaveChanges();
            }
            else
            {
                message = "You have not specified a file.";
                status = false;
            }

            return Json(new
            {
                success = status,
                message = message
            });
        }
        [HttpPost]
        public JsonResult AddTenant(FinancialModel model)
        {
            AddTenantToPropertyModel tenant = new AddTenantToPropertyModel();
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var ten = AccountService.GetLoginByEmail(model.TenantToPropertyModel.TenantEmail);
            if (ten == null)
            {
                tenant.TenantEmail = model.TenantToPropertyModel.TenantEmail;
                tenant.StartDate = model.TenantToPropertyModel.StartDate;
                tenant.EndDate = model.TenantToPropertyModel.EndDate;
                tenant.PaymentAmount = model.TenantToPropertyModel.PaymentAmount;
                tenant.PaymentFrequencyId = model.TenantToPropertyModel.PaymentFrequencyId;
                tenant.PropertyId = model.PropId;

                //var TenantEmailResult = SendInvitationEmailToTenant(tenant);

                return Json(new { Success = false, NewPropId = model.PropId, Todo = "Send email", ErrorMsg = "Cannot find person in login table!" });
            }
            else
            {
                var person = AccountService.GetPersonByLoginId(ten.Id);
                var result = PropertyService.AddTenantToProperty(login, person.Id, model.PropId, model.TenantToPropertyModel.StartDate,
                    model.TenantToPropertyModel.EndDate, model.TenantToPropertyModel.PaymentFrequencyId, model.TenantToPropertyModel.PaymentAmount);
                if (result.IsSuccess)
                {
                    return Json(new { Sucess = true, Msg = "Added!", result = "Redirect", url = Url.Action("Index", "PropertyOwners") });

                }
                else
                {
                    return Json(new { Sucess = false, Msg = result.ErrorMessage, redirect = "Redirect", url = Url.Action("Index", "PropertyOwners") });
                }
            }
        }

        [HttpPost]
        public JsonResult UpdateFinance(FinancialModel model)
        {
            var status = true;
            decimal actualTotalRepayment = 0;
            var message = "Record added successfully";
            var data = model;
            // AddTenantToPropertyModel tenant = new AddTenantToPropertyModel();
            //*********** AddNewProperty
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            //var newProp = PropertyOwnerService.AddOnboardProperty(login, model);

            ////*********** AddRepayments

            var newRepayment = new PropertyRepayment();



            newRepayment = PropertyOwnerService.AddOnboardRepayment(login, model.Repayments, model.PropId);
            decimal _totalRepayment = 0;

            int _nosWeeks = 0;
            int _nosFortnights = 0;
            int _nosMonthly = 0;
            if (newRepayment != null)
            {
                foreach (Service.Models.RepaymentViewModel repayment in model.Repayments)
                {

                    switch (repayment.FrequencyType)
                    {
                        case 1: // Weekly
                                // find the nos of weeks in datediff(StartDate, EndDate)
                            _nosWeeks = ((newRepayment.EndDate - newRepayment.StartDate) ?? TimeSpan.Zero).Days / 7;
                            // _totalAmount = nos weeks * amount
                            _totalRepayment = _nosWeeks * newRepayment.Amount;
                            break;
                        case 2:   // Fortnightly
                                  // find the nos of Fortnights in datediff(StartDate, EndDate)
                            _nosFortnights = ((newRepayment.EndDate - newRepayment.StartDate) ?? TimeSpan.Zero).Days / 14;
                            // _totalAmount = nos weeks * amount
                            _totalRepayment = _nosFortnights * newRepayment.Amount;
                            break;
                        case 3: //Monthly
                                // find the nos of Monthls in datediff(StartDate, EndDate)
                            _nosMonthly = ((newRepayment.EndDate - newRepayment.StartDate) ?? TimeSpan.Zero).Days / 30;
                            _totalRepayment = _nosMonthly * newRepayment.Amount;
                            // _totalAmount = nos Monthls * amount
                            break;
                    }

                    actualTotalRepayment += _totalRepayment;


                }

                //*****AddExpenses
                var newExpense = new PropertyExpense();
                newExpense = PropertyOwnerService.AddOnboardExpense(login, model.Expenses, model.PropId);
                //******AddFinancial
                var newFinancial = new PropertyFinance();
                newFinancial = PropertyOwnerService.AddPropertyFinance(login, model, model.PropId, actualTotalRepayment);
            }

            return Json(new { success = status, message = message });

        }

        [HttpGet]
        public ActionResult GetRepayments(int id)
        {

            var data = PropertyService.GetRepayments(id).ToList();
            //var TotalRepaymentsForPeriod = PropertyService.GetTotalIdvRepayment(.Amount, x.StartDate, x.EndDate ?? DateTime.Now, x.FrequencyType);
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult GetExpenses(int id)
        {
            var data = PropertyService.GetExpenses(id).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AddTenantDashBoard(int? propId, string returnUrl)
        {
            var freqs = PropertyOwnerService.GetAllPaymentFrequencies();
            ViewBag.Frequencies = freqs;
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var properties = PropertyService.GetPropertiesAndAddress(login.Id, propId).ToList();
            properties.ForEach(x => x.AddressString = x.Address.ToAddressString());
            var model = new PropDataModel
            {
                ReturnUrl = returnUrl,
                Properties = properties
            };
            ViewBag.ReturnUrl = returnUrl ?? "/Home/Dashboard";
            return View(model);
        }

        [HttpGet]
        public ActionResult EditTenantInProperty(int tenantId, int propId, string returnUrl)
        {
            var freqs = PropertyOwnerService.GetAllPaymentFrequencies();
            var user = User.Identity.Name;
            var id = AccountService.GetLoginByEmail(user).Id;
            var property = PropertyService.GetPropertyById(propId);

            ViewBag.Frequencies = freqs;
            ViewBag.ReturnUrl = returnUrl;

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

                    ViewBag.Property = propertyVm;
                }
            }

            var propertyTenant = PropertyService.GetTenantInProperty(tenantId, propId);

            propertyTenant.ReturnUrl = returnUrl;
            var propertyTenants = propertyTenant;

            return View(propertyTenants);

        }


        protected override void Dispose(bool disposing)
        {
            db?.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Dashboard()
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            var props = PropertyService.GetAllPropertiesByOwner(login.Id);
            var totalProps = props.Count();
            var ownerOccupied = db.OwnerProperty.Where(x => x.OwnerId == login.Id && x.Property.IsActive && (x.Property.IsOwnerOccupied ?? false)).Count();
            var tenantOccupied = db.OwnerProperty.Where(x => x.OwnerId == login.Id && x.Property.IsActive
                                && x.Property.TenantProperty.Where(y => y.IsActive ?? false).Count() > 0).Count();
            var vacant = totalProps - (ownerOccupied + tenantOccupied);

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
                    Occupied = ownerOccupied + tenantOccupied,
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
                    //NewItems = newRequests,
                    Current = newRequests,
                    Accepted = acceptedRequests,
                    Rejected = rejRequests
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
            return View(model);
        }

        //public static bool IsImage(string ext)
        //{
        //    return _validExtensions.Contains(ext);
        //}


        public ActionResult FinanceDetails()
        {

            return null;
        }
    }
}
using KeysPlus.Data;
using KeysPlus.Service.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace KeysPlus.Service.Services
{
    public static class PropertyService
    {
        static string _error = "Something went wrong please try again later!";

        #region Methods

        public static Property GetPropertyById(int id)
        {
            using (var db = new KeysEntities())
            {
                return db.Property.FirstOrDefault(x => x.Id == id && x.IsActive);
            }
        }

        public static PropertyHomeValue GetCurrentHomeValue(int Id)
        {
            using (var db = new KeysEntities())
            {
                var homeValues = db.PropertyHomeValue.Where(h => h.PropertyId == Id);
                var activeHomeValue = homeValues.Where(a => a.IsActive == true).FirstOrDefault();

                return activeHomeValue;

            }

        }

        public static Address GetAddressById(int addId)
        {
            using (var db = new KeysEntities())
            {
                return db.Address.FirstOrDefault(x => x.AddressId == addId && x.IsActive);
            }
        }
        public static IEnumerable<Property> GetAllPropertiesByOwner(int ownerId)
        {
            using (var db = new KeysEntities())
            {
                return db.OwnerProperty.Where(x => x.OwnerId == ownerId && x.Property.IsActive).Select(x => x.Property).ToList();
            }
        }
        public static IEnumerable<PropertyAndAddressModel> GetPropertiesAndAddress(int ownerId, int? propId, bool withTenant = false)
        {
            using (var db = new KeysEntities())
            {
                var props = db.OwnerProperty.Where(x => x.OwnerId == ownerId && x.Property.IsActive);
                if (withTenant)
                {
                    props = props.Where(x => x.Property.TenantProperty.Where(y => y.IsActive ?? true).Count() > 0);
                }
                var result = props.Select(x => new PropertyAndAddressModel
                {
                    Id = x.Property.Id,
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
                    YearBuilt = x.Property.YearBuilt ?? 1990,
                });
                if (propId.HasValue && propId != 0)
                {
                    result = result.Where(x => x.Id == propId);
                }

                return result.ToList();
            }
        }

        public static IEnumerable<RequestTypeModel> GetRequestTypes()
        {
            using (var db = new KeysEntities())
            {
                var result = db.RequestType.Select(x => new RequestTypeModel
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList();
                return result;
            }
        }
        public static ServiceResponseResult AddPropertyToOwner(int ownerId, Property property)
        {
            using (var db = new KeysEntities())
            {
                try
                {
                    db.OwnerProperty.Add(new OwnerProperty { OwnerId = ownerId, Property = property, CreatedOn = DateTime.UtcNow });
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false };
                }
            }
        }

        public static ServiceResponseResult DeletePropertyById(int propertyId)
        {
            var result = new ServiceResponseResult { IsSuccess = false };
            using (var db = new KeysEntities())
            {
                var property = db.Property.FirstOrDefault(x => x.Id == propertyId);
                if (property == null)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Sorry, property not found!";
                    return result;
                }
                property.IsActive = false;
                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return result;
                }
            }
        }

        public static Tenant GetTenantById(int id)
        {
            using (var db = new KeysEntities())
            {
                return db.Tenant.FirstOrDefault(x => x.Id == id);
            }
        }

        public static ServiceResponseResult AddTenant(Tenant tenant)
        {
            using (var db = new KeysEntities())
            {
                if (db.Tenant.Any(x => x.Id == tenant.Id))
                {
                    return new ServiceResponseResult { IsSuccess = true };
                }
                else
                {
                    var newtenant = new Tenant
                    {
                        Id = tenant.Id,
                        CreatedOn = DateTime.Now,
                        Address = new Address
                        {
                            CountryId = 1,
                            IsActive = true,
                        }
                    };
                    try
                    {
                        db.Tenant.Add(newtenant);
                        db.SaveChanges();
                        return new ServiceResponseResult { IsSuccess = true };
                    }
                    catch (Exception)
                    {
                        return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "Something went wrong, please try again later!" };
                    }
                }
            }
        }

        public static ServiceResponseResult ActivateTenant(Login login, int tenantId)
        {
            var result = new ServiceResponseResult { IsSuccess = false };
            using (var db = new KeysEntities())
            {
                var ActivateTenant = db.Person.Where(x => x.Id == tenantId);
                try
                {
                    db.Login.Find(tenantId).IsActive = true;
                    db.Person.Find(tenantId).IsActive = true;
                    db.SaveChanges();
                    result.IsSuccess = true;
                    return result;
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Some thing went wrong, please try again later!";
                    return result;
                }
            }
        }

        public static ServiceResponseResult AddTenantLiabilities(TenantPropertyLiability model)
        {
            var result = new ServiceResponseResult { IsSuccess = false };
            using (var db = new KeysEntities())
            {
                try
                {
                    db.TenantPropertyLiability.Add(model);
                    db.SaveChanges();
                    result.IsSuccess = true;
                    return result;
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Some thing went wrong, please try again later!";
                    return result;
                }
            }
        }

        public static ServiceResponseResult AddTenantToProperty(Login user, int tenantId, int propertyId, DateTime startDate, DateTime? endDate, int? paymentFrequencyId, decimal? paymentAmount)
        {
            var result = new ServiceResponseResult { IsSuccess = false };
            using (var db = new KeysEntities())
            {
                if (user == null)
                {
                    user = db.Login.Where(x => x.Id == tenantId).FirstOrDefault();
                }
                var property = db.Property.FirstOrDefault(x => x.Id == propertyId);
                if (property == null)
                {
                    result.ErrorMessage = "Cannot find property!";
                }
                else
                {
                    var tenant = db.Tenant.FirstOrDefault(x => x.Id == tenantId) ?? new Tenant { Id = tenantId };
                    if (!AddTenant(tenant).IsSuccess)
                    {
                        return new ServiceResponseResult { IsSuccess = false };
                    }
                    else
                    {
                        var existingTenants = db.TenantProperty.Where(x => x.PropertyId == propertyId && (x.TenantId == tenantId && x.IsActive == true)).FirstOrDefault();

                        if (existingTenants != null)
                        {
                            result.ErrorMessage = "This tenant already exist within this property";
                            return result;
                        }
                        else
                        {
                            var tenantProperty = new TenantProperty
                            {
                                PropertyId = propertyId,
                                TenantId = tenantId,
                                StartDate = startDate,
                                EndDate = endDate,
                                PaymentFrequencyId = paymentFrequencyId,
                                PaymentAmount = paymentAmount,
                                CreatedBy = user.UserName,
                                CreatedOn = DateTime.Now,
                                UpdatedBy = user.UserName,
                                UpdatedOn = DateTime.Now

                            };
                            try
                            {
                                db.TenantProperty.Add(tenantProperty);
                                db.SaveChanges();
                                result.IsSuccess = true;
                                return result;
                            }
                            catch (Exception ex)
                            {
                                result.IsSuccess = false;
                                result.ErrorMessage = "Some thing went wrong, please try again later!";
                                return result;
                            }
                        }
                    }
                }
                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "Something went wrong, please try again later!" };
            }
        }

        public static IEnumerable<PropertyType> GetAllProprtyTypes()
        {
            using (var db = new KeysEntities())
            {
                return db.PropertyType.ToList();
            }
        }

        public static IEnumerable<PropertyHomeValueType> GetAllProprtyHomeValueTypes()
        {
            using (var db = new KeysEntities())
            {
                return db.PropertyHomeValueType.ToList();
            }
        }

        public static ServiceResponseResult AddAddress(Address add)
        {
            using (var db = new KeysEntities())
            {
                try
                {
                    db.Address.Add(add);
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false };
                }
            }
        }

        public static int GetRequestsCount(Login login)
        {
            using (var db = new KeysEntities())
            {
                if (login == null)
                {
                    return 0;
                }
                var properties = GetAllPropertiesByOwner(login.Id).Select(x => x.Id).ToList();
                var count = db.PropertyRequest.Where(x => properties.Contains(x.PropertyId) && !x.IsViewed).Count();
                return count;
            }
        }

        public static IEnumerable<PropertyAndRequestsModel> GetAllPropertiesWithRequests(Login user)
        {
            using (var db = new KeysEntities())
            {
                var reqs = db.OwnerProperty.Where(x => x.OwnerId == user.Id).Select(x => x.Property)
                    .Select(x => new { Property = x, Requests = x.PropertyRequest.Where(y => y.ToOwner && y.IsActive) })
                    .Where(x => x.Requests.Count() > 0)
                    .Select(x => new PropertyAndRequestsModel
                    {
                        PropertyId = x.Property.Id,
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
                        PropertyImages = x.Property.PropertyMedia
                                                    .Select(y => new MediaModel
                                                    {
                                                        Id = y.Id,
                                                        NewFileName = y.NewFileName,
                                                        OldFileName = y.OldFileName,
                                                        Status = "load"
                                                    }).ToList(),
                        NewRequestsCount = x.Requests.Where(y => !y.IsViewed).Count(),

                    }).OrderByDescending(x => x.NewRequestsCount).ToList();
                var model = db.OwnerProperty.Where(x => x.OwnerId == user.Id)
                            .Select(x => new PropertyAndRequestsModel
                            {
                                PropertyId = x.Property.Id,
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
                                NewRequestsCount = x.Property.PropertyRequest.Where(y => y.ToOwner && y.IsActive && !y.IsViewed).Count(),

                            }).OrderByDescending(x => x.NewRequestsCount).ToList();

                var allRequests = db.OwnerProperty.Where(x => x.OwnerId == user.Id).Select(x => x.Property)
                            .Select(x => new { Property = x, Requests = x.PropertyRequest.Where(y => y.ToOwner && y.IsActive) })
                            .Where(x => x.Requests.Count() > 0)
                            .Select(x => new PropertyAndRequestsModel
                            {
                                PropertyId = x.Property.Id,
                                PropertyAddress = x.Property.Address.Number + " " + x.Property.Address.Street + ", " + x.Property.Address.Suburb + ", " + x.Property.Address.City + " - " + x.Property.Address.PostCode,
                                TenantName = x.Property.TenantProperty.FirstOrDefault().Tenant.Person.FirstName + " " + x.Property.TenantProperty.FirstOrDefault().Tenant.Person.LastName ?? "",
                                TenantContactNumber = x.Property.TenantProperty.FirstOrDefault().Tenant.Person.Login.PhoneNumber ?? "Not Avaliable",

                                NewRequestsCount = x.Requests.Where(y => !y.IsViewed).Count(),
                                TenantJobRequests = x.Requests.Select(p => new TenantJobRequestModel
                                {
                                    PropertyId = x.Property.Id,
                                    TenantJobRequestId = p.Id,
                                    IsAccepted = p.RequestStatusId == (int)JobRequestStatus.Accepted ? true : false,
                                    RequestType = p.RequestType.Name,
                                    JobDescription = p.RequestMessage,
                                    DateCreated = p.CreatedOn,  //DateTime.UtcNow,
                                    IsViewed = p.IsViewed,
                                    RequestStatus = p.RequestStatus.Name,
                                    MediaFiles = p.PropertyRequestMedia.Select(y => new MediaModel
                                    {
                                        Data = y.NewFileName,
                                        Id = y.Id,
                                        NewFileName = y.NewFileName,
                                        OldFileName = y.OldFileName,
                                        Status = "load"
                                    }).ToList()
                                }).OrderByDescending(a => !a.IsViewed).ToList(),
                            }).OrderByDescending(x => x.NewRequestsCount).ToList();
                return reqs;
            }
        }

        public static PropertyRequest GetPropertyRequestById(int Id)
        {
            using (var db = new KeysEntities())
            {
                return db.PropertyRequest.Where(x => x.Id == Id).FirstOrDefault();
            }
        }
        public static ServiceResponseResult DeleteWatchlistItemById(int watchlistId)
        {
            var result = new ServiceResponseResult { IsSuccess = false };
            using (var db = new KeysEntities())
            {
                var watchlist = db.RentalWatchList.FirstOrDefault(x => x.Id == watchlistId);
                if (watchlist == null)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Sorry, watchlist not found!";
                    return result;
                }
                db.RentalWatchList.Remove(watchlist);
                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return result;
                }
            }
        }
        public static ServiceResponseResult UpdateRequest(RequestModel model, Login login, HttpFileCollectionBase files)
        {
            using (var db = new KeysEntities())
            {
                try
                {
                    var req = db.PropertyRequest.FirstOrDefault( x => x.Id == model.Id);
                    if (req == null) return new ServiceResponseResult() { IsSuccess = false, ErrorMessage = "No Record Found !!" };
                    if (req.RequestStatusId == (int)JobRequestStatus.Accepted) return new ServiceResponseResult() { IsSuccess = false, ErrorMessage ="Job accepted already no more Modifications Allowed !! " };
                    req.RequestMessage = model.RequestMessage;
                    //db.PropertyRequest.Attach(req);
                    //var entry = db.Entry(req);
                    //entry.State = System.Data.Entity.EntityState.Modified;
                    //entry.Property(x => x.IsViewed).IsModified = true;
                    model.FilesRemoved.ForEach(x => {
                        var media = db.PropertyRequestMedia.FirstOrDefault(y => y.Id == x);
                        if (media != null)
                        {
                            db.PropertyRequestMedia.Remove(media);
                            MediaService.RemoveMediaFile(media.NewFileName);
                        }
                    });
                    var mediaFiles = MediaService.SaveFiles(files, 5 - req.PropertyRequestMedia.Count, AllowedFileType.AllFiles).NewObject as List<MediaModel>;
                    if (mediaFiles != null)
                    {
                        mediaFiles.ForEach(x => req.PropertyRequestMedia.Add(new PropertyRequestMedia
                        {
                            NewFileName = x.NewFileName,
                            OldFileName = x.OldFileName,
                        }));
                    }
                    db.SaveChanges();

                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }
        public static ServiceResponseResult EditRequest(RequestModel model, HttpFileCollectionBase files = null)
        {
            using (var db = new KeysEntities())
            {
                try
                {
                    var req = db.PropertyRequest.FirstOrDefault( x => x.Id == model.Id);
                    req.MapFrom<RequestModel>(model);

                    db.PropertyRequest.Attach(req);
                    var entry = db.Entry(req);
                    entry.State = System.Data.Entity.EntityState.Modified;
                    if (model.FilesRemoved != null)
                    {
                        model.FilesRemoved.ToList().ForEach(x =>
                        {
                            var media = req.PropertyRequestMedia.FirstOrDefault(y => y.Id == x);
                            if (media != null)
                            {
                                db.PropertyRequestMedia.Remove(media);
                                MediaService.RemoveMediaFile(media.NewFileName);
                            }
                        });
                    }
                    var mediaFiles = MediaService.SaveFiles(files, 5 - req.PropertyRequestMedia.Count(), AllowedFileType.AllFiles)
                        .NewObject as List<MediaModel>;
                    mediaFiles.ForEach(x => { req.PropertyRequestMedia.Add(new PropertyRequestMedia { NewFileName = x.NewFileName, OldFileName = x.OldFileName}); });
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }
        public static ServiceResponseResult UpdateRequest(PropertyRequest request, Login login = null, HttpFileCollectionBase files = null, string serverPath = null)
        {
            using (var db = new KeysEntities())
            {
                try
                {
                    db.PropertyRequest.Attach(request);
                    var entry = db.Entry(request);
                    entry.State = System.Data.Entity.EntityState.Modified;
                    entry.Property(x => x.IsViewed).IsModified = true;
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }

        public static ServiceResponseResult UpdateInspectionRequest(int inspectionId)
        {
            using (var db = new KeysEntities())
            {
                var inspection = db.Inspection.FirstOrDefault(x => x.Id == inspectionId);
                if (inspection == null)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "Unable to find inspection" };
                }
                inspection.IsViewed = true;
                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = ex.ToString()};
                }
            }
        }

        public static ServiceResponseResult AddPropertyRequest(Login login, RequestModel model, HttpFileCollectionBase files = null)
        {
            using (var db = new KeysEntities())
            {
                if(model.ToOwner == true) { //Tenant request to property owner
                    if (!TenantService.IsTenantInProperty(login, model.PropertyId))
                    {
                        return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "You are not a tenant of this property" };
                    }
                }
                var propertyRequest = new PropertyRequest
                {
                    RecipientId = model.RecipientId,
                    RequestTypeId = model.RequestTypeId,
                    PropertyId = model.PropertyId,
                    ToOwner = model.ToOwner,
                    ToTenant = model.ToTenant,
                    RequestMessage = model.RequestMessage,
                    RequestStatusId = 1,
                    CreatedBy = login.Id,
                    UpdatedBy = login.Id,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    IsActive = true
                };
                if (model.RequestTypeId == 3) propertyRequest.DueDate = model.DueDate;
                db.PropertyRequest.Add(propertyRequest);
                var mediaResult = MediaService.SaveFiles(files, 5, AllowedFileType.AllFiles).NewObject as List<MediaModel>;
                mediaResult.ForEach(x =>
                {
                    propertyRequest.PropertyRequestMedia.Add(new PropertyRequestMedia
                    {
                        NewFileName = x.NewFileName,
                        OldFileName = x.OldFileName,
                    });
                });
                try
                {
                    db.SaveChanges();
                    return   new ServiceResponseResult { IsSuccess = true };

                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };

                }
            }
        }

        public static ServiceResponseResult AcceptApplication(AcceptAndDeclineRentalApplicationModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var tenant = AccountService.GetLoginById(model.TenantId);
                var isTenant = TenantService.IsLoginATenant(tenant);
                if (!isTenant) return new ServiceResponseResult { IsSuccess = false };
                var isOwnerProp = db.OwnerProperty.FirstOrDefault(x => x.OwnerId == login.Id && x.PropertyId == model.PropertyId) == null ? false : true;
                if (!isOwnerProp) return new ServiceResponseResult { IsSuccess = false };
                var app = db.RentalApplication.FirstOrDefault(x => x.Id == model.ApplicationId);
                if (app == null) return new ServiceResponseResult { IsSuccess = false };
                var allOtherRentalApp = db.RentalApplication.Where(x => x.RentalListing.PropertyId == /*model.PropertyId*/ model.RentalListingId && x.Id != model.ApplicationId);
                allOtherRentalApp.ToList().ForEach(x => x.ApplicationStatusId = (int)Models.RentalApplicationStatus.Declined);

                app.ApplicationStatusId = (int)Models.RentalApplicationStatus.Accepted;
                var amount= app.RentalListing?.TargetRent;
                var newTenantProp = new TenantProperty
                {
                    TenantId = model.TenantId,
                    PropertyId = model.PropertyId,
                    StartDate = DateTime.UtcNow,
                    UpdatedBy = login.Email,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = login.Email,
                    UpdatedOn = DateTime.UtcNow,
                    IsActive = true,
                    PaymentAmount = amount
                };
                db.TenantProperty.Add(newTenantProp);
                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }

        public static ServiceResponseResult DeclineApplication(AcceptAndDeclineRentalApplicationModel model, Login login)
        {
            using (var db = new KeysEntities())
            {

                var isOwnerProp = db.OwnerProperty.FirstOrDefault(x => x.OwnerId == login.Id && x.PropertyId == model.PropertyId) == null ? false : true;
                if (!isOwnerProp) return new ServiceResponseResult { IsSuccess = false };
                var app = db.RentalApplication.FirstOrDefault(x => x.Id == model.ApplicationId);
                if (app == null) return new ServiceResponseResult { IsSuccess = false };
                app.ApplicationStatusId = (int)Models.RentalApplicationStatus.Declined;

                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }

        public static ServiceResponseResult RemoveTenantProperty(Login login, int tenantPropertyId)
        {
            using (var db = new KeysEntities())
            {
                var foundTenant = db.TenantProperty.Where(x => x.Id == tenantPropertyId).FirstOrDefault();
                foundTenant.IsActive = false;

                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }

        public static SearchResult GetRentalListings(PORentalListingSearchModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var allProps = db.OwnerProperty.Where(x => x.OwnerId == login.Id && x.Property.IsActive).Select(x => x.PropertyId);
                var listings = db.RentalListing.Where(x => allProps.Contains(x.PropertyId) && x.IsActive);
                var data = listings.Select(x => new RentListingViewModel
                {
                    Model = new RentListingModel
                    {
                        Id = x.Id,
                        PropertyId = x.PropertyId,
                        Title = x.Title,
                        Description = x.Description.Trim(),
                        MovingCost = x.MovingCost ?? 0,
                        TargetRent = x.TargetRent,
                        AvailableDate = x.AvailableDate,
                        Furnishing = x.Furnishing,
                        IdealTenant = x.IdealTenant,
                        OccupantCount = x.OccupantCount ?? 0,
                        PetsAllowed = x.PetsAllowed,
                        MediaFiles = x.RentalListingMedia.Select(y => new MediaModel
                        {
                            Id = y.Id,
                            Status = "load",
                            NewFileName = y.NewFileName,
                            OldFileName = y.OldFileName
                        }).ToList(),
                    },
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
                    RentalPaymentType = x.Property.TargetRentType.Name,
                    NewApplicationsCount = x.RentalApplication.Where(y => y.RentalListingId == x.Id && y.IsActive && y.ApplicationStatusId == 1 && !(y.IsViewedByOwner ?? false)).Count(),
                    Latitude = x.Property.Address.Lat,
                    Longitude = x.Property.Address.Lng,
                });

                var allItems = data.OrderByDescending(x => x.NewApplicationsCount).ToPagedList(model.Page, 10);
                allItems.ToList().ForEach(x => x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties()));
                switch (model.SortOrder)
                {
                    case "New Application(Desc)":
                        data = data.OrderByDescending(s => s.NewApplicationsCount);
                        break;
                    case "New Application":
                        data = data.OrderBy(s => s.NewApplicationsCount);
                        break;
                    case "TitLe":
                        data = data.OrderBy(s => s.Model.Title);
                        break;
                    case "TitLe(Desc)":
                        data = data.OrderByDescending(s => s.Model.Title);
                        break;
                    case "Earliest Date":
                        data = data.OrderBy(s => s.CreatedOn);
                        break;
                    case "Latest Date":
                        data = data.OrderByDescending(s => s.CreatedOn);
                        break;
                    default:
                        data = data.OrderByDescending(s => s.NewApplicationsCount);
                        break;
                }
                if (!String.IsNullOrWhiteSpace(model.SearchString))
                {
                    SearchUtil searchTool = new SearchUtil();
                    int searchType = searchTool.CheckDisplayType(model.SearchString);
                    string formatString = searchTool.ConvertString(model.SearchString);
                    switch (searchType)
                    {
                        case 1:

                            data = data.Where(x => x.PropertyAddress == formatString
                                           || x.PropertyAddress.ToLower().EndsWith(formatString)
                                           || (x.Model.Title ?? "").ToLower().EndsWith(formatString));
                            break;
                        case 2:
                            data = data.Where(x => x.PropertyAddress == formatString
                                         || x.PropertyAddress.ToLower().StartsWith(formatString)
                                         || (x.Model.Title ?? "").ToLower().StartsWith(formatString));
                            break;
                        case 3:
                            data = data.Where(x => x.PropertyAddress == formatString
                                          || x.PropertyAddress.ToLower().Contains(formatString)
                                          || (x.Model.Title ?? "").ToLower().Contains(formatString));
                            break;
                    }
                };
                var items = data.ToPagedList(model.Page, 10);
                items.ToList().ForEach(x => x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties()));
                var count = items.Count;
                var result = new SearchResult { SearchCount = items.Count, Items = count == 0 ? allItems : items };
                return result;
            }
        }

        public static int GetNewApplicationCount(Login login)
        {
            using (var db = new KeysEntities())
            {
                if (login == null)
                {
                    return 0;
                }
                var PropIds = db.OwnerProperty.Where(x => x.OwnerId == login.Id && x.Property.IsActive).Select(x => x.Property.Id).ToList();
                var Applications = new List<RentalApplication>();
                foreach (var propid in PropIds)
                {
                    var application = db.RentalApplication.Where(x => x.RentalListing.PropertyId == propid && x.IsActive && x.ApplicationStatusId == (int)Models.RentalApplicationStatus.Applied && !(x.IsViewedByOwner ?? false)).FirstOrDefault();
                    if (application != null) { Applications.Add(application); }
                }
                var count = Applications.Count();
                return count;
            }
        }


        public static SearchResult GetRentalApplications(PORentAppsSearchModelModel model)
        {
            using (var db = new KeysEntities())
            {
                var data = db.RentalApplication.Where(x => x.RentalListing.Id == model.RentalListingId && x.IsActive && x.ApplicationStatusId == (int)Models.RentalApplicationStatus.Applied)
                            .Select(x => new //RentalApplicationsViewModel
                            {
                                Model = new RentalApplicationModel
                                {
                                    Id = x.Id,
                                    PropertyId = x.RentalListing.PropertyId,
                                    RentalListingId = x.RentalListingId,
                                    PersonId = x.PersonId,
                                    TenantsCount = x.TenantsCount,
                                    Note = x.Note,
                                    IsViewedByOwner = x.IsViewedByOwner ?? false,
                                    MediaFiles = db.RentalApplicationMedia.Where(a => a.RentalApplicationId == x.Id).Select(a => new MediaModel
                                    {
                                        Id = a.Id,
                                        OldFileName = a.OldFileName,
                                        NewFileName = a.NewFileName,
                                    }).ToList(),
                                },

                                PropertyName = x.RentalListing.Property.Name,
                                TenantName = (x.Person.FirstName ?? "") + " " + (x.Person.LastName ?? ""),
                                TenantContactNumber = x.Person.Tenant.MobilePhoneNumber,
                                CreatedOn = x.CreatedOn,
                                Address = (x.RentalListing.Property.Address.Number.Replace(" ", "")) + " " +
                                                (x.RentalListing.Property.Address.Street.Trim()) + " " +
                                                (x.RentalListing.Property.Address.Suburb.Trim()) + " " +
                                                (x.RentalListing.Property.Address.City.Trim()) + " " +
                                                (x.RentalListing.Property.Address.PostCode.Replace(" ", ""))
                            });

                var allItems = data.OrderByDescending(x => x.CreatedOn).ToPagedList(model.Page, 10);
                switch (model.SortOrder)
                {
                    case "Lowest Tenants Count":
                        data = data.OrderBy(s => s.Model.TenantsCount);
                        break;
                    case "Highest Tenants Count":
                        data = data.OrderByDescending(s => s.Model.TenantsCount);
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
                    SearchUtil searchTool = new SearchUtil();
                    int searchType = searchTool.CheckDisplayType(model.SearchString);
                    string formatString = searchTool.ConvertString(model.SearchString);
                    switch (searchType)
                    {
                        case 1:
                            data = data.Where(x => x.TenantName.ToString() == formatString
                                            || x.TenantName.ToLower().EndsWith(formatString));
                            break;
                        case 2:
                            data = data.Where(x => x.TenantName.ToString() == formatString
                                         || x.TenantName.ToLower().StartsWith(formatString));
                            break;
                        case 3:
                            data = data.Where(x => x.TenantName.ToString() == formatString
                                          || x.TenantName.ToLower().Contains(formatString));
                            break;
                    }
                }
                var items = data.ToPagedList(model.Page, 10);
                var count = items.Count;
                items = count == 0 ? allItems : items;
                items.ToList().ForEach(x => x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties()));
                var searchResult = new SearchResult { SearchCount = items.Count, Items = count == 0 ? allItems : items };
                return searchResult;
            }
        }
        public static SearchResult GetTenantsByProperty(POMyTenantSearchModel model, int loginId)
        {
            using (var db = new KeysEntities())
            {
                var propr = new List<int>();
                propr = model.PropertyIds;
                var allProps = db.OwnerProperty.Where(x => x.OwnerId == loginId && x.Property.IsActive).Select(x => x.PropertyId);
                var tenantsProperty = db.TenantProperty.Where(x => allProps.Contains(x.PropertyId));

                var data = tenantsProperty.Where(x => x.IsActive == true).Select(
                         x => new PropertyTenantsModel
                         {
                             Id = x.Id,
                             TenantId = x.TenantId,
                             TenantName = x.Tenant.Person.FirstName + " " + x.Tenant.Person.LastName,
                             TenantEmail = x.Tenant.Person.Login.Email,
                             TenantPhone = x.Tenant.Person.Login.PhoneNumber,
                             ProfilePicture = x.Tenant.Person.ProfilePhoto,
                             StartDate = x.StartDate,
                             EndDate = x.EndDate,
                             RentAmount = x.PaymentAmount,
                             RentFrequency = x.TenantPaymentFrequencies.Code,
                             PropertyId = x.PropertyId,
                             PropertyAddress = (x.Property.Address.Number.Replace(" ", "")) + " " +
                                                           (x.Property.Address.Street.Trim()) + " " +
                                                           (x.Property.Address.Suburb.Trim()) + " " +
                                                           (x.Property.Address.City.Trim()) + " " +
                                                           (x.Property.Address.PostCode.Replace(" ", "")),
                             StreetAddress = (x.Property.Address.Number.Replace(" ", "")) + " " + (x.Property.Address.Street.Trim()),
                             CitySub = (x.Property.Address.Suburb.Trim()) + " " + (x.Property.Address.City.Trim())

                         }).ToList();


                var allItems = data.OrderByDescending(x => x.StartDate).ToPagedList(model.Page, 10);

                switch (model.SortOrder)
                {
                    case "Start Date":
                        data = data.OrderBy(s => s.StartDate).ToList();
                        break;
                    case "End Date":
                        data = data.OrderByDescending(s => s.EndDate).ToList();
                        break;
                    case "TenantName":
                        data = data.OrderBy(s => s.TenantName).ToList();
                        break;
                    case "TenantName(Desc)":
                        data = data.OrderByDescending(s => s.TenantName).ToList();
                        break;

                    default:
                        data = data.OrderByDescending(s => s.StartDate).ToList();
                        break;
                }
                if (!String.IsNullOrWhiteSpace(model.SearchString))
                {
                    SearchUtil searchTool = new SearchUtil();
                    int searchType = searchTool.CheckDisplayType(model.SearchString);
                    string formatString = searchTool.ConvertString(model.SearchString);
                    switch (searchType)
                    {
                        case 1:

                            data = data.Where(x => x.PropertyAddress == formatString
                                           || x.PropertyAddress.ToLower().EndsWith(formatString)
                                           || (x.TenantName ?? "").ToLower().EndsWith(formatString)).ToList();
                            break;
                        case 2:
                            data = data.Where(x => x.PropertyAddress == formatString
                                         || x.PropertyAddress.ToLower().StartsWith(formatString)
                                         || (x.TenantName ?? "").ToLower().StartsWith(formatString)).ToList();
                            break;
                        case 3:
                            data = data.Where(x => x.PropertyAddress == formatString
                                          || x.PropertyAddress.ToLower().Contains(formatString)
                                          || (x.TenantName ?? "").ToLower().Contains(formatString)).ToList();
                            break;
                    }
                };

                var items = data.ToPagedList(model.Page, 10);
                var count = items.Count;
                var result = new SearchResult { SearchCount = items.Count, Items = (count == 0) ? allItems : items };
                return result;
            }
        }

        public static IEnumerable<TenantJobRequestModel> GetRequestByProperty(int propId)
        {
            using (var db = new KeysEntities())
            {
                return db.PropertyRequest.Where(x => x.PropertyId == propId)
                    .Select(x => new TenantJobRequestModel
                    {
                        PropertyId = x.PropertyId,
                        TenantJobRequestId = x.Id,
                        RequestType = x.RequestType.Name,
                        //IsAccepted = x.RequestStatusId == (int)JobRequestStatus.Accepted ? true : false,
                        JobDescription = x.RequestMessage,
                        IsViewed = x.IsViewed,
                        RequestStatus = x.RequestStatus.Name,
                        DateCreated = x.CreatedOn,
                        PersonId = x.CreatedBy,
                        MediaFiles = x.PropertyRequestMedia.Select(y => new MediaModel { }).ToList()
                    }).OrderByDescending(x => !x.IsViewed).ToList();
            }
        }

        public static IEnumerable<PropertyTenantsModel> GetTenantListByPropertyId(int propertyId)
        {
            using (var db = new KeysEntities())
            {
                var propr = propertyId;
                var tenantList = db.TenantProperty.Where(x => x.PropertyId == propertyId && (x.IsActive ?? false)).Select(
                    x => new PropertyTenantsModel
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,
                        TenantName = x.Tenant.Person.FirstName + " " + x.Tenant.Person.LastName,
                        TenantEmail = x.Tenant.Person.Login.Email,
                        TenantPhone = x.Tenant.Person.Login.PhoneNumber,
                        ProfilePicture = x.Tenant.Person.ProfilePhoto,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        RentAmount = x.PaymentAmount,
                        RentFrequency = x.TenantPaymentFrequencies.Code,
                        PropertyId = x.PropertyId,
                        PropertyAddress = (x.Property.Address.Number.Replace(" ", "")) + " " +
                                                      (x.Property.Address.Street.Trim()) + " " +
                                                      (x.Property.Address.Suburb.Trim()) + " " +
                                                      (x.Property.Address.City.Trim()) + " " +
                                                      (x.Property.Address.PostCode.Replace(" ", "")),
                        StreetAddress = (x.Property.Address.Number.Replace(" ", "")) + " " + (x.Property.Address.Street.Trim()),
                        CitySub = (x.Property.Address.Suburb.Trim()) + " " + (x.Property.Address.City.Trim()),




                    }).ToList();
                return tenantList;
            }
        }
        public static AddTenantToPropertyModel GetTenantInProperty(int tenantId, int propertyId)
        {
            using (var db = new KeysEntities())
            {
                var tenantInProperty = db.TenantProperty.FirstOrDefault(x => x.PropertyId == propertyId && x.TenantId == tenantId && x.IsActive != false);

                var model = new AddTenantToPropertyModel
                {
                    Id = tenantInProperty.Id,
                    FirstName = tenantInProperty.Tenant.Person.FirstName,
                    LastName = tenantInProperty.Tenant.Person.LastName,
                    TenantEmail = tenantInProperty.Tenant.Person.Login.Email,
                    StartDate = tenantInProperty.StartDate,
                    EndDate = tenantInProperty.EndDate,
                    PaymentAmount = tenantInProperty.PaymentAmount,
                    PaymentFrequencyId = tenantInProperty.PaymentFrequencyId,
                    PropertyId = tenantInProperty.PropertyId,
                    PaymentStartDate = tenantInProperty.PaymentStartDate,
                    PaymentDueDate = tenantInProperty.PaymentDueDate ?? 1,
                    IsMainTenant = tenantInProperty.IsMainTenant ?? false,
                    YearBuilt = tenantInProperty.Property.YearBuilt,
                    Liabilities = db.TenantPropertyLiability.Where(x => x.TenantPropertyId == tenantInProperty.Id).Select(x => new LiabilityModel
                    {
                        Id = x.Id,
                        TenantPropertyId = x.TenantPropertyId,
                        Name = x.LiabilityName,
                        Status = "Load",
                        Amount = x.Amount
                    }).ToList()
                };

                return model;
            }
        }

        public static IEnumerable<ViewRequestReplyModel> GetRequestReplies(int? requestId)
        {
            using (var db = new KeysEntities())
            {
                var data = db.RequestReply.Where(x => x.RequestId == requestId).Select(x => new ViewRequestReplyModel
                {
                    RequestId = x.RequestId,
                    RequestReply = x.Message,
                    CreatedBy = x.CreatedBy,
                    CreatedOn = x.CreatedOn,
                    TenantName = x.PropertyRequest.Property.TenantProperty.Where(y => y.TenantId == x.CreatedBy).Select(a => a.Tenant.Person.FirstName + " " + a.Tenant.Person.LastName).FirstOrDefault(),
                    TenantPhoneNumber = db.Login.Where(person => person.Id == x.CreatedBy).FirstOrDefault().PhoneNumber.ToString(),
                    MediaFiles = x.RequestReplyMedia.Select(y => new MediaModel
                    {
                        Id = y.Id,
                        NewFileName = y.NewFileName,
                        OldFileName = y.OldFileName,
                        Status = "load",

                    }).ToList(),
                }).ToList();

                return data;
            }

        }
        public static Address GetAddressByRentalListing(int rentalListingId)
        {
            using (var db = new KeysEntities())
            {
                var listing = db.RentalListing.FirstOrDefault(x => x.Id == rentalListingId && x.IsActive);
                return listing?.Property.Address;
            }
        }
        public static Address GetAddressByPropId(int propId)
        {
            using (var db = new KeysEntities())
            {
                var prop = db.Property.Where(x => x.Id == propId && x.IsActive);
                return prop.FirstOrDefault()?.Address;
            }
        }

        public static IEnumerable<HomeValueViewModel> GetHomeValues(int propId)
        {
            using (var db = new KeysEntities())
            {
                return db.PropertyHomeValue.Where(x => x.PropertyId == propId)
                    .Select(x => new HomeValueViewModel
                    {
                        Id = x.Id,
                        PropertyId = propId,
                        Value = x.Value,
                        TypeId = x.HomeValueTypeId,
                        ValueType = x.PropertyHomeValueType.HomeValueType,
                        Date = x.Date,
                        IsActive = x.IsActive ?? false,
                    }).ToList();
            }
        }
        public static IEnumerable<ExpenseViewModel> GetExpenses(int propId)
        {
            using (var db = new KeysEntities())
            {
                var propertyExpense = db.PropertyExpense.Where(x => x.PropertyId == propId).Select(x => new ExpenseViewModel
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    Description = x.Description,
                    ExpenseDate = x.Date
                }).ToList();
                return propertyExpense;
            }
        }

        public static IEnumerable<RepaymentViewModel> GetRepayments(int propId)
        {
            using (var db = new KeysEntities())
            {
                var propertyRepayment = db.PropertyRepayment.Where(x => x.PropertyId == propId).Select(x => new RepaymentViewModel
                {
                    Id = x.Id,
                    PropertyId = propId,
                    Amount = x.Amount,
                    FrequencyType = x.FrequencyType,
                    FrequencyName = x.TargetRentType.Name,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    //  TotalRepaymentsForPeriod = GetTotalIdvRepayment(x.Amount, x.StartDate, x.EndDate?? DateTime.Now, x.FrequencyType)

                }).ToList();
                foreach (var x in propertyRepayment)
                {
                    x.TotalRepaymentsForPeriod = GetTotalIdvRepayment(x.Amount, x.StartDate, x.EndDate ?? DateTime.Now, x.FrequencyType);
                }
                foreach (var x in propertyRepayment)
                {
                    x.IsActive = IsRepaymentActive(x.EndDate, x.StartDate);
                }
                return propertyRepayment;
            }
        }
        public static IEnumerable<RentalPaymentViewModel> GetRentalPayment(int propId)
        {
            using (var db = new KeysEntities())
            {
                return db.PropertyRentalPayment.Where(x => x.PropertyId == propId)
                    .Select(x => new RentalPaymentViewModel
                    {
                        Id = x.Id,
                        PropertyId = x.PropertyId,
                        Amount = x.Amount,
                        FrequencyTypeId = x.FrequencyType,
                        FrequencyType = x.TargetRentType.Name,
                        Date = x.Date
                    }).ToList();
            }
        }
        public static string IsRepaymentActive(DateTime? endDate, DateTime startDate)
        {
            string isActive = "No";
            if (!endDate.HasValue && startDate <= DateTime.Now || endDate > DateTime.Now && startDate <= DateTime.Now)
            {
                return isActive = "Yes";
            }
            return isActive;
        }

        public static decimal GetTotalIdvRepayment(decimal amount, DateTime startDate, DateTime? endDate, int frequencyType)
        {
            int _nosWeeks = 0;
            int _nosFortnights = 0;
            int _nosMonthly = 0;
            decimal _totalRepayment = 0;
            switch (frequencyType)
            {
                case 1: // Weekly
                        // find the nos of weeks in datediff(StartDate, EndDate)
                    _nosWeeks = ((endDate - startDate) ?? TimeSpan.Zero).Days / 7;
                    // _totalAmount = nos weeks * amount
                    _totalRepayment = _nosWeeks * amount;
                    break;
                case 2:   // Fortnightly
                          // find the nos of Fortnights in datediff(StartDate, EndDate)
                    _nosFortnights = ((endDate - startDate) ?? TimeSpan.Zero).Days / 14;
                    // _totalAmount = nos weeks * amount
                    _totalRepayment = _nosFortnights * amount;
                    break;
                case 3: //Monthly
                        // find the nos of Monthls in datediff(StartDate, EndDate)
                    _nosMonthly = ((endDate - startDate) ?? TimeSpan.Zero).Days / 30;
                    _totalRepayment = _nosMonthly * amount;
                    // _totalAmount = nos Monthls * amount
                    break;
            }
            return _totalRepayment;
        }

        public static ServiceResponseResult AddTenant(Login user, int tenantId, AddTenantToPropertyModel model)
        {
            var result = new ServiceResponseResult { IsSuccess = false };
            using (var db = new KeysEntities())
            {
                if (user == null)
                {
                    user = db.Login.Where(x => x.Id == tenantId).FirstOrDefault();
                }
                var property = db.Property.FirstOrDefault(x => x.Id == model.PropertyId);
                if (property == null)
                {
                    result.ErrorMessage = "Cannot find property!";
                }
                else
                {
                    var tenant = db.Tenant.FirstOrDefault(x => x.Id == tenantId) ?? new Tenant { Id = tenantId };
                    if (!AddTenant(tenant).IsSuccess)
                    {
                        return new ServiceResponseResult { IsSuccess = false };
                    }
                    else
                    {
                        var existingTenants = db.TenantProperty.Where(x => x.PropertyId == model.PropertyId && (x.TenantId == tenantId && x.IsActive == true)).FirstOrDefault();

                        if (existingTenants != null)
                        {
                            result.ErrorMessage = "This tenant already exist within this property";
                            return result;
                        }
                        else
                        {
                            var tenantProperty = new TenantProperty
                            {
                                PropertyId = model.PropertyId,
                                TenantId = tenantId,
                                StartDate = model.StartDate,
                                EndDate = model.EndDate,
                                PaymentFrequencyId = model.PaymentFrequencyId,
                                PaymentAmount = model.PaymentAmount,
                                PaymentStartDate = model.PaymentStartDate,
                                PaymentDueDate = model.PaymentDueDate,
                                IsMainTenant = model.IsMainTenant,
                                IsActive = true,
                                CreatedBy = user.UserName,
                                CreatedOn = DateTime.Now,
                                UpdatedBy = user.UserName,
                                UpdatedOn = DateTime.Now,

                            };
                            db.TenantProperty.Add(tenantProperty);

                            if (model.Liabilities != null)
                            {
                                model.Liabilities.ForEach(x =>
                                {
                                    db.TenantPropertyLiability.Add(new TenantPropertyLiability
                                    {
                                        LiabilityName = x.Name,
                                        Amount = x.Amount,
                                        TenantProperty = tenantProperty
                                    });
                                });
                            }
                            try
                            {
                                db.SaveChanges();
                                result.IsSuccess = true;
                                return result;
                            }
                            catch (Exception ex)
                            {
                                result.IsSuccess = false;
                                result.ErrorMessage = ex.StackTrace;
                                return result;
                            }
                        }
                    }
                }
                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "Something went wrong, please try again later!" };
            }
        }
        public static IEnumerable<TenantPropertyViewModel> GetTenantsByPropId(int propId)
        {
            var output = new List<TenantPropertyViewModel>();
            using (var db = new KeysEntities())
            {
                var tenantProperties = db.TenantProperty.Where(tp => tp.PropertyId == propId);
                foreach (var tp in tenantProperties)
                {
                    output.Add(new TenantPropertyViewModel
                    {
                        PropertyId = tp.PropertyId,
                        TenantId = tp.TenantId,
                        TenantName = db.Person.Find(tp.TenantId).FirstName + " " + db.Person.Find(tp.TenantId).LastName
                    });
                };
            }
            return output;
        }


    }

    #endregion
}
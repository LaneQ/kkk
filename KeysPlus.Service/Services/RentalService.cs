using KeysPlus.Data;
using KeysPlus.Service.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace KeysPlus.Service.Services
{
    public static class RentalService
    {
        static string _error = "Something went wrong please try again later!";
        public static IEnumerable<RentalListing> GetRentalProperties(string searchString, string sortOrder)
        {
            using (var db = new KeysEntities())
            {
                return db.RentalListing.Include("Property").Include("RentalListingMedia").Include("Property.Address").Include("Property.PropertyType").Include("Property.TargetRentType").Include("Property.PropertyMedia").Where(x => x.IsActive == true).ToList();
            }
        }

        public static string GetWatchListStatus(int Id, int userId)
        {
            using (var db = new KeysEntities())
            {
                var data = db.RentalWatchList.Where(m => m.PersonId == userId && m.RentalListingId == Id && m.IsActive == true).FirstOrDefault();
                if (data != null)
                {
                    return "Watching";
                }
            }
            return "WatchList";
        }

        public static SearchResult GetAllRentalProperties(RentalListingSearchModel model)
        {
            using (var db = new KeysEntities())
            {
                var data = db.RentalListing.Where(x => x.IsActive == true)
                    .Select(x => new RentListingViewModel
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
                        IsOwner = db.OwnerProperty.FirstOrDefault(y => y.PropertyId == x.PropertyId).OwnerId == model.UserId,
                        IsApplied = db.RentalApplication.Any(y=> y.RentalListingId == x.Id && y.PersonId == model.UserId),
                        Bedrooms = x.Property.Bedroom ?? 0,
                        Bathrooms = x.Property.Bathroom ?? 0,
                        ParkingSpaces = x.Property.ParkingSpace ?? 0,
                        LandSqm = x.Property.LandSqm ?? 0,
                        FloorArea = x.Property.FloorArea ?? 0,
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
                            Latitude = x.Property.Address.Lat,
                            Longitude = x.Property.Address.Lng,
                        },
                        PropertyAddress = (x.Property.Address.Number.Replace(" ", "")) + " " +
                                          (x.Property.Address.Street.Trim() ?? "") + " " +
                                          (x.Property.Address.Suburb.Trim() ?? "") + " " +
                                          (x.Property.Address.City.Trim() ?? "") + "-" +
                                          (x.Property.Address.PostCode.Replace(" ", "")),
                        PropertyType = x.Property.PropertyType.Name,
                        RentalPaymentType = x.Property.TargetRentType.Name,
                        Latitude = x.Property.Address.Lat,
                        Longitude = x.Property.Address.Lng,
                    });

                var allItems = data.OrderByDescending(x => x.CreatedOn).ToPagedList(model.Page, 10);
                switch (model.SortOrder)
                {
                    case "Title":
                        data = data.OrderBy(s => s.Model.Title);
                        break;
                    case "Title(Desc)":
                        data = data.OrderByDescending(s => s.Model.Title);
                        break;
                    case "Highest Rent":
                        data = data.OrderByDescending(s => s.Model.TargetRent);
                        break;
                    case "Lowest Rent":
                        data = data.OrderBy(s => s.Model.TargetRent);
                        break;
                    case "Latest Available Date":
                        data = data.OrderByDescending(s => s.Model.AvailableDate);
                        break;
                    case "Earliest Available Date":
                        data = data.OrderBy(s => s.Model.AvailableDate);
                        break;
                    case "Earliest Listing":
                        data = data.OrderBy(s => s.CreatedOn);
                        break;
                    case "Latest Listing":
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
                            data = data.AsEnumerable().Where(r => r.Model.Title.ToLower().EndsWith(formatString)
                                      || r.PropertyAddress.ToLower().EndsWith(formatString)
                                      || ("$" + r.Model.TargetRent.ToString()).EndsWith(formatString)
                                      || (r.Model.AvailableDate != null && r.Model.AvailableDate.Value.ToString("MMMM d yyyy").ToLower().EndsWith(formatString.Replace(",", "")))
                                      || (r.Model.AvailableDate != null && r.Model.AvailableDate.Value.ToString("d MMMM yyyy").ToLower().EndsWith(formatString.Replace(",", "")))
                                      || (r.Model.AvailableDate != null && r.Model.AvailableDate.Value.ToString("MMM d yyyy").ToLower().EndsWith(formatString.Replace(",", "")))
                                      || (r.Model.AvailableDate != null && r.Model.AvailableDate.Value.ToString("d MMM yyyy").ToLower().EndsWith(formatString.Replace(",", "")))).AsQueryable();
                            break;
                        case 2:
                            data = data.AsEnumerable().Where(r => r.Model.Title.ToLower().StartsWith(formatString)
                                      || r.PropertyAddress.ToLower().StartsWith(formatString)
                                      || ("$" + r.Model.TargetRent.ToString()).StartsWith(formatString)
                                      || (r.Model.AvailableDate != null && r.Model.AvailableDate.Value.ToString("MMMM d yyyy").ToLower().StartsWith(formatString.Replace(",", "")))
                                      || (r.Model.AvailableDate != null && r.Model.AvailableDate.Value.ToString("d MMMM yyyy").ToLower().StartsWith(formatString.Replace(",", "")))
                                      || (r.Model.AvailableDate != null && r.Model.AvailableDate.Value.ToString("MMM d yyyy").ToLower().StartsWith(formatString.Replace(",", "")))
                                      || (r.Model.AvailableDate != null && r.Model.AvailableDate.Value.ToString("d MMM yyyy").ToLower().StartsWith(formatString.Replace(",", "")))).AsQueryable();
                            break;
                        case 3:
                            data = data.AsEnumerable().Where(r => r.Model.Title.ToLower().Contains(formatString)
                                      || r.PropertyAddress.ToLower().Contains(formatString)
                                      || ("$" + r.Model.TargetRent.ToString()).Contains(formatString)
                                      || (r.Model.AvailableDate != null && r.Model.AvailableDate.Value.ToString("MMMM d yyyy").ToLower().Contains(formatString.Replace(",", "")))
                                      || (r.Model.AvailableDate != null && r.Model.AvailableDate.Value.ToString("d MMMM yyyy").ToLower().Contains(formatString.Replace(",", "")))
                                      || (r.Model.AvailableDate != null && r.Model.AvailableDate.Value.ToString("MMM d yyyy").ToLower().Contains(formatString.Replace(",", "")))
                                      || (r.Model.AvailableDate != null && r.Model.AvailableDate.Value.ToString("d MMM yyyy").ToLower().Contains(formatString.Replace(",", "")))).AsQueryable();
                            break;
                    }
                }
                var items = data.ToPagedList(model.Page, 10);
                var count = items.Count;
                items = count == 0 ? allItems : items;
                items.ToList().ForEach(x => {
                    x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties());
                    x.IsInWatchlist = db.RentalWatchList.FirstOrDefault( y => y.PersonId == model.UserId && y.RentalListingId == x.Model.Id && y.IsActive) != null;
                });
               
                var result = new SearchResult { SearchCount = items.Count, Items = count == 0 ? allItems : items };
                return result;
            }
        }

        public static ServiceResponseResult AddTenantJobRequest(MarketJobModel model, Login login, HttpFileCollectionBase files = null)
        {
            using (var db = new KeysEntities())
            {
                try
                {
                    var request = db.PropertyRequest.Where(x => x.Id == model.RequestId).FirstOrDefault();
                    if (request == null) return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "No record found!" };
                    request.RequestStatusId = (int)JobRequestStatus.Accepted;
                    request.IsUpdated = true;
                    var jobRequest = new TenantJobRequest
                    {
                        PropertyId = model.PropertyId,
                        OwnerId = login.Id,
                        JobDescription = model.JobDescription,
                        JobStatusId = 1,  //Bug #2031 (Part 3)
                        MaxBudget = model.MaxBudget,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = login.Email,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedBy = login.Email,
                        Title = model.Title
                    };
                    db.TenantJobRequest.Add(jobRequest);
                    var mediaFiles = MediaService.SaveFiles(files, 5, AllowedFileType.Images).NewObject as List<MediaModel>;
                    mediaFiles.ForEach(x => jobRequest.TenantJobRequestMedia.Add(new TenantJobRequestMedia
                    {
                        NewFileName = x.NewFileName,
                        OldFileName = x.OldFileName,
                    }));
                    db.SaveChanges();
                    var perId = request.RecipientId;
                    if (perId.HasValue)
                    {
                        var addr = request.Property.Address;
                        var ten = db.Person.FirstOrDefault(x => x.Id == perId);
                        var tenLog = ten.Login;
                        PropertyOwnerService.SendAcceptRequestEmail(ten.FirstName, tenLog.UserName, addr.ToAddressString());
                    }
                    
                    return new ServiceResponseResult { IsSuccess = true };

                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }

        public static ServiceResponseResult AddRentallApllication(RentalApplicationModel model, Login login, HttpFileCollectionBase files = null)
        {
            using (var db = new KeysEntities())
            {
                var result = new ServiceResponseResult { IsSuccess = false };
                if (!TenantService.IsLoginATenant(login))
                {
                    var errorMsg = "Account not tenant!";
                    result.ErrorMessage = errorMsg;
                    return result;
                }
                var rentalApp = new RentalApplication
                {
                    RentalListingId = model.RentalListingId,
                    PersonId = login.Id,
                    TenantsCount = model.TenantsCount,
                    ApplicationStatusId = 1,
                    Note = model.Note,
                    CreatedBy = login.Email,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedBy = login.Email,
                    UpdatedOn = DateTime.UtcNow,
                    IsActive = true,
                };
                var mediaFiles = MediaService.SaveFiles(files, 5, AllowedFileType.AllFiles).NewObject as List<MediaModel>;
                mediaFiles.ForEach(x => rentalApp.RentalApplicationMedia.Add(new RentalApplicationMedia
                {
                    NewFileName = x.NewFileName,
                    OldFileName = x.OldFileName,
                }));
                try
                {
                    db.RentalApplication.Add(rentalApp);
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }

        public static ServiceResponseResult EditRentallApllication(RentalApplicationModel model, Login login, HttpFileCollectionBase files = null)
        {
            using (var db = new KeysEntities())
            {
                var result = new ServiceResponseResult { IsSuccess = false };
                if (!TenantService.IsLoginATenant(login))
                {
                    var errorMsg = "Account not tenant!";
                    result.ErrorMessage = errorMsg;
                    return result;
                }
                var foundRentalApplication = db.RentalApplication.Where(x => x.Id == model.Id).FirstOrDefault();
                if (foundRentalApplication == null)
                {
                    var errorMsg = "Cannot locate the Rental application";
                    result.ErrorMessage = errorMsg;
                    return result;
                }
                else
                {
                    foundRentalApplication.RentalListingId = model.RentalListingId;
                    foundRentalApplication.PersonId = login.Id;
                    foundRentalApplication.TenantsCount = model.TenantsCount;
                    foundRentalApplication.Note = model.Note;
                    foundRentalApplication.ApplicationStatusId = 1;
                    foundRentalApplication.CreatedBy = login.Email;
                    foundRentalApplication.CreatedOn = DateTime.UtcNow;
                    foundRentalApplication.UpdatedBy = login.Email;
                    foundRentalApplication.UpdatedOn = DateTime.UtcNow;
                    foundRentalApplication.IsActive = true;

                    model.FilesRemoved.ForEach(x =>
                    {
                        var media = db.RentalApplicationMedia.FirstOrDefault(y => y.Id == x);
                        if (media != null) { db.RentalApplicationMedia.Remove(media); MediaService.RemoveMediaFile(media.NewFileName); }
                    });
                    var mediaFiles = MediaService.SaveFiles(files, 5 - foundRentalApplication.RentalApplicationMedia.Count, AllowedFileType.AllFiles).NewObject as List<MediaModel>;
                    if (mediaFiles != null)
                    {
                        mediaFiles.ForEach(x => foundRentalApplication.RentalApplicationMedia.Add(new RentalApplicationMedia
                        {
                            NewFileName = x.NewFileName,
                            OldFileName = x.OldFileName,
                        }));
                    }
                };
                try
                {

                    db.SaveChanges();
                    var mFiles = new List<MediaModel>();
                    var medias = foundRentalApplication.RentalApplicationMedia
                                .Select(x => MediaService.GenerateViewProperties(new MediaModel { Id = x.Id, NewFileName = x.NewFileName, OldFileName = x.OldFileName })).ToList();
                    return new ServiceResponseResult { IsSuccess = true, NewObject = mFiles };

                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }

        public static ServiceResponseResult DeleteRentallApllication(int rentalApplicationId)
        {
            var result = new ServiceResponseResult { IsSuccess = false };
            using (var db = new KeysEntities())
            {
                var deleteRentallApllication = db.RentalApplication.Where(x => x.Id == rentalApplicationId).First();
                if (deleteRentallApllication == null)
                {
                    var errorMsg = "Cannot locate the Rental application";
                    result.ErrorMessage = errorMsg;
                    return result;
                }
                else
                {
                    deleteRentallApllication.IsActive = false;
                };
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

        public static Person GetOwnerDetails(RentalApplicationModel model)
        {
            using (var db = new KeysEntities())
            {
                var ownerId = db.RentalListing.Where(x => x.Id == model.RentalListingId).Select(y => y.Property.OwnerProperty.FirstOrDefault().Person.Id).FirstOrDefault();
                return db.Person.FirstOrDefault(x => x.Id == ownerId);
            }

        }

        public static IEnumerable<RequestType> GetRequestTypes()
        {
            using (var db = new KeysEntities())
            {
                return db.RequestType.ToList();
            }
        }

        public static RentalApplication GetRentalApplicationById(int id)
        {
            using (var db = new KeysEntities())
            {
                return db.RentalApplication.FirstOrDefault(x => x.Id == id);
            }
        }

        public static ServiceResponseResult UpdateApplicationView(int appId)
        {
            using (var db = new KeysEntities())
            {

                var foundApplication = db.RentalApplication.Where(x => x.Id == appId).FirstOrDefault();
                if (foundApplication == null) return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "Item not found!" };
                if (foundApplication.IsViewedByOwner == true) { return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "Already Viewed" }; }
                if (foundApplication.IsViewedByOwner ?? false) return new ServiceResponseResult { IsSuccess = false };
                foundApplication.IsViewedByOwner = true;
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
        public static ServiceResponseResult<List<MediaModel>> EditRentalListing(RentListingModel model, HttpFileCollectionBase files, Login user)
        {
            using (var db = new KeysEntities())
            {
                var foundProp = db.OwnerProperty.FirstOrDefault(x => x.PropertyId == model.PropertyId && x.OwnerId == user.Id);
                if (foundProp == null)
                {
                    return new ServiceResponseResult<List<MediaModel>> { IsSuccess = false, ErrorMessage = "Can not find property." };
                }
                var oldRentalListing = db.RentalListing.Find(model.Id);
                oldRentalListing.Description = model.Description;
                oldRentalListing.Title = model.Title;
                oldRentalListing.MovingCost = model.MovingCost;
                oldRentalListing.TargetRent = model.TargetRent;
                oldRentalListing.AvailableDate = model.AvailableDate;
                oldRentalListing.Furnishing = model.Furnishing;
                oldRentalListing.IdealTenant = model.IdealTenant;
                oldRentalListing.OccupantCount = model.OccupantCount;
                oldRentalListing.PetsAllowed = model.PetsAllowed;
                oldRentalListing.UpdatedBy = user.Email;
                oldRentalListing.UpdatedOn = DateTime.UtcNow;
                if (model.FilesRemoved != null)
                {
                    model.FilesRemoved.ToList().ForEach(x =>
                     {
                         var media = oldRentalListing.RentalListingMedia.FirstOrDefault(y => y.Id == x);
                         if (media != null)
                         {
                             db.RentalListingMedia.Remove(media);
                             MediaService.RemoveMediaFile(media.NewFileName);
                         }
                     });
                }


                var fileList = MediaService.SaveMediaFiles(files, 5, null).NewObject as List<MediaModel>;
                if (fileList != null)
                {
                    fileList.ForEach(x => oldRentalListing.RentalListingMedia.Add(new RentalListingMedia { NewFileName = x.NewFileName, OldFileName = x.OldFileName }));
                }

                try
                {
                    db.SaveChanges();
                    var medias = oldRentalListing.RentalListingMedia
                                .Select(x => MediaService.GenerateViewProperties(new MediaModel { Id = x.Id, NewFileName = x.NewFileName, OldFileName = x.OldFileName })).ToList();
                    return new ServiceResponseResult<List<MediaModel>> { IsSuccess = true, Result = medias };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult<List<MediaModel>> { IsSuccess = true, ErrorMessage = _error };
                }
            }
        }

        public static ServiceResponseResult AddRentalListing(RentalListingModel model, HttpFileCollectionBase files, Login user)
        {
            using (var db = new KeysEntities())
            {
                var foundProp = db.OwnerProperty.FirstOrDefault(x => x.PropertyId == model.PropertyId && x.OwnerId == user.Id);
                if (foundProp == null)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "Can not find property." };
                }
                var newRentalListing = new RentalListing
                {
                    PropertyId = model.PropertyId,
                    Description = model.RentalDescription,
                    Title = model.Title = model.Title,
                    MovingCost = model.MovingCost,
                    TargetRent = model.TargetRent,
                    AvailableDate = model.AvailableDate,
                    Furnishing = model.Furnishing,
                    IdealTenant = model.IdealTenant,
                    OccupantCount = model.OccupantCount,
                    PetsAllowed = model.PetsAllowed,
                    CreatedBy = user.Email,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedBy = user.Email,
                    IsActive = true,
                    RentalStatusId = 1
                };
                var fileList = MediaService.SaveMediaFiles(files, 5, null).NewObject as List<MediaModel>;
                if (fileList != null)
                {
                    fileList.ForEach(x => newRentalListing.RentalListingMedia.Add(new RentalListingMedia { NewFileName = x.NewFileName, OldFileName = x.OldFileName }));
                }
                db.RentalListing.Add(newRentalListing);
                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = true, ErrorMessage = _error };
                }
            }

        }

        public static SearchResult GetTenantRequests(POTenantRequestSearchModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var props = db.OwnerProperty.Where(x => x.OwnerId == login.Id && x.Property.IsActive);
                var propsId = props.Select(x => x.Property.Id);
                var reqs = props.Select(x => x.Property)
                            .SelectMany(x => x.PropertyRequest.Where(y => y.ToOwner && !y.ToTenant && y.IsActive));
                var tents = db.TenantProperty.Where(x => propsId.Contains(x.PropertyId)).Select(x => x.Tenant).GroupBy(x => x.Id).Select(g => g.FirstOrDefault());
                var data = from req in reqs
                           join ten in tents on req.CreatedBy equals ten.Id
                           select new TenantRequestViewModel
                           {
                               Model = new RequestModel
                               {
                                   Id = req.Id,
                                   PropertyId = req.PropertyId,
                                   RequestTypeId = req.RequestTypeId,
                                   RequestMessage = req.RequestMessage,
                                   IsViewed = req.IsViewed,
                                   RequestStatusId = req.RequestStatusId,
                                   MediaFiles = req.PropertyRequestMedia.Select(y => new MediaModel
                                   {
                                       Data = y.NewFileName,
                                       Id = y.Id,
                                       NewFileName = y.NewFileName,
                                       OldFileName = y.OldFileName,
                                       Status = "load"
                                   }).ToList()
                               },
                               RequestType = req.RequestType.Name,
                               RequestStatus = req.RequestStatus.Name,
                               TenantId = ten.Id,
                               CreatedOn = req.CreatedOn,
                               Address = new AddressViewModel
                               {
                                   AddressId = req.Property.Address.AddressId,
                                   CountryId = req.Property.Address.CountryId,
                                   Number = req.Property.Address.Number.Replace(" ", ""),
                                   Street = req.Property.Address.Street.Trim(),
                                   City = req.Property.Address.City.Trim(),
                                   Suburb = req.Property.Address.Suburb.Trim() ?? "",
                                   PostCode = req.Property.Address.PostCode.Replace(" ", "")
                               },
                               TenantName = (ten.Person.FirstName ?? "") + " " + (ten.Person.LastName ?? ""),
                               TenantPhoneNumber = ten.MobilePhoneNumber,
                               TenantProfileFoto = ten.Person.ProfilePhoto,
                           };
                if (model.RequestStatus.HasValue)
                {
                    var status = (int)model.RequestStatus.Value;
                    data = data.Where(x => x.Model.RequestStatusId == status);
                }
                var allItems = data.OrderByDescending(x => x.CreatedOn).ToPagedList(model.Page, 10);
                switch (model.SortOrder)
                {
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

                    data = data.AsEnumerable().Where(x => x.Model.RequestMessage.ToLower().Contains(formatString)
                                           || x.CreatedOn.ToString("MMMM d yyyy").ToLower().Contains(formatString.Replace(",", ""))
                                           || (x.RequestStatus ?? "").ToLower().Contains(formatString)).AsQueryable();

                };
                var items = data.ToPagedList(model.Page, 10);
                var count = items.Count;
                items = count == 0 ? allItems : items;
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                items.ToList().ForEach(x =>
                {
                    x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties());
                    x.TenantProfileFoto = urlHelper.Content(MediaService.GetContentPath(x.TenantProfileFoto));
                });
                return new SearchResult { SearchCount = count, Items = items };
            }
        }

        public static RentAppDashboardModel GetDashboardRentAppsInfo(int ownerId)
        {
            using (var db = new KeysEntities())
            {
                var props = db.OwnerProperty.Where(x => x.OwnerId == ownerId && x.Property.IsActive);
                var propIds = props.Select(x => x.Id);
                var rentApps = db.RentalApplication.Where(x => propIds.Contains(x.RentalListing.PropertyId) && x.IsActive);
                var newApps = rentApps.Where(x => x.ApplicationStatusId != 3 && (!x.IsViewedByOwner ?? false)).Count();
                var approved = rentApps.Where(x => x.ApplicationStatusId == 2).Count(); ;
                var pendingApps = rentApps.Where(x => (x.IsViewedByOwner ?? false) && x.ApplicationStatusId == 1).Count();
                var declinedApps = rentApps.Where(x => x.ApplicationStatusId == 3).Count();
                return new RentAppDashboardModel
                {
                    NewItems = newApps,
                    Approved = approved,
                    Pending = pendingApps,
                    Rejected = declinedApps,
                };
            }
        }
    }
}

using KeysPlus.Data;
using KeysPlus.Service.Models;
using PagedList;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;

namespace KeysPlus.Service.Services
{
    public class TenantService
    {
        public static readonly string _error = "Something went wrong please try again later";

        public static DateTime? GetNextPaymentDate(DateTime? startDate, int? dueDate, int? freq)
        {
            if(!startDate.HasValue || !dueDate.HasValue || !freq.HasValue)
            {
                return null;
            }
            else
            {
                var days = freq.Value == 1 ? 7 : freq.Value == 2 ? 14 : 30;
                var today = DateTime.UtcNow;
                var dayReturn = startDate.Value.AddDays(dueDate.Value);
                while (dayReturn < today)
                {
                    dayReturn = dayReturn.AddDays(days);
                }
                return dayReturn;
            }
        }
        public static IEnumerable<TenantRentalViewModel> GetMyRentals(int loginId)
        {
            using (var db = new KeysEntities())
            {
                var allRps = db.TenantProperty.Include("Property").Include("Property.Address").Include("Property.PropertyMedia");
                var rps = allRps.Where(tp => tp.TenantId == loginId && (tp.IsActive ?? false) && tp.Property.IsActive).ToList();
                var propIds = rps.Select(x => x.PropertyId).ToList();
                var owPs = db.OwnerProperty.Where(x => propIds.Contains(x.PropertyId)).GroupBy(x => x.PropertyId)
                    .Select(g =>new { PropertyId = g.FirstOrDefault().PropertyId,
                        Person = g.FirstOrDefault().Person,
                        Login = g.FirstOrDefault().Person.Login
                    }).ToList();
                var data =(from x in rps
                            join op in owPs on x.PropertyId equals op.PropertyId
                            select new TenantRentalViewModel
                            {
                                Model = new MyRentalsModel
                                {
                                    Id = x.Id,
                                    PropertyId = x.PropertyId,
                                    TenantId = loginId,
                                    PaymentAmount = x.PaymentAmount,
                                    PaymentStartDate = x.PaymentStartDate,
                                    PaymentDueDate = x.PaymentDueDate,
                                    PaymentFrequencyId = x.PaymentFrequencyId,
                                    StartDate = x.StartDate,
                                    MediaFiles = x.Property.PropertyMedia.Select(y => new MediaModel
                                    {
                                        Id = y.Id,
                                        Data = y.NewFileName,
                                        NewFileName = y.NewFileName,
                                        OldFileName = y.OldFileName,
                                        Status = "load"
                                    }).ToList(),
                                },
                                CreatedOn = x.CreatedOn,
                                AddressString = x.Property.Address.Number + " " + x.Property.Address.Street + " " + x.Property.Address.City,
                                RentalPaymentType = x.TenantPaymentFrequencies?.Name,
                                Landlordname = op.Person.FirstName,
                                LandlordPhone = op.Person.Login.PhoneNumber,
                                LandlordId = op.Person.Id,
                                Address = new AddressViewModel
                                {
                                    AddressId = x.Property.Address.AddressId,
                                    CountryId = x.Property.Address.CountryId,
                                    Number = x.Property.Address.Number != null ? x.Property.Address.Number.Replace(" ", "") : null,
                                    Street = x.Property.Address.Street != null ? x.Property.Address.Street.Trim() : null,
                                    City = x.Property.Address.City != null ? x.Property.Address.City.Trim() : null,
                                    Suburb = x.Property.Address.Suburb != null ? x.Property.Address.Suburb.Trim() : "",
                                    PostCode = x.Property.Address.PostCode != null ? x.Property.Address.PostCode.Replace(" ", "") : null,
                                },
                            }).ToList();
                data.ToList().ForEach( x => { x.NextPaymenDate = GetNextPaymentDate(x.Model.PaymentStartDate, x.Model.PaymentDueDate, x.Model.PaymentFrequencyId); });
                return data;
            }
        }
        public static List<RentalInfoModel> GetRentalInfo(int loginId)
        {
            using (var db = new KeysEntities()) { 
                var allRps = db.TenantProperty.Include("Property").Include("Property.Address").Include("Property.PropertyMedia");
                var rps = allRps.Where(tp => tp.TenantId == loginId && tp.Property.IsActive && (tp.IsActive ?? true)).ToList();
                var propIds = rps.Select(x => x.PropertyId).ToList();
                var owPs = db.OwnerProperty.Where(x => propIds.Contains(x.PropertyId)).GroupBy(x => x.PropertyId)
                    .Select(g => new {
                        PropertyId = g.FirstOrDefault().PropertyId,
                        Person = g.FirstOrDefault().Person,
                        Login = g.FirstOrDefault().Person.Login
                    }).ToList();
                var data = (from x in rps
                            join op in owPs on x.PropertyId equals op.PropertyId
                            select new RentalInfoModel
                            {
                                Id = x.Id,
                                PropertyId = x.PropertyId,
                                LandlordId = op.Person.Id,
                                TenantId = loginId,
                                PaymentAmount = x.PaymentAmount,
                                PaymentStartDate = x.PaymentStartDate,
                                PaymentDueDate = x.PaymentDueDate,
                                RentalPaymentType = x.TenantPaymentFrequencies?.Name,
                                Landlordname = op.Person.FirstName,
                                LandlordPhone = op.Person.Login.PhoneNumber,
                                PaymentFrequencyId = x.PaymentFrequencyId,
                                Address = x.Property.Address.Number + " " + x.Property.Address.Street + " " + x.Property.Address.City,
                                PropertyAddress = new AddressViewModel
                                {
                                    AddressId = x.Property.Address.AddressId,
                                    CountryId = x.Property.Address.CountryId,
                                    Number = x.Property.Address.Number != null ? x.Property.Address.Number.Replace(" ", "") : null,
                                    Street = x.Property.Address.Street != null ? x.Property.Address.Street.Trim() : null,
                                    City = x.Property.Address.City != null ? x.Property.Address.City.Trim() : null,
                                    Suburb = x.Property.Address.Suburb != null ? x.Property.Address.Suburb.Trim() : "",
                                    PostCode = x.Property.Address.PostCode != null ? x.Property.Address.PostCode.Replace(" ", "") : null,
                                },
                            }).ToList();
                data.ToList().ForEach(x => { x.NextPaymenDate = GetNextPaymentDate(x.PaymentStartDate, x.PaymentDueDate, x.PaymentFrequencyId); });
                return data;
            }
        }

        public static List<MyRentalsModel> GetTenantRentals(int loginId)
        {
            using (var db = new KeysEntities())
            {
                try
                {
                    List<MyRentalsModel> rentalModel = new List<MyRentalsModel>();
                    var myRentProps = db.TenantProperty.Where(tp => tp.TenantId == loginId && (tp.IsActive ?? true)).ToList();
                    var propertyIds = myRentProps.Select(pt => pt.PropertyId).ToList();
                    foreach (var tp in myRentProps)
                    {
                        var id = tp.PropertyId;
                        var landlordId = db.OwnerProperty.Where(op => op.PropertyId == id).Select(op => op.OwnerId).FirstOrDefault();
                        var landlordDetails = db.Login.FirstOrDefault(l => l.Id == landlordId);
                        var person = db.Person.FirstOrDefault(p => p.Id == landlordId);
                        var propertyDetails = db.Property.FirstOrDefault(p => p.Id == id);
                        var photolist = db.PropertyMedia.Where(p => p.PropertyId == id).ToList();

                        var model = new MyRentalsModel
                        {
                            OwnerId = landlordId,
                            Id = tp.Id,
                            PropertyId = propertyDetails.Id,
                            TenantId = loginId,
                            Address = propertyDetails.Address.Number + " " + propertyDetails.Address.Street + " " + propertyDetails.Address.City,
                            PropertyAddress = new AddressViewModel
                            {
                                AddressId = propertyDetails.Address.AddressId,
                                CountryId = propertyDetails.Address.CountryId,
                                Number = propertyDetails.Address.Number?.Replace(" ", ""),
                                Street = propertyDetails.Address.Street?.Trim(),
                                City = propertyDetails.Address.City?.Trim(),
                                Suburb = propertyDetails.Address.Suburb?.Trim() ?? "",
                                PostCode = propertyDetails.Address.PostCode?.Replace(" ", ""),
                            },
                            Landlordname = person.FirstName + " " + person.LastName,
                            LandlordPhone = landlordDetails.PhoneNumber ?? "Not Available",
                            LandlordId = landlordId,
                            TargetRent = propertyDetails.TargetRent,
                            PaymentAmount = tp.PaymentAmount,
                            RentalPaymentType = tp.PaymentFrequencyId.ToString(),
                            //RentalPaymentType = propertyDetails.TargetRentType.Name,
                            PaymentStartDate = tp.PaymentStartDate,
                            PaymentDueDate = tp.PaymentDueDate,
                        };
                        foreach (var photo in photolist)
                        {
                            model.MediaFiles.Add(new MediaModel()   //add photos to the PhotoFiles List
                            {
                                Id = photo.Id,
                                Data = photo.NewFileName,
                                //Data = Url.Content("~/images/" + photo.NewFileName),    
                                NewFileName = photo.NewFileName,
                                OldFileName = photo.OldFileName,
                                Status = "load"
                            });
                        }
                        rentalModel.Add(model);
                    }
                    return rentalModel;

                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public static Tenant GetTenantByEmail(string email)
        {
            using (var db = new KeysEntities())
            {
                var login = db.Login.FirstOrDefault(x => x.Email == email);
                return login == null ? null : db.Tenant.FirstOrDefault(x => x.Id == login.Id);
            }
        }

        public static Tenant GetTenantByLogin(Login login)
        {
            using (var db = new KeysEntities())
            {
                return login == null ? null : db.Tenant.FirstOrDefault(x => x.Id == login.Id);
            }
        }

        public static bool IsUsernameTenant(string userName)
        {
            using (var db = new KeysEntities())
            {
                var login = db.Login.FirstOrDefault(x => x.Email == userName);

                return login == null ? false : db.Tenant.FirstOrDefault(x => x.Id == login.Id) == null ? false : true;
            }
        }

        public static DateTime? GetPaymentDate(int id, int tid)
        {
            using (var db = new KeysEntities())
            {
                var d = db.TenantProperty.Where(x => x.PropertyId == id && x.TenantId == tid).Select(y => y.PaymentStartDate);
                return d.ToList()[0];
            }
        }

        public static bool IsLoginATenant(Login login)
        {
            using (var db = new KeysEntities())
            {
                //return login == null ? false : db.Tenant.FirstOrDefault(x => x.Id == login.Id && x.IsActive) == null ? false : true;
                return login == null ? false : db.LoginRole.FirstOrDefault(x => x.PersonId == login.Id && x.RoleId == 5) == null ? false : true;
            }
        }

        public static ServiceResponseResult SaveDetails(TenantDetailsModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var result = new ServiceResponseResult { IsSuccess = false };

                if (!IsLoginATenant(login))
                {
                    var errorMsg = "Account not tenant!";
                    result.ErrorMessage = errorMsg;
                    return result;
                }
                var address = new Address
                {
                    CountryId = 1,
                    Number = model.Address.Number,
                    Street = model.Address.Street,
                    Suburb = model.Address.Suburb,
                    City = model.Address.City,
                    PostCode = model.Address.PostCode,
                    Lat = model.Address.Latitude,
                    Lng = model.Address.Longitude,
                    UpdatedBy = login.Email,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = login.Email,
                    UpdatedOn = DateTime.UtcNow,
                    IsActive = true
                };
                var tenant = db.Tenant.FirstOrDefault(x => x.Id == login.Id);
                tenant.DateOfBirth = model.DateOfBirth;
                tenant.HomePhoneNumber = model.HomePhoneNumber;
                tenant.MobilePhoneNumber = model.MobilePhoneNumber;
                tenant.CreatedOn = DateTime.UtcNow;
                tenant.UpdatedOn = DateTime.UtcNow;
                tenant.CreatedBy = login.Id;
                tenant.UpdatedBy = login.Id;
                tenant.IsActive = true;
                tenant.HasProofOfIdentity = false;
                tenant.IsCompletedPersonalProfile = true;
                var person = db.Person.FirstOrDefault(m => m.Id == login.Id);

                //try
                //{
                    db.Address.Add(address);
                    db.SaveChanges();
                    tenant.Address = address;
                    person.Address1 = address;
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                //}
                //catch (Exception ex)
                //{
                //    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                //}
            }
        }

        public static ServiceResponseResult AddTenantMediaFiles(HttpFileCollectionBase files, Tenant tenant, string serverPath)
        {
            var allowedFiles = 10;

            if (files.Count > allowedFiles)
            {
                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = $"You can't add more than {allowedFiles} photos" };
            };
            if (files != null && files.Count > 0)
            {
                var numberOfFiles = files.Count < allowedFiles ? files.Count : allowedFiles;
                List<string> acceptedExtensions = new List<string> { ".jpg", ".png", ".gif", ".jpeg" };
                using (var db = new KeysEntities())
                {

                    try
                    {
                        for (int i = 0; i < numberOfFiles; i++)
                        {
                            var file = files[i];
                            var fileExtension = Path.GetExtension(file.FileName);
                            if (fileExtension != null && !acceptedExtensions.Contains(fileExtension.ToLower()))
                            {
                                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = $"Supported file types are *.jpg, *.png, *.gif, *.jpeg" };
                            }
                            else
                            {
                                var fileName = Path.GetFileName(file.FileName);
                                var index = fileName.LastIndexOf(".");
                                var newFileName = fileName.Insert(0, $"{Guid.NewGuid()}");
                                var physicalPath = Path.Combine(serverPath, newFileName);
                                file.SaveAs(physicalPath);
                                var foundTenant = db.Person.Where(x => x.Id == tenant.Id).FirstOrDefault();
                                foundTenant.ProfilePhoto = newFileName;
                                tenant.HasProofOfIdentity = true;
                                db.Tenant.Attach(tenant);
                                var entry = db.Entry(tenant);
                                entry.Property(p => p.HasProofOfIdentity).IsModified = true;
                                db.SaveChanges();
                            }
                        }

                        return new ServiceResponseResult { IsSuccess = true };
                    }
                    catch (Exception ex)
                    {
                        return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                    }
                }
            }
            else
            {
                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "You have not specified a file." };
            }
        }

        public static bool IsTenantInProperty(string tenantEmail, int propertyId)
        {
            using (var db = new KeysEntities())
            {
                var tenant = db.Login.FirstOrDefault(x => x.Email == tenantEmail);
                if (tenant == null) return false;
                return db.TenantProperty.Where(x => x.TenantId == tenant.Id && x.PropertyId == propertyId).Any();
            }
        }
        public static bool IsTenantInProperty(Login tenant, int propertyId)
        {
            using (var db = new KeysEntities())
            {
                return db.TenantProperty.Where(x => x.TenantId == tenant.Id && x.PropertyId == propertyId).Any();
            }
        }

        public static bool IsProfileComplete(string email)
        {
            var tenant = GetTenantByEmail(email);
            return tenant == null ? false : tenant.IsCompletedPersonalProfile ? true : false;
        }

        public static SearchResult GetAllRentApplications(RentalAppSearchModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var data = db.RentalApplication.Where(x => x.PersonId == login.Id && x.IsActive == true)
                    .Select(x => new
                    {
                        Model = new RentalApplicationModel
                        {
                            Id = x.Id,
                            RentalListingId = x.RentalListingId,
                            PersonId = x.PersonId,
                            Note = x.Note,
                            ApplicationStatusId = x.ApplicationStatusId,
                            TenantsCount = x.TenantsCount,
                            IsViewedByOwner = x.IsViewedByOwner ?? false ,
                            MediaFiles = x.RentalApplicationMedia.Select(y => new MediaModel
                            {
                                Id = y.Id,
                              Status = "load",
                                NewFileName = y.NewFileName,
                                OldFileName = y.OldFileName,                                
                                Data = y.NewFileName
                            }).ToList()
                        },
                        CreatedOn = x.CreatedOn,
                        Status = new {
                            Id = x.RentalApplicationStatus.Id,
                            Status = x.RentalApplicationStatus.Status,
                        },

                        RentalListing = new RentListingModel {
                            Title = x.RentalListing.Title,
                            Description = x.RentalListing.Description,
                            MovingCost = x.RentalListing.MovingCost,
                            TargetRent = x.RentalListing.TargetRent,
                            AvailableDate = x.RentalListing.AvailableDate,
                            Furnishing = x.RentalListing.Furnishing,
                            IdealTenant = x.RentalListing.IdealTenant,
                            OccupantCount = x.RentalListing.OccupantCount,
                            PetsAllowed = x.RentalListing.PetsAllowed,
                           MediaFiles = x.RentalListing.RentalListingMedia.Select( y => new MediaModel { Id = y.Id,  Status = "load", NewFileName = y.NewFileName, OldFileName = y.OldFileName, Data = y.NewFileName }).ToList(),
                           },
                        RentalPaymentType = x.RentalListing.Property.TargetRentType.Name,
                        Bedrooms = x.RentalListing.Property.Bedroom ?? 0,
                        Bathrooms = x.RentalListing.Property.Bathroom ?? 0,
                        ParkingSpaces = x.RentalListing.Property.ParkingSpace ?? 0,
                        LandSqm = x.RentalListing.Property.LandSqm ?? 0,
                        FloorArea = x.RentalListing.Property.FloorArea ?? 0,
                        PropertyType = x.RentalListing.Property.TargetRentType.Name,
                        AddressString = x.RentalListing.Property.Address.Number + " " + x.RentalListing.Property.Address.Street + ", " + x.RentalListing.Property.Address.Suburb + ", " + x.RentalListing.Property.Address.City + " - " + x.RentalListing.Property.Address.PostCode,
                        Address = new AddressViewModel
                        {
                            AddressId = x.RentalListing.Property.Address.AddressId,
                            CountryId = x.RentalListing.Property.Address.CountryId,
                            Number = x.RentalListing.Property.Address.Number.Replace(" ", ""),
                            Street = x.RentalListing.Property.Address.Street.Trim(),
                            City = x.RentalListing.Property.Address.City.Trim(),
                            Suburb = x.RentalListing.Property.Address.Suburb.Trim() ?? "",
                            PostCode = x.RentalListing.Property.Address.PostCode.Replace(" ", ""),
                        },
                    });
                if (model.RentalStatus.HasValue)
                {
                    data = data.Where(x => x.Model.ApplicationStatusId == (int)model.RentalStatus.Value);
                }
                var allItems = data.OrderByDescending(x => x.CreatedOn).ToPagedList(model.Page, 10);
                switch (model.SortOrder)
                {
                    case "Lowest Rent":
                        data = data.OrderBy( x => x.RentalListing.TargetRent);
                        break;
                    case "Highest Rent":
                        data = data.OrderByDescending(x => x.RentalListing.TargetRent);
                        break;
                    case "Earliest Date":
                        data = data.OrderBy(x => x.CreatedOn);
                        break;
                    case "Latest Date":
                        data = data.OrderByDescending(x => x.CreatedOn);
                        break;
                    case "Earliest Available":
                        data = data.OrderBy(x => x.RentalListing.AvailableDate);
                        break;
                    case "Latest Available":
                        data = data.OrderByDescending(x => x.RentalListing.AvailableDate);
                        break;
                    case "Accepted First":
                        data = data.OrderBy(x => x.Status.Status);
                        break;
                    default:
                        data = data.OrderByDescending(x => x.CreatedOn);
                        break;
                }
                if (!String.IsNullOrWhiteSpace(model.SearchString))
                {
                    SearchUtil searchTool = new SearchUtil();
                    int searchType = searchTool.CheckDisplayType(model.SearchString);
                    string formatString = searchTool.ConvertString(model.SearchString);
                    data = data.AsEnumerable().Where(x => (x.AddressString?.ToLower().Contains(formatString) ?? false)
                                           || (x.Model.Note?.ToLower().Contains(formatString) ?? false)
                                           || x.RentalListing.AvailableDate.Value.ToString("MMMM d yyyy").ToLower().Contains(formatString.Replace(",", ""))
                                           || x.RentalListing.AvailableDate.Value.ToString("d MMMM yyyy").ToLower().Contains(formatString.Replace(",", ""))
                                           || x.RentalListing.AvailableDate.Value.ToString("MMM d yyyy").ToLower().Contains(formatString.Replace(",", ""))
                                           || x.RentalListing.AvailableDate.Value.ToString("d MMM yyyy").ToLower().Contains(formatString.Replace(",", ""))
                                           || (x.Status.Status ?? "").ToLower().Contains(formatString)).AsQueryable();

                };
                var items = data.ToPagedList(model.Page, 10);
                var count = items.Count;
                items = count == 0 ? allItems : items;
                items.ToList().ForEach(x => { x.Model.MediaFiles.ForEach( y => y.InjectMediaModelViewProperties()); });
                return new SearchResult { SearchCount = count, Items = items};
            }
        }

        public static MyRequestViewModel GetRequestById(int id)
        {
            using (var db = new KeysEntities())
            {
                var requestDetails = db.PropertyRequest.FirstOrDefault(x => x.Id == id && x.RequestTypeId == 3);
                if (requestDetails == null) return null;
                var a = new MyRequestViewModel
                {
                    Model = new RequestModel
                    {
                        Id = requestDetails.Id,
                        RequestMessage = requestDetails.RequestMessage,
                        RequestTypeId = requestDetails.RequestTypeId,
                        RequestStatusId = requestDetails.RequestStatusId,
                        MediaFiles = requestDetails.PropertyRequestMedia.Select(y => new MediaModel { NewFileName = y.NewFileName, OldFileName = y.OldFileName, Id = y.Id }).ToList()
                    },
                    RequestStatus = requestDetails.RequestStatus.Name,
                    RequestType = requestDetails.RequestType.Name,
                    CreatedOn = requestDetails.CreatedOn,
                };
                a.Model.MediaFiles.ForEach(x => x.InjectMediaModelViewProperties());
                return a;
            }
        }
        public static ServiceResponseResult EditInspection(InspectionModel model, Login login, HttpFileCollectionBase files)
        {
            using (var db = new KeysEntities())
            {
                var editInspection = db.Inspection.Where(x => x.Id == model.Id).First();
                if (editInspection != null)
                {
                    editInspection.IsUpdated = true;
                    editInspection.PercentDone = model.PercentDone;
                    editInspection.Message = model.Message;
                    editInspection.UpdatedBy = login.Id;
                    editInspection.UpdatedOn = DateTime.UtcNow;
                    if (model.FilesRemoved.Count > 0)
                    {
                        foreach (var file in model.FilesRemoved)
                        {
                            var removeFile = db.InspectionMedia.Find(file);
                            db.InspectionMedia.Remove(removeFile);
                            MediaService.RemoveMediaFile(removeFile.NewFileName);
                        }
                    }
                    var fileList = MediaService.SaveFiles(files, 5 - editInspection.InspectionMedia.Count, AllowedFileType.AllFiles).NewObject as List<MediaModel>;
                    if (fileList != null)
                    {
                        fileList.ForEach(x => editInspection.InspectionMedia.Add(new InspectionMedia { NewFileName = x.NewFileName, InspectionId = editInspection.Id, OldFileName = x.OldFileName, IsActive = true }));
                    }
                };
                try
                {
                    db.SaveChanges();
                    var mediaFiles = editInspection.InspectionMedia.Select( x => MediaService.GenerateViewProperties(new MediaModel { NewFileName = x.NewFileName, OldFileName = x.OldFileName })).ToList();
                    mediaFiles = mediaFiles ?? new List<MediaModel>();
                    return new ServiceResponseResult { IsSuccess = true , NewObject = mediaFiles};
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = true, ErrorMessage = _error };
                }


            }
        }

        public static ServiceResponseResult DeclineRequest(RequestModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var request = db.PropertyRequest.Where(x => x.Id == model.Id).First();
                var rId = request.RequestStatusId;
                if (rId == 2 || rId == 4 || rId == 5)
                {
                    return new ServiceResponseResult { IsSuccess = true, ErrorMessage = "Can not update request!" };
                }
                if (request != null)
                {
                    request.Reason = model.Reason;
                    request.RequestStatusId = 5;
                    request.UpdatedBy = login.Id;
                    request.UpdatedOn = DateTime.UtcNow;
                };
                try
                {
                    db.SaveChanges();
                    
                    var nvc = new NameValueCollection();
                    nvc.Add("RequestStatus", "Declined");
                    string url = request.ToOwner ? UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "Tenants/Home/MyRequests", UtilService.ToQueryString(nvc)) :
                        UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "PropertyOwners/Property/MyRequests", UtilService.ToQueryString(nvc));
                    var recipent = db.Person.Where(x => x.Id == request.CreatedBy).First();
                    SendGridEmailModel mail = new SendGridEmailModel
                    {
                        RecipentName = recipent.FirstName + " " + recipent.MiddleName + " " + recipent.LastName,
                        ButtonText = "",
                        ButtonUrl = url,
                        RecipentEmail = db.Login.Where(x=>x.Id == request.CreatedBy).First().Email,
                        JobTitle = request.RequestMessage ?? "No Description",
                    };
                    EmailService.SendEmailWithSendGrid(EmailType.DeclineRequest, mail);
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception)
                {
                    return new ServiceResponseResult { IsSuccess = true, ErrorMessage = _error };
                }
            }
        }
        public static ServiceResponseResult AcceptRequest(RequestModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var request = db.PropertyRequest.Where(x => x.Id == model.Id).First();
                var rId = request.RequestStatusId;
                if (rId == 2 || rId == 4 || rId == 5)
                {
                    return new ServiceResponseResult { IsSuccess = true, ErrorMessage = "Can not update request!" };
                }
                if (request != null)
                {
                    request.Reason = model.Reason;
                    request.RequestStatusId = 2;
                    request.UpdatedBy = login.Id;
                    request.UpdatedOn = DateTime.UtcNow;
                };
                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception)
                {
                    return new ServiceResponseResult { IsSuccess = true, ErrorMessage = _error };
                }
            }
        }

        public static SearchResult GetAllRequests(MyRequestSearchModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var data = db.TenantProperty.Where(x => x.TenantId == login.Id && (x.IsActive ?? false)).SelectMany(x => x.Property.PropertyRequest)
                           .Where(x => x.RequestStatusId != 4 && x.IsActive == true && x.ToOwner && !x.ToTenant && x.CreatedBy == login.Id)
                           .Select(x => new MyRequestViewModel
                           {
                               Model = new RequestModel {
                                   Id = x.Id,
                                   PropertyId = x.PropertyId,
                                   RequestMessage = x.RequestMessage,
                                   IsUpdated = x.IsUpdated ?? false,
                                   RequestStatusId = x.RequestStatusId,
                                   MediaFiles = x.PropertyRequestMedia.Select(y => new MediaModel
                                   {
                                       Data = y.NewFileName,
                                       Id = y.Id,
                                       NewFileName = y.NewFileName,
                                       OldFileName = y.OldFileName,
                                       Status = "load"
                                   }).ToList()
                               },
                               PropertyAddress = x.Property.Address.Number + " " + x.Property.Address.Street + ", " + x.Property.Address.Suburb + ", " + x.Property.Address.City,
                               LandlordName = x.Property.OwnerProperty.FirstOrDefault().Person.FirstName + " " + x.Property.OwnerProperty.FirstOrDefault().Person.LastName,
                               LandlordContactNumber = x.Property.OwnerProperty.FirstOrDefault().Person.Login.PhoneNumber ?? "Not Available",
                               RequestType = x.RequestType.Name,
                               RequestStatus = (x.IsViewed && !x.RequestStatusId.Equals((int)JobRequestStatus.Accepted)) ? " Viewed by Owner Decision Pending" : x.RequestStatus.Name,
                               CreatedOn = x.CreatedOn,
                               //CreatedOn = dateadd(hour, datediff(hour, getutcdate(), getdate()), x.CreatedOn),
                           });
                if (model.PropertyId.HasValue)
                {
                    data = data.Where(x => x.Model.PropertyId == model.PropertyId);
                }
                if (model.RequestStatus.HasValue)
                {
                    data = data.Where(x => x.Model.RequestStatusId == (int)model.RequestStatus.Value);
                }
                var allItems = data.OrderByDescending(x => x.CreatedOn).ToPagedList(model.Page, 10);
                switch (model.SortOrder)
                {
                    case "Latest Request":
                        data = data.OrderByDescending(s => s.CreatedOn);
                        break;
                    case "Earliest Request":
                        data = data.OrderBy(s => s.CreatedOn);
                        break;
                    case "Requested Type":
                        data = data.OrderBy(s => s.RequestType);
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
                            data = data.AsEnumerable().Where(x => x.Model.RequestMessage.ToLower().EndsWith(formatString)
                                             || x.RequestStatus.ToLower().EndsWith(formatString)
                                             || x.LandlordName.ToLower().EndsWith(formatString)
                                             || x.LandlordContactNumber.ToLower().EndsWith(formatString)
                                             || x.RequestType.ToLower().EndsWith(formatString)).AsQueryable();
                            break;

                        case 2:
                            data = data.AsEnumerable().Where(x => x.Model.RequestMessage.ToLower().StartsWith(formatString)
                                           || x.RequestStatus.ToLower().StartsWith(formatString)
                                           || x.LandlordName.ToLower().StartsWith(formatString)
                                           || x.LandlordContactNumber.ToLower().StartsWith(formatString)
                                           || x.RequestType.ToLower().StartsWith(formatString)).AsQueryable();
                            break;

                        case 3:
                            data = data.AsEnumerable().Where(x => x.Model.RequestMessage.ToLower().Contains(formatString)
                                        || x.RequestStatus.ToLower().Contains(formatString)
                                        || x.LandlordName.ToLower().Contains(formatString)
                                        || x.LandlordContactNumber.ToLower().Contains(formatString)
                                        || x.RequestType.ToLower().Contains(formatString)).AsQueryable();
                            break;
                    }
                }
                var items = data.ToPagedList(model.Page, 10);
                var count = items.Count;
                items = count == 0 ? allItems : items;
                items.ToList().ForEach(x => x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties()));
                var result = new SearchResult { SearchCount = count, Items = items };
                return result;
            }
        }

        public static SearchResult GetLandlordRequests(Login login, LandlordRequestsSearchModel model)
        {
            using (var db = new KeysEntities())
            {
                var props = db.TenantProperty.Where(x => x.TenantId == login.Id && (x.IsActive ?? true));
                if (model.PropId.HasValue)
                {
                    props = props.Where(x => x.PropertyId == model.PropId);
                }
                var propIds = props.Select(x => x.PropertyId);
                var landLords = db.OwnerProperty.Where(x => propIds.Contains(x.PropertyId))
                                .Select(x => x.Person).GroupBy(x => x.Id).Select(g => g.FirstOrDefault());
                var reqs = db.Property.Where(x => propIds.Contains(x.Id)).SelectMany(x => x.PropertyRequest).Where(y => (y.IsActive && y.ToTenant && !y.ToOwner) && y.RecipientId == login.Id);
                var data = from req in reqs
                           join per in landLords on req.CreatedBy equals per.Id
                           select new MyRequestViewModel
                           {
                               Model = new RequestModel {
                                   RecipientId = req.RecipientId,
                                   Id = req.Id,
                                   PropertyId = req.PropertyId,
                                   RequestMessage = req.RequestMessage,
                                   RequestTypeId = req.RequestTypeId,
                                   RequestStatusId = req.RequestStatusId,
                                   DueDate = req.DueDate,
                                   MediaFiles = req.PropertyRequestMedia.Select(x => new MediaModel
                                   {
                                       Id = x.Id,
                                       NewFileName = x.NewFileName,
                                       OldFileName = x.OldFileName,
                                       Status = "load",
                                   }).ToList(),
                               },
                               RequestType = req.RequestType.Name,
                               RequestStatus = req.RequestStatus.Name,
                               LandlordName = per.FirstName + " " + per.LastName,
                               LandlordContactNumber = per.Login.PhoneNumber,
                               CreatedOn = req.CreatedOn,
                               Address = new AddressViewModel
                               {
                                   AddressId = req.Property.Address.AddressId,
                                   CountryId = req.Property.Address.CountryId,
                                   Number = req.Property.Address.Number.Replace(" ", ""),
                                   Street = req.Property.Address.Street.Trim(),
                                   City = req.Property.Address.City.Trim(),
                                   Suburb = req.Property.Address.Suburb.Trim() ?? "",
                                   PostCode = req.Property.Address.PostCode.Replace(" ", ""),
                               }
                           };
                if (login != null)
                {
                    data = data.Where(x => x.Model.RecipientId == login.Id);
                }
                if (model.PropId.HasValue)
                {
                    data = data.Where(x => x.Model.PropertyId == model.PropId);
                }
                if (model.RequestStatus.HasValue)
                {
                    data = data.Where(x => x.Model.RequestStatusId == (int)model.RequestStatus.Value);
                }
                var allItems = data.OrderByDescending(s => s.CreatedOn).ToPagedList(model.Page, 10);

                switch (model.SortOrder)
                {
                    case "Latest First":
                        data = data.OrderByDescending(s => s.CreatedOn);
                        break;
                    case "Earliest First":
                        data = data.OrderBy(s => s.CreatedOn);
                        break;
                    default:
                        data = data.OrderByDescending(s => s.RequestStatus);
                        break;
                }
                if (!String.IsNullOrWhiteSpace(model.SearchString))
                {
                    var searchTool = new SearchUtil();
                    string formatString = searchTool.ConvertString(model.SearchString);
                    data = data.Where(x => x.Model.RequestMessage.ToLower().Contains(formatString)
                                    || x.RequestStatus.ToLower().Contains(formatString)
                                    || x.LandlordName.ToLower().Contains(formatString)
                                    || x.RequestType.ToLower().Contains(formatString));
                }
                var items = data.ToPagedList(model.Page, 10);
                var count = items.Count;
                items = count == 0 ? allItems : items;
                items.ToList().ForEach(x => x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties()));
                var result = new SearchResult { SearchCount = count, Items = items };
                return result;
            }
        }

        public static ServiceResponseResult AcceptLandlordRequest(int requestId)
        {
            using (var db = new KeysEntities())
            {
                var request = db.PropertyRequest.Where(x => x.Id == requestId).FirstOrDefault();
                var rId = request.RequestStatusId;
                if (rId == 2 || rId == 4 || rId == 5)
                {
                    return new ServiceResponseResult { IsSuccess = true, ErrorMessage = "Can not update request!"};
                }
                //move record to inspection table
                if (request.RequestTypeId == 3)
                {
                    var replyRequest = new Inspection
                    {
                        RequestId = requestId,
                        Message = request.RequestMessage,
                        CreatedBy = request.CreatedBy,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = request.UpdatedBy,
                        UpdatedOn = DateTime.UtcNow,
                        IsActive = true,
                        PropertyId = request.PropertyId,
                        StatusId = 1,
                        PercentDone = 0,
                        OwnerUpdate = false,
                        IsViewed = false,
                        IsUpdated = false

                    };
                    db.Inspection.Add(replyRequest);
                    //move medias relate to the inspection
                    var fileList = db.PropertyRequestMedia.Where(x => x.PropertyRequestId == requestId).ToList();
                    if (fileList != null)
                    {
                        fileList.ForEach(x => replyRequest.InspectionMedia.Add(new InspectionMedia { NewFileName = x.NewFileName, InspectionId = replyRequest.Id, OldFileName = x.OldFileName, IsActive = true }));
                    }
                }

                request.RequestStatusId = 2;
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
        public static ServiceResponseResult AddInspection(InspectionModel model, Login login, HttpFileCollectionBase files)
        {
            using (var db = new KeysEntities())
            {
                var request = db.PropertyRequest.Where(x => x.Id == model.RequestId).FirstOrDefault();
                if (request.RequestTypeId == 3)
                {
                    var replyRequest = new Inspection
                    {
                        RequestId = model.RequestId,
                        Message = model.Message,
                        CreatedBy = login.Id,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = login.Id,
                        UpdatedOn = DateTime.UtcNow,
                        IsActive = true,
                        PropertyId = model.PropertyId,
                        StatusId = 1,
                        PercentDone = model.PercentDone,
                        OwnerUpdate = false,
                        IsViewed = false,
                        IsUpdated = false

                    };
                    db.Inspection.Add(replyRequest);
                    var fileList = MediaService.SaveFiles(files, 5, AllowedFileType.AllFiles).NewObject as List<MediaModel>;
                    if (fileList != null)
                    {
                        fileList.ForEach(x => replyRequest.InspectionMedia.Add(new InspectionMedia { NewFileName = x.NewFileName, InspectionId = replyRequest.Id, OldFileName = x.OldFileName, IsActive = true }));
                    }
                }
                request.RequestStatusId = 2;
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

        public static SearchResult GetAllInspections(TenantInspectionSearchModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var props = db.TenantProperty.Where(x => x.TenantId == login.Id && (x.IsActive ?? true)).Select(x => x.PropertyId);
                var data = db.Inspection.Where(x => props.Contains(x.PropertyId))
                    .Select(x => new TenantInsPectionViewModel
                    {
                        Model = new InspectionModel {
                            Id = x.Id,
                            Message = x.Message,
                            StatusId = x.StatusId,
                            RequestId = x.RequestId,
                            IsViewed = x.IsViewed ?? false,
                            PercentDone = x.PercentDone,
                            OwnerUpdate = x.OwnerUpdate ?? false,
                            Reason = x.Reason,
                            MediaFiles = x.InspectionMedia.Select(y => new MediaModel { Id = y.Id, NewFileName = y.NewFileName, OldFileName = y.OldFileName }).ToList()
                        },
                        CreatedOn = x.CreatedOn,
                        Status = x.InspectionStatus.Name,
                        DueDate = x.PropertyRequest.DueDate,
                        LandlordName = x.Property.OwnerProperty.FirstOrDefault().Person.FirstName + " " + x.Property.OwnerProperty.FirstOrDefault().Person.LastName,
                        LandlordPhone = x.Property.OwnerProperty.FirstOrDefault().Person.Login.PhoneNumber,
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
                    });
                var allItems = data.OrderByDescending(x => x.CreatedOn).ToPagedList(model.Page, 10);
                switch (model.SortOrder)
                {
                    case "Latest First":
                        data = data.OrderByDescending(x => x.CreatedOn);
                        break;
                    case "Earliest First":
                        data = data.OrderBy(x => x.CreatedOn);
                        break;
                    case "High Progress":
                        data = data.OrderByDescending(x => x.Model.PercentDone);
                        break;
                    case "Low Progress":
                        data = data.OrderBy(x => x.Model.PercentDone);
                        break;
                    default:
                        data = data.OrderByDescending(x => x.CreatedOn);
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
                            data = data.Where(x => x.Model.Message.ToLower().EndsWith(formatString)
                                             || x.Status.ToLower().EndsWith(formatString)
                                             || x.Model.Reason.ToLower().EndsWith(formatString));
                            break;

                        case 2:
                            data = data.Where(x => x.Model.Message.ToLower().StartsWith(formatString)
                                           || x.Status.ToLower().StartsWith(formatString)
                                           || x.Model.Reason.ToLower().StartsWith(formatString));
                            break;
                        case 3:
                            data = data.Where(x => x.Model.Message.ToLower().Contains(formatString)
                                            || x.Status.ToLower().StartsWith(formatString)
                                            || x.Model.Reason.ToLower().Contains(formatString));

                            break;
                    }
                };
                var items = data.ToPagedList(model.Page, 10);
                var count = items.Count;
                items = count == 0 ? allItems : items;
                items.ToList().ForEach(x => {
                    x.CanBeEdited = x.DueDate != null ? DateTime.UtcNow > x.DueDate : false;
                    x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties());
                });
                var result = new SearchResult { SearchCount = items.Count, Items = count == 0 ? allItems : items };
                return result;
            }
        }

        public static ServiceResponseResult RemoveFromWatchlist(int rentListingId, Login login)
        {
            using (var db = new KeysEntities())
            {
                var result = new ServiceResponseResult { IsSuccess = false};
                var item = db.RentalWatchList.FirstOrDefault( x => x.PersonId == login.Id && x.RentalListingId == rentListingId);
                if (item == null) {
                    result.ErrorMessage = "Item not found!";
                    return result;
                }
                db.RentalWatchList.Remove(item);
                try
                {
                    db.SaveChanges();
                    result.IsSuccess = true;
                    return result;
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Please try later!";
                    return result;
                }
            }
        }
    }
}

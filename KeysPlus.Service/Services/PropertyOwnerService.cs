using KeysPlus.Data;
using KeysPlus.Service.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Specialized;
using System.Net.Mail;
using PagedList;

namespace KeysPlus.Service.Services
{
    public class PropertyOwnerService
    {
        const string error = "Something went wrong please try again later!";
        public static IEnumerable<TenantPaymentFrequencies> GetAllPaymentFrequencies()
        {
            using (var db = new KeysEntities())
            {
                return db.TenantPaymentFrequencies.ToList();
            }
        }

        public static int GetOwnerId(int propertyId)
        {
            using (var db = new KeysEntities())
            {
                var ownerId = -1;
                var ownerProperty = db.OwnerProperty.FirstOrDefault(p => p.PropertyId == propertyId);
                if (ownerProperty != null)
                    ownerId = ownerProperty.OwnerId;
                return ownerId;
            }
        }

        public static TenantJobRequest UpdateTenantJobRequest(Login user, int jobRequestId, String jobDescription, decimal? maxBudget)
        {
            using (var db = new KeysEntities())
            {
                var jobRequest = db.TenantJobRequest.Find(jobRequestId);
                if (jobRequest != null)
                {
                    jobRequest.JobDescription = jobDescription;
                    jobRequest.MaxBudget = maxBudget;
                    jobRequest.UpdatedBy = user.UserName;
                    jobRequest.UpdatedOn = DateTime.Now;
                    try
                    {
                        db.SaveChanges();
                        return jobRequest;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                return null;
            }
        }
        
        public static ServiceResponseResult EditProperty(PropertyModel model, HttpFileCollectionBase files, Login user)
        {
            //List<String> dlist = new List<String>();
            using (var db = new KeysEntities())
            {
                var property = db.Property.Where(p => p.Id == model.Id).First();
                property.Name = model.Name;
                property.Description = model.Description;
                property.Bedroom = model.Bedroom;
                property.Bathroom = model.Bathroom;
                property.FloorArea = model.FloorArea;
                property.LandSqm = model.LandSqm;
                property.ParkingSpace = model.ParkingSpace;
                property.PropertyTypeId = model.PropertyTypeId;
                property.IsActive = true;
                property.YearBuilt = model.YearBuilt;
                property.TargetRent = model.TargetRent;
                property.Address.Number = model.Address.Number.Replace(" ", "");
                property.Address.Street = model.Address.Street.Trim();
                property.Address.City = model.Address.City.Trim();
                property.Address.Suburb = model.Address.Suburb;
                property.Address.PostCode = model.Address.PostCode.Replace(" ", "");
                property.IsOwnerOccupied = model.IsOwnerOccupied;
                property.TargetRent = model.TargetRent;
                property.UpdatedBy = user.Email;
                property.UpdatedOn = DateTime.UtcNow;
                property.TargetRentTypeId = model.TargetRentTypeId;
                if (model.FilesRemoved != null)
                {
                    model.FilesRemoved.ToList().ForEach(x =>
                    {
                        var media = property.PropertyMedia.FirstOrDefault(y => y.Id == x);
                        if (media != null)
                        {
                            db.PropertyMedia.Remove(media);
                            MediaService.RemoveMediaFile(media.NewFileName);
                        }
                    });
                }
                var fileList = MediaService.SaveFiles(files, 5-property.PropertyMedia.Count(), AllowedFileType.Images).NewObject as List<MediaModel>;
                if (fileList != null)
                {
                    fileList.ForEach(x => property.PropertyMedia.Add(new PropertyMedia { NewFileName = x.NewFileName, OldFileName = x.OldFileName }));
                }
                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = true, ErrorMessage = "error" };
                }
            }
        }

        //****mchanged the viewmodel from PropertyOnboardModel to PropertyMyOnboardModel
        public static Property AddOnboardProperty(Login user, PropertyMyOnboardModel model)
        {
            using (var db = new KeysEntities())
            {
                var adModel = model.Address;
                var address = new Address
                {
                    Number = adModel.Number,
                    Street = adModel.Street,
                    City = adModel.City,
                    Suburb = adModel.Suburb ?? adModel.City,
                    PostCode = adModel.PostCode,
                    CountryId = 1,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true,
                    CreatedBy = user.Email,
                    UpdatedBy = user.Email,
                    UpdatedOn = DateTime.UtcNow,
                    Lat = adModel.Latitude,
                    Lng = adModel.Longitude,
                    Region = adModel.Region
                };
                db.Address.Add(address);
                db.SaveChanges();
                var newProperty = new Property
                {
                    AddressId = address.AddressId,
                    Name = model.PropertyName,
                    IsActive = true,
                    Bathroom = model.Bathroom,
                    Bedroom = model.Bedroom,
                    CreatedBy = user.Email,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedBy = user.Email,
                    UpdatedOn = DateTime.UtcNow,
                    Description = model.Description,
                    FloorArea = model.FloorArea,
                    LandSqm = model.LandArea,
                    ParkingSpace = model.ParkingSpace,
                    YearBuilt = model.YearBuilt,
                    PropertyTypeId = model.PropertyTypeId,
                    TargetRent = model.TenantToPropertyModel?.PaymentAmount ?? 0,
                    TargetRentTypeId = model.TargetRentType,
                    IsOwnerOccupied = model.IsOwnerOccupied
                };
                db.Property.Add(newProperty);
                var checkOwner = db.Owners.Where(x => x.Id == user.Id).FirstOrDefault();
                if (checkOwner == null)
                {
                    var newOwner = new Owners
                    {
                        Id = user.Id
                    };
                    db.Owners.Add(newOwner);
                }

                db.SaveChanges();
                var person = AccountService.GetPersonByLoginId(user.Id);

                var ownerProperty = new OwnerProperty
                {
                    PropertyId = newProperty.Id,
                    OwnershipStatusId = 1,
                    OwnerId = user.Id,
                    CreatedBy = user.Email,
                    UpdatedBy = user.Email,
                    PurchaseDate = DateTime.UtcNow, // Should Create a feild in the Add property page and get value from there
                    UpdatedOn = DateTime.UtcNow,
                    CreatedOn = DateTime.UtcNow
                };
                db.OwnerProperty.Add(ownerProperty);
                db.SaveChanges();
                return newProperty;
            }
        }
        //public static PropertyRepayment AddOnboardRepayment(Login user, List<RepaymentViewModel> model)
        //{

        //    using (var db = new KeysEntities())
        //    {

        //        try
        //        {
        //            var newRepayment = new PropertyRepayment();
        //            foreach (Service.Models.RepaymentViewModel repayment in model)
        //            {
        //                newRepayment.PropertyId = repayment.PropertyId;
        //                newRepayment.Amount = repayment.Amount;
        //                newRepayment.EndDate = repayment.EndDate;
        //                newRepayment.StartDate = repayment.StartDate;
        //                newRepayment.FrequencyType = repayment.FrequencyType;
        //                newRepayment.CreatedBy =user.UserName;
        //                newRepayment.CreatedOn = DateTime.Today;

        //                db.PropertyRepayment.Add(newRepayment);
        //                db.SaveChanges();
        //            }


        //            return newRepayment;
        //        }
        //        catch (Exception ex)
        //        {
        //            return null;
        //        }
        //    }

        //}
        //public static PropertyExpense AddOnboardExpense(Login user, List<ExpenseViewModel> model)
        //{

        //    using (var db = new KeysEntities())
        //    {
        //        try
        //        {

        //            var newExpense = new PropertyExpense();
        //            foreach (ExpenseViewModel expense in model)
        //            {
        //                newExpense.PropertyId = expense.PropertyId;
        //                newExpense.Amount = expense.Amount;
        //                newExpense.Date = expense.ExpenseDate;
        //                newExpense.Description = expense.Description; ;
        //                newExpense.CreatedBy = user.UserName;
        //                newExpense.CreatedOn = DateTime.Today;
        //                db.PropertyExpense.Add(newExpense);
        //                db.SaveChanges();
        //            }

        //            return newExpense;
        //        }
        //        catch (Exception ex)
        //        {
        //            return null;
        //        }

        //    }

        //}
        //public static PropertyFinance AddOnboardFinance(Login user, PropertyFinancialViewModel model)
        //{

        //    using (var db = new KeysEntities())
        //    {
        //        try
        //        {
        //            var newFinancial = new PropertyFinance();
        //        var newHomeValues = new PropertyHomeValue();
        //        var totalExpense = db.PropertyExpense.Where(t => t.PropertyId == model.PropertyId).Select(t => t.Amount).DefaultIfEmpty().Sum();

        //        newFinancial.CurrentHomeValue = model.CurrentHomeValue;
        //        newFinancial.PurchasePrice = model.PurchasePrice;
        //        newFinancial.Mortgage = model.Mortgage;
        //        newFinancial.PropertyId = model.PropertyId;
        //        newFinancial.PurchasePrice = model.PurchasePrice;
        //        newFinancial.TotalRepayment = model.TotalRepayment;
        //        newFinancial.TotalExpense = totalExpense;

        //        db.PropertyFinance.Add(newFinancial);

        //        // Add the homeValue to db.PropertyHomeValue with the property id into propertyHomeValue 
        //        newHomeValues.PropertyId = model.PropertyId;
        //        newHomeValues.Value = model.CurrentHomeValue ?? 0;
        //        newHomeValues.Date = DateTime.Today;
        //        newHomeValues.HomeValueTypeId = model.HomeValueType;
        //        newHomeValues.CreatedBy = user.UserName;
        //        newHomeValues.CreatedOn = DateTime.Today;
        //        db.PropertyHomeValue.Add(newHomeValues);
        //        db.SaveChanges();
        //        return newFinancial;
        //        }
        //        catch (Exception ex)
        //        {
        //            return null;
        //        }

        //    }

        //}

        public static IEnumerable<PropertyViewModel> GetOwnerProperties(Login login)
        {
            using (var db = new KeysEntities())
            {
                var propertiesId = db.OwnerProperty.Where(x => x.OwnerId == login.Id && x.Property.IsActive)
                    .Select(x => x.Property.Id);

                var properties = db.RentalApplication.Where(x => propertiesId.Contains(x.RentalListing.PropertyId))
                                .Select(x => new { Property = x.RentalListing.Property, App = x })
                                .GroupBy(x => x.Property)
                                .Select(x => new PropertyViewModel
                                {
                                    Id = x.Key.Id,
                                    PropertyTypeId = x.Key.PropertyTypeId,

                                    NewApplications = x.Count(),
                                });

                return properties;
            }
        }
        //*************************************************************************************
        public static PropertyRepayment AddOnboardRepayment(Login user, List<RepaymentViewModel> model, int PropertyId)
        {
            using (var db = new KeysEntities())
            {
                try
                {
                    var newRepayment = new PropertyRepayment();
                    foreach (Service.Models.RepaymentViewModel repayment in model)
                    {
                        newRepayment.PropertyId = PropertyId;
                        newRepayment.Amount = repayment.Amount;
                        newRepayment.EndDate = repayment.EndDate;
                        newRepayment.StartDate = repayment.StartDate;
                        newRepayment.FrequencyType = repayment.FrequencyType;
                        newRepayment.CreatedBy = user.UserName;
                        newRepayment.CreatedOn = DateTime.Today;

                        db.PropertyRepayment.Add(newRepayment);
                        db.SaveChanges();
                    }
                    return newRepayment;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public static PropertyExpense AddOnboardExpense(Login user, List<ExpenseViewModel> model, int PropertyId)
        {
            using (var db = new KeysEntities())
            {
                try
                {
                    var newExpense = new PropertyExpense();
                    foreach (ExpenseViewModel expense in model)
                    {
                        newExpense.PropertyId = PropertyId;
                        newExpense.Amount = expense.Amount;
                        newExpense.Date = expense.ExpenseDate;
                        newExpense.Description = expense.Description; ;
                        newExpense.CreatedBy = user.UserName;
                        newExpense.CreatedOn = DateTime.Today;
                        db.PropertyExpense.Add(newExpense);
                        db.SaveChanges();
                    }
                    return newExpense;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        public static PropertyFinance AddOnboardFinance(Login user, PropertyMyOnboardModel model, int PropertyId, decimal TotalRepayment)
        {
            using (var db = new KeysEntities())
            {
                try
                {
                    var newFinancial = new PropertyFinance();
                    var newHomeValues = new PropertyHomeValue();
                    var totalExpense = db.PropertyExpense.Where(t => t.PropertyId == PropertyId).Select(t => t.Amount).DefaultIfEmpty().Sum();

                    newFinancial.CurrentHomeValue = model.CurrentHomeValue;
                    newFinancial.PurchasePrice = model.PurchasePrice;
                    newFinancial.Mortgage = model.Mortgage;
                    newFinancial.PropertyId = PropertyId;
                    newFinancial.PurchasePrice = model.PurchasePrice;
                    newFinancial.TotalRepayment = TotalRepayment;
                    newFinancial.TotalExpense = totalExpense;
                    db.PropertyFinance.Add(newFinancial);

                    // Add the homeValue to db.PropertyHomeValue with the property id into propertyHomeValue 
                    newHomeValues.PropertyId = PropertyId;
                    newHomeValues.Value = model.CurrentHomeValue ?? 0;
                    newHomeValues.Date = DateTime.Today;
                    newHomeValues.HomeValueTypeId = model.HomeValueType;
                    newHomeValues.CreatedBy = user.UserName;
                    newHomeValues.CreatedOn = DateTime.Today;
                    newHomeValues.IsActive = true;
                    db.PropertyHomeValue.Add(newHomeValues);
                    db.SaveChanges();
                    return newFinancial;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

        }
        public static PropertyFinance AddPropertyFinance(Login user, FinancialModel model, int PropertyId, decimal TotalRepayment)
        {

            using (var db = new KeysEntities())
            {
                try
                {
                    //var oldFinancial = db.PropertyFinance.Where(x => x.PropertyId == PropertyId).First();
                    var newFinancial = new PropertyFinance();
                    var newHomeValues = new PropertyHomeValue();
                    var totalExpense = db.PropertyExpense.Where(t => t.PropertyId == PropertyId).Select(t => t.Amount).DefaultIfEmpty().Sum();

                    newFinancial.CurrentHomeValue = model.CurrentHomeValue;
                    newFinancial.PurchasePrice = model.PurchasePrice;
                    newFinancial.Mortgage = model.Mortgage;
                    newFinancial.PropertyId = PropertyId;
                    newFinancial.PurchasePrice = model.PurchasePrice;
                    newFinancial.TotalRepayment = TotalRepayment;
                    newFinancial.TotalExpense = totalExpense;

                    //db.SaveChanges();
                    db.PropertyFinance.Add(newFinancial);

                    // Add the homeValue to db.PropertyHomeValue with the property id into propertyHomeValue 
                    newHomeValues.PropertyId = PropertyId;
                    newHomeValues.Value = model.CurrentHomeValue ?? 0;
                    newHomeValues.Date = DateTime.Today;
                    newHomeValues.HomeValueTypeId = model.HomeValueType;
                    newHomeValues.CreatedBy = user.UserName;
                    newHomeValues.CreatedOn = DateTime.Today;
                    db.PropertyHomeValue.Add(newHomeValues);
                    db.SaveChanges();
                    return newFinancial;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public static bool IsLoginOwner(Login login)
        {
            using (var db = new KeysEntities())
            {
                return login == null ? false : db.Owners.FirstOrDefault(x => x.Id == login.Id) == null ? false : true;
            }
        }

        public static bool IsLoginPropertyManager(Login login)
        {
            using (var db = new KeysEntities())
            {
                bool result = false;
                var roles = AccountService.GetUserRolesbyEmail(login.Email);
                foreach (int role in roles){
                    if (role == 3)
                    {
                        result = true;
                    }
                }
                return login == null ? false : result;
            }
        }

        public static SearchResult GetMyRequests(POMyRequestsSearchModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var data = db.OwnerProperty.Where(x => x.OwnerId == login.Id).SelectMany(x => x.Property.PropertyRequest)
                           .Where(x => x.RequestStatusId != 4 && x.IsActive == true && x.ToTenant && !x.ToOwner)
                           .Select(x => new
                           {
                               Model = new RequestModel
                               {
                                   Id = x.Id,
                                   PropertyId = x.PropertyId,
                                   RequestStatusId = x.RequestStatus.Id,
                                   RequestMessage = x.RequestMessage,
                                   Reason = x.Reason,
                                   IsViewed = x.IsViewed,
                                   MediaFiles = x.PropertyRequestMedia.Select(y => new MediaModel
                                   {
                                       Data = y.NewFileName,
                                       Id = y.Id,
                                       NewFileName = y.NewFileName,
                                       OldFileName = y.OldFileName,
                                       Status = "load"
                                   }).ToList(),
                               },
                               RequestType = x.RequestType.Name,
                               RequestStatus = x.RequestStatus.Name,
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
                               PropertyAddress = x.Property.Address.Number.Replace(" ", "") + " " + x.Property.Address.Street.Trim() + " " + x.Property.Address.City.Trim() + " " + x.Property.Address.Suburb.Trim() + " " + x.Property.Address.PostCode.Replace(" ", ""),
                               CreatedOn = x.CreatedOn,
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
                    case "Latest First":
                        data = data.OrderByDescending(s => s.CreatedOn);
                        break;
                    case "Earliest First":
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
                            data = data.Where(x => x.Model.RequestMessage.ToLower().EndsWith(formatString)
                                                || x.PropertyAddress.ToLower().EndsWith(formatString));
                            break;
                        case 2:
                            data = data.Where(x => x.Model.RequestMessage.ToLower().StartsWith(formatString)
                                                || x.PropertyAddress.ToLower().StartsWith(formatString));
                            break;
                        case 3:
                            data = data.Where(x => x.Model.RequestMessage.ToLower().Contains(formatString)
                                                || x.PropertyAddress.ToLower().Contains(formatString));
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

        public static bool IsUserOwner(string userName)
        {
            using (var db = new KeysEntities())
            {
                var login = db.Login.FirstOrDefault(x => x.Email == userName && x.IsActive);
                return login == null ? false : db.LoginRole.FirstOrDefault(x => x.PersonId == login.Id && x.RoleId == 4) == null ? false : true;
            }
        }

        public static async Task<ServiceResponseResult> AcceptTenantRequest(int requestId, Login login)
        {
            using (var db = new KeysEntities())
            {
                var req = db.PropertyRequest.FirstOrDefault(x => x.Id == requestId);
                var addr = req.Property.Address;
                var perId = req.RecipientId;
                var suc = false;
                string errorMessage = error;
                if (req == null) return new ServiceResponseResult { IsSuccess = suc, ErrorMessage = "Can not find request!" };
                var statusId = req.RequestStatusId;
                if (statusId == 2 || statusId == 4 || statusId == 5) return new ServiceResponseResult { IsSuccess = suc, ErrorMessage = "Can not update request!" };
                req.RequestStatusId = 2;
                try
                {
                    db.SaveChanges();
                    suc = true;
                    errorMessage = "Request accepted successfully!";
                    if (perId.HasValue)
                    {
                        var ten = db.Person.FirstOrDefault( x => x.Id == perId);
                        var tenLog = ten.Login;
                        await SendAcceptRequestEmail(ten.FirstName, tenLog.UserName, addr.ToAddressString());
                    }
                    
                    return new ServiceResponseResult { IsSuccess = suc, ErrorMessage = errorMessage };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = error };
                }
            }
        }

        public static async Task SendAcceptRequestEmail(string name, string email, string add)
        {

            string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "Tenants/Home/Requests?RequestStatus=2");
            SendGridEmailModel mail = new SendGridEmailModel
            {
                RecipentName = name,
                ButtonText = "",
                ButtonUrl = url,
                RecipentEmail = email,
                Address = add,
                PersonType = "land lord"
            };
            await EmailService.SendEmailWithSendGrid(EmailType.AcceptRequestEmail, mail);
        }

        public static IEnumerable<PaymentTracking> GetRenalPaymentTracking(Login login)
        {
            using (var db = new KeysEntities())
            {
                var paymentTrackings = db.RentalPaymentTracking
                        .Select(x => new PaymentTracking
                        {
                            Id = x.Id,
                            Address = x.TenantProperty.Property.Address,
                            TenantName = x.TenantProperty.Tenant.Person.FirstName,
                            DueDate = x.DueDate,
                            PaymentComplete = x.PaymentComplete
                        }).ToList();
                paymentTrackings.ForEach( x => { x.AddressString = x.Address.ToAddressString(); x.Address = null; });
                return paymentTrackings;
            }
        }

    }
}
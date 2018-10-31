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
    public static class CompanyService
    {
        static string _error = "Something went wrong please try again later!";

        public static SearchResult GetJobs(SSMyJobsSearchModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var data = db.Job.Where(x => (x.JobStatusId == 2 || x.JobStatusId == 3 || x.JobStatusId == 4) && x.ProviderId == login.Id)
                    .Select(x => new JobViewModel
                    {
                        Model = new JobModel
                        {
                            Id = x.Id,
                            PropertyId = x.PropertyId,
                            ProviderId = x.ProviderId,
                            OwnerId = x.OwnerId,
                            JobStatusId = x.JobStatusId,
                            JobRequestId = x.JobRequestId,
                            JobStartDate = x.JobStartDate,
                            JobEndDate = x.JobEndDate,
                            JobDescription = x.JobDescription,
                            Note = x.Note,
                            AcceptedQuote = x.AcceptedQuote,
                            PercentDone = x.PercentDone??0,
                            OwnerUpdate = x.OwnerUpdate ?? false,
                            ServiceUpdate = x.ServiceUpdate ?? false,
                            MediaFiles = x.JobMedia.Select(y => new MediaModel { Id = y.Id, NewFileName = y.NewFileName, OldFileName = y.OldFileName, Status = "load" }).ToList(),
                        },
                        CreatedOn = x.CreatedOn,
                        JobStatusName = x.ServiceProviderJobStatus.Name,
                        ProviderName = x.ServiceProvider.Person.FirstName + " " + x.ServiceProvider.Person.LastName,
                        ProviderCompany = x.ServiceProvider.Company.Name,
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
                        PropertyAddress = x.Property.Address.Number + " " + x.Property.Address.Street + " " + x.Property.Address.Suburb + " " + x.Property.Address.City + " " + x.Property.Address.PostCode,
                    });
                var allItems = data.OrderByDescending(x => x.CreatedOn).ToPagedList(model.Page, 10);
                switch (model.SortOrder)
                {
                    case "Lowest Progress":
                        data = data.OrderBy(s => s.Model.PercentDone);
                        break;
                    case "Highest Progress":
                        data = data.OrderByDescending(s => s.Model.PercentDone);
                        break;
                    case "Highest Accepted Quote":
                        data = data.OrderByDescending(s => s.Model.AcceptedQuote);
                        break;
                    case "Lowest Accepted Quote":
                        data = data.OrderBy(s => s.Model.AcceptedQuote);
                        break;
                    case "Latest Jobs":
                        data = data.OrderByDescending(s => s.CreatedOn);
                        break;
                    case "Earliest Jobs":
                        data = data.OrderBy(s => s.CreatedOn);
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
                            data = data.Where(job => job.PropertyAddress.ToLower().StartsWith(formatString)
                                           || job.ProviderName.ToLower().StartsWith(formatString)
                                           || job.Model.AcceptedQuote.ToString().ToLower().StartsWith(formatString)
                                           || job.Model.JobDescription.ToLower().StartsWith(formatString)
                                           || job.JobStatusName.ToLower().StartsWith(formatString));
                            break;
                        case 2:
                            data = data.Where(job => job.PropertyAddress.ToLower().EndsWith(formatString)
                                           || job.ProviderName.ToLower().EndsWith(formatString)
                                           || job.Model.AcceptedQuote.ToString().ToLower().EndsWith(formatString)
                                           || job.Model.JobDescription.ToLower().EndsWith(formatString)
                                           || job.JobStatusName.ToLower().EndsWith(formatString));
                            break;
                        case 3:
                            data = data.Where(job => job.PropertyAddress.ToLower().Contains(formatString)
                                           || job.ProviderName.ToLower().Contains(formatString)
                                           || job.Model.AcceptedQuote.ToString().ToLower().Contains(formatString)
                                           || job.Model.JobDescription.ToLower().Contains(formatString)
                                           || job.JobStatusName.ToLower().Contains(formatString));
                            break;
                    }
                }
                var items = data.ToPagedList(model.Page, 10);
                var count = items.Count;
                items = count == 0 ? allItems : items;
                items.ToList().ForEach(x => x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties()));
                var result = new SearchResult { SearchCount = items.Count, Items = count == 0 ? allItems : items };
                return result;
            }
        }
        public static SearchResult GetJobQuotes(QuotesSearchViewModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var data = db.JobQuote.Where(x => x.ProviderId == login.Id)
                    .Select(q => new
                    {
                        Model = new JobQuoteModel{
                            Id = q.Id,
                            Amount = q.Amount,
                            Note = q.Note,
                            JobRequestId = q.JobRequestId,
                            MediaFiles = q.JobQuoteMedia.Select(y => new MediaModel { Id = y.Id, OldFileName = y.FileName.Substring(36), NewFileName = y.FileName }).ToList(),
                        },
                        JobDescription = q.TenantJobRequest.JobDescription,
                        CreatedOn = q.CreatedOn,
                        PropertyAddress = q.TenantJobRequest.Property.Address.Number + " " + q.TenantJobRequest.Property.Address.Street + ", " + q.TenantJobRequest.Property.Address.Suburb + ", " + q.TenantJobRequest.Property.Address.City + " - " + q.TenantJobRequest.Property.Address.PostCode,
                        Address = new AddressViewModel
                                {
                                    AddressId = q.TenantJobRequest.Property.Address.AddressId,
                                    CountryId = q.TenantJobRequest.Property.Address.CountryId,
                                    Number = q.TenantJobRequest.Property.Address.Number.Replace(" ", ""),
                                    Street = q.TenantJobRequest.Property.Address.Street.Trim(),
                                    City = q.TenantJobRequest.Property.Address.City.Trim(),
                                    Suburb = q.TenantJobRequest.Property.Address.Suburb.Trim() ?? "",
                                    PostCode = q.TenantJobRequest.Property.Address.PostCode.Replace(" ", "")
                                },
                        Status = q.Status
                    });
                if (!String.IsNullOrWhiteSpace(model.Status))
                {
                    data = data.Where( x => x.Status.ToLower() == model.Status);
                }
                var allItems = data.OrderByDescending(x => x.CreatedOn).ToPagedList(model.Page, 10);
                switch (model.SortOrder)
                {
                    case "Lowest Amount":
                        data = data.OrderBy(s => s.Model.Amount);
                        break;
                    case "Highest Amount":
                        data = data.OrderByDescending(s => s.Model.Amount);
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
                if (!String.IsNullOrWhiteSpace(model.SearchString))
                {
                    SearchUtil searchTool = new SearchUtil();
                    int searchType = searchTool.CheckDisplayType(model.SearchString);
                    string formatString = searchTool.ConvertString(model.SearchString);
                    decimal number;
                    var canParse = Decimal.TryParse(formatString, out number);
                    switch (searchType)
                    {
                        case 1:
                            data = data.Where(x => x.PropertyAddress.ToLower().EndsWith(formatString) || (x.Status ?? "").ToLower().EndsWith(formatString) || (x.JobDescription.ToLower().EndsWith(formatString)) || (x.Model.Note.ToLower().EndsWith(formatString)));
                            if (canParse)
                            {
                                data = data.Where(x => x.Model.Amount.Equals(number));
                            }
                            break;
                        case 2:
                            data = data.Where(x => x.PropertyAddress.ToLower() == formatString || (x.Status ?? "").ToLower().StartsWith(formatString) || (x.JobDescription.ToLower().StartsWith(formatString)) || (x.Model.Note.ToLower().StartsWith(formatString)));
                            if (canParse)
                            {
                                data = data.Where(x => x.Model.Amount.Equals(number));
                            }
                            break;
                        case 3:

                           // data = data.Where(x => x.PropertyAddress.ToLower().Contains(formatString) || (x.Status ?? "").ToLower().Contains(formatString) || (x.JobDescription.ToLower().Contains(formatString)) || (x.Model.Note.ToLower().Contains(formatString)));
                            if (canParse)
                            {
                                data = data.Where(x => x.Model.Amount.Equals(number));
                            }
                            else
                            {
                                data = data.Where(x => x.PropertyAddress.ToLower().Contains(formatString) || (x.Status ?? "").ToLower().Contains(formatString) || (x.JobDescription.ToLower().Contains(formatString)) || (x.Model.Note.ToLower().Contains(formatString)));
                            }
                            break;
                    }
                }
                var items = data.ToPagedList(model.Page, 10);
                var count = items.Count;
                items = count == 0 ? allItems : items;
                items.ToList().ForEach(x => x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties()));
                var result = new SearchResult { SearchCount = items.Count, Items = count == 0 ? allItems : items };
                return result;
            }
        }
        public static ServiceResponseResult AddNewCompany(CompanyViewModel model, Login login, HttpFileCollectionBase files)
        {
            using (var db = new KeysEntities())
            {
                var foundSp = db.ServiceProvider.FirstOrDefault(x => x.Id == login.Id);
                if (foundSp == null)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "Can not find account!" };
                }
                var physicalAddress = new Address
                {
                    CountryId = 1,
                    CreatedBy = login.Email,
                    CreatedOn = DateTime.Now,
                    UpdatedBy = login.Email,
                    UpdatedOn = DateTime.Now,
                    Number = model.PhysicalAddress.Number,
                    Street = model.PhysicalAddress.Street,
                    City = model.PhysicalAddress.City,
                    Suburb = model.PhysicalAddress.Suburb,
                    PostCode = model.PhysicalAddress.PostCode,
                    Lat = model.PhysicalAddress.Latitude,
                    Lng = model.PhysicalAddress.Longitude,
                    IsActive = true
                };
                try
                {
                    db.Address.Add(physicalAddress);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
                var serviceProvider = new Company
                {
                    IsActive = true,
                    Name = model.Name,
                    Website = model.Website,
                    PhoneNumber = model.PhoneNumber,
                    UpdatedBy = login.Email,
                    CreatedOn = DateTime.Now,
                    CreatedBy = login.Email,
                    UpdatedOn = DateTime.Now,
                    PhysicalAddressId = physicalAddress.AddressId,
                };
                if (model.IsShipSame)
                {
                    serviceProvider.BillingAddressId = physicalAddress.AddressId;
                }
                else
                {
                    var billingAddress = new Address
                    {
                        CountryId = 1,
                        CreatedBy = login.Email,
                        CreatedOn = DateTime.Now,
                        UpdatedBy = login.Email,
                        UpdatedOn = DateTime.Now,
                        Number = model.BillingAddress.Number,
                        Street = model.BillingAddress.Street,
                        City = model.BillingAddress.City,
                        Suburb = model.BillingAddress.Suburb,
                        PostCode = model.BillingAddress.PostCode,
                        Lat = model.PhysicalAddress.Latitude,
                        Lng = model.PhysicalAddress.Longitude,
                        IsActive = true
                    };
                    try
                    {
                        db.Address.Add(billingAddress);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                    }
                    serviceProvider.BillingAddressId = billingAddress.AddressId;

                }
                var saveResult = MediaService.SaveMediaFiles(files, 1);
                if (saveResult.IsSuccess)
                {
                    var newOb = saveResult.NewObject as List<MediaModel>;
                    serviceProvider.ProfilePhoto = newOb.FirstOrDefault().NewFileName;
                }

                foundSp.Company = serviceProvider;
                try
                {
                    db.Company.Add(serviceProvider);
                    foundSp.Company = serviceProvider;
                    foundSp.IsProfileComplete = true;
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }
        public static bool IsUserServiceSupplier(string userName)
        {
            using (var db = new KeysEntities())
            {
                var login = db.Login.FirstOrDefault(x => x.Email == userName && x.IsActive);
                return login == null ? false : db.LoginRole.FirstOrDefault(x => x.PersonId == login.Id && x.RoleId == 6) == null ? false : true;
            }
        }
        public static bool IsServiceSupplier(Login login)
        {
            using (var db = new KeysEntities())
            {
                if (login != null)
                {
                    return db.ServiceProvider.FirstOrDefault(x => x.Id == login.Id) == null ? false : true;
                }
                else
                {
                    return false;
                }
            }
        }
        public static bool IsProfileComplete(Login login)
        {
            using (var db = new KeysEntities())
            {
                return db.ServiceProvider.FirstOrDefault(x => x.Id == login.Id)?.IsProfileComplete ?? false;
            }
        }

        public static ServiceResponseResult EditMarketJobQuote(JobQuoteModel quoteModel, Login login, HttpFileCollectionBase files)
        {
            var result = new ServiceResponseResult { IsSuccess = false };
            using (var db = new KeysEntities())
            {
                var editJobQuote = db.JobQuote.Where(x => x.Id == quoteModel.Id).FirstOrDefault();
                editJobQuote.Id = quoteModel.Id;
                editJobQuote.Note = quoteModel.Note;
                editJobQuote.UpdatedOn = DateTime.Now;

                if(quoteModel.FilesRemoved != null)
                {
                    quoteModel.FilesRemoved.ToList().ForEach(x =>
                    {
                        var media = db.JobQuoteMedia.FirstOrDefault(y => y.Id == x);
                        if (media != null)
                        {
                            db.JobQuoteMedia.Remove(media);
                            MediaService.RemoveMediaFile(media.FileName);
                        }
                    });
                }
            
                var mediaFiles = MediaService.SaveFiles(files, 5- editJobQuote.JobQuoteMedia.Count, AllowedFileType.AllFiles).NewObject as List<MediaModel>;
                if (mediaFiles != null)
                {
                    mediaFiles.ForEach(x => editJobQuote.JobQuoteMedia.Add(new JobQuoteMedia
                    {
                        FileName = x.NewFileName,
                        IsActive = true
                    }));
                }

                try {
                    db.SaveChanges();                        
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception ex) {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }

        public static ServiceResponseResult RemoveFromWatchlist(int marketJobId, Login login)
        {
            using (var db = new KeysEntities())
            {
                var result = new ServiceResponseResult { IsSuccess = false };
                var item = db.JobWatchList.FirstOrDefault(x => x.PersonId == login.Id && x.MarketJobId == marketJobId);
                if (item == null)
                {
                    result.ErrorMessage = "Item not found!";
                    return result;
                }
                db.JobWatchList.Remove(item);
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

using KeysPlus.Data;
using KeysPlus.Service.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Web.Mvc;
using PagedList;


namespace KeysPlus.Service.Services
{
    public static class JobService
    {
        const string _error = "Please try again later.";

        
        public static IEnumerable<TenantJobRequest> GetOwnerMarketJobs(Login login)
        {
            using (var db = new KeysEntities())
            {
                return db.TenantJobRequest.Where(x => x.JobStatusId == 1 && x.OwnerId == login.Id);
            }
        }

        public static SearchResult AdvancedSearchMarketJob(AdvancedMarketJobSearchModel model,string sortOder)
        {
            using (var db = new KeysEntities())
            {
                var allJobRequests = db.TenantJobRequest.Where(x => x.JobStatusId == 1);
                var data = allJobRequests.Select(x => new JobMarketModel 
                {
                        Id = x.Id,
                        PropertyId = x.PropertyId,
                        Title = x.Title,
                        JobDescription = x.JobDescription,
                        MaxBudget = x.MaxBudget,
                        MediaFiles = x.TenantJobRequestMedia.Select(y => new MediaModel
                        {
                            Data = y.NewFileName,
                            Id = y.Id,
                            NewFileName = y.NewFileName,
                            OldFileName = y.OldFileName,
                            Status = "load",
                        }).ToList(),
                    //},
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
                    PropertyAddress = x.Property.Address.Number.ToString() + " " + x.Property.Address.Street.ToString() + " " + x.Property.Address.Suburb.ToString() + " " + x.Property.Address.City.ToString(),
                    CreatedOn = x.CreatedOn,
                }).AsQueryable();
                
                var allItems = data.OrderByDescending(x => x.CreatedOn).ToPagedList(model.Page, 10);
                allItems.ToList().ForEach(x => x.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties()));
                // code for search and filtering should be here
                switch (sortOder)
                {
                    case "JobDesc":
                        data = data.OrderBy(s => s.JobDescription);
                        break;
                    case "JobDesc_desc":
                        data = data.OrderBy(s => s.JobDescription);
                        break;
                    case "Date":
                        data = data.OrderBy(s => s.CreatedOn);
                        break;
                    case "Date_desc":
                        data = data.OrderByDescending(s => s.CreatedOn);
                        //data = data.OrderByDescending(s => DateTime.Parse(s.PostedDate));

                        break;
                    case "MaxBudget":
                        data = data.OrderBy(s => s.MaxBudget);
                        break;
                    case "maxBudget_desc":
                        data = data.OrderByDescending(s => s.MaxBudget);
                        break;
                    case "Title":
                        data = data.OrderBy(s => s.Title);
                        break;
                    case "Title_Desc":
                        data = data.OrderByDescending(s => s.Title);
                        break;
                    default:
                        data = data.OrderByDescending(s => s.CreatedOn);
                        break;
                }
                if (model.SuburbList.Count > 0)
                {
                    data = from r in data
                           where model.SuburbList.Contains(r.Address.Suburb)
                           select r;
                }
                data = from r in data
                       where ((r.MaxBudget >= (model.MinBudget ?? r.MaxBudget)) && (r.MaxBudget <= (model.MaxBudget ?? r.MaxBudget)))
                       select r;
                var items = data.OrderBy(x=>x.CreatedOn).ToPagedList(model.Page, 10);
                items.ToList().ForEach(x => x.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties()));
                var count = items.Count; 
                var result = new SearchResult { SearchCount = items.Count, Items = count == 0 ? allItems : items };
                return result;
            }
        }
        public static SearchResult GetAllMarketJobs(MarketJobSearchModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var data = db.TenantJobRequest.Where(x => x.JobStatusId == 1);
                var ownerJobRequests = data.Where(x => x.OwnerId == login.Id);
                var ownerJobRequestsId = ownerJobRequests.Select(x => x.Id);
                if (model.IsOwnerView)
                {
                    data = ownerJobRequests;
                }
                var result = data.Select(x => new MarketJobViewModel
                {
                    Model = new MarketJobModel
                    {
                        Id = x.Id,
                        PropertyId = x.PropertyId,
                        Title = x.Title,
                        JobDescription = x.JobDescription,
                        MaxBudget = x.MaxBudget,
                        MediaFiles = x.TenantJobRequestMedia.Select(y => new MediaModel
                        {
                            Data = y.NewFileName,
                            Id = y.Id,
                            NewFileName = y.NewFileName,
                            OldFileName = y.OldFileName,
                            Status = "load",
                        }).ToList()
                    },
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
                        Longitude = x.Property.Address.Lng
                    },
                    PropertyAddress = x.Property.Address.Number + " " + x.Property.Address.Street + ", " + x.Property.Address.Suburb + ", " + x.Property.Address.City,
                    IsOwnedByUser = model.IsOwnerView ? model.IsOwnerView : x.OwnerId == login.Id,
                    IsApplyByUser = db.JobQuote.Any(y => y.JobRequestId == x.Id && y.ProviderId == login.Id && y.Status.ToLower() == "opening"),
                    NewQuotesCount = x.JobQuote.Where(y => y.Status.ToLower() == "opening" && !(y.IsViewed ?? false)).Count(),
                    CreatedOn = x.CreatedOn,
                });
                var allItems = model.IsOwnerView ? result.OrderByDescending(x => x.NewQuotesCount).ToPagedList(model.Page, 10) 
                    : result.OrderByDescending(x => x.CreatedOn).ToPagedList(model.Page, 10);
                switch (model.SortOrder)
                {
                    case "Title":
                        result = result.OrderBy(s => s.Model.Title);
                        break;
                    case "Title(Desc)":
                        result = result.OrderByDescending(s => s.Model.Title);
                        break;
                    case "Highest Budget":
                        result = result.OrderByDescending(s => s.Model.MaxBudget);
                        break;
                    case "Lowest Budget":
                        result = result.OrderBy(s => s.Model.MaxBudget);
                        break;
                    case "Earliest Listing":
                        result = result.OrderBy(s => s.CreatedOn);
                        break;
                    case "Latest Listing":
                        result = result.OrderByDescending(s => s.CreatedOn);
                        break;
                    case "Highest New Quotes":
                        result = result.OrderByDescending(x => x.NewQuotesCount);
                        break;
                    case "Lowest New Quotes":
                        result = result.OrderBy(x => x.NewQuotesCount);
                        break;
                    default:
                        result = model.IsOwnerView? result.OrderByDescending(x => x.NewQuotesCount) : result.OrderByDescending(s => s.CreatedOn);
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
                            result = result.Where(job => job.Model.MaxBudget.ToString() == formatString
                                           || job.PropertyAddress.ToLower().EndsWith(formatString)
                                          || (job.Model.Title ?? "").ToLower().EndsWith(formatString)
                                          || job.Model.JobDescription.ToLower().EndsWith(formatString));
                            break;
                        case 2:

                            result = result.Where(job => job.Model.MaxBudget.ToString() == formatString
                                         || job.PropertyAddress.ToLower().StartsWith(formatString)
                                           || (job.Model.Title ?? "").ToLower().StartsWith(formatString)
                                           || job.Model.JobDescription.ToLower().StartsWith(formatString));
                            break;
                        case 3:
                            result = result.Where(job => job.Model.MaxBudget.ToString() == formatString
                                          || job.PropertyAddress.ToLower().Contains(formatString)
                                            || (job.Model.Title ?? "").ToLower().Contains(formatString)
                                            || job.Model.JobDescription.ToLower().Contains(formatString));
                            break;
                    }
                }
                var items = result.ToPagedList(model.Page, 10);
                var count = items.Count;
                items = count == 0 ? allItems : items;
                items.ToList().ForEach(x => {
                    x.Model.MediaFiles.ForEach(y => y.InjectMediaModelViewProperties());
                    x.IsInWatchlist = db.JobWatchList.FirstOrDefault(y => y.PersonId == login.Id & y.MarketJobId == x.Model.Id && y.IsActive) != null;
                                    
                });
                var searchResult = new SearchResult { SearchCount = items.Count, Items = count == 0 ? allItems : items };
                return searchResult;
            }
        }

        public static ServiceResponseResult AddJobQuote(QuoteModel model, Login login, HttpFileCollectionBase files = null)
        {
            using (var db = new KeysEntities())
            {
                var _currentJobId = model.JobRequestId;
                var _currentJobMaxBudget = db.TenantJobRequest.SingleOrDefault(j => j.Id == _currentJobId).MaxBudget;
                if (_currentJobMaxBudget != null && _currentJobMaxBudget < model.Amount)
                {
                    return new ServiceResponseResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Sorry, you quote is larger than Max Budget of this Job."
                    };
                }
                if (model.Id == -1)
                {
                    var jobQuote = new JobQuote();
                    var mf = MediaService.SaveMediaFiles(files, 5);
                    if (mf.IsSuccess)
                    {
                        var mediaFiles = mf.NewObject as List<MediaModel>;
                        if (mediaFiles != null)
                        {
                            mediaFiles.ForEach(x => jobQuote.JobQuoteMedia.Add(new JobQuoteMedia
                            {
                                FileName = x.NewFileName,
                                IsActive = true
                            }));
                        }
                    }
                    // Bug Fix #2031
                    jobQuote.JobRequestId = model.JobRequestId;
                    jobQuote.ProviderId = login.Id;
                    jobQuote.Status = "opening".ToLower();
                    jobQuote.Amount = model.Amount;
                    jobQuote.CreatedBy = login.Email;
                    jobQuote.UpdatedBy = login.Email;
                    jobQuote.CreatedOn = DateTime.UtcNow;
                    jobQuote.UpdatedOn = DateTime.UtcNow;
                    jobQuote.Note = model.Note;
                    jobQuote.IsViewed = false;
                    db.JobQuote.Add(jobQuote);

                }
                else
                {
                    var jobQuote = db.JobQuote.Find(model.Id);
                    var marketJob = db.TenantJobRequest.Find(model.JobRequestId);

                    if (jobQuote != null && marketJob != null)
                    {
                        jobQuote.Amount = model.Amount;
                        jobQuote.UpdatedBy = login.Email;
                        jobQuote.UpdatedOn = DateTime.UtcNow;
                    }
                }
                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception)
                {

                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }

        public static ServiceResponseResult RemoveMarketJob(int morketJobId)
        {
            var result = new ServiceResponseResult { IsSuccess = false };
            using (var db = new KeysEntities())
            {
                var deletemorketJob = db.TenantJobRequest.Where(x => x.Id == morketJobId).First();
                if (deletemorketJob == null)
                {
                    var errorMsg = "Cannot locate the Job in the MArket Place";
                    result.ErrorMessage = errorMsg;
                    return result;
                }
                else
                {

                    deletemorketJob.JobStatusId = 3;
                    var Quotes = db.JobQuote.Where(x => x.JobRequestId == deletemorketJob.Id).ToList();
                    Quotes.ForEach(x => x.Status = "unsuccessful");
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

        public static ServiceResponseResult EditMarketJob(MarketJobModel model, Login login, HttpFileCollectionBase files = null)
        {
            var result = new ServiceResponseResult { IsSuccess = false };
            using (var db = new KeysEntities())
            {
                var files1 = model.MediaFiles;
                var editMarketJob = db.TenantJobRequest.Where(x => x.Id == model.Id).FirstOrDefault();
                if (editMarketJob == null)
                {
                    var errorMsg = "Cannot locate the Job in the Market Place";
                    result.ErrorMessage = errorMsg;
                    return result;
                }
                else
                {
                    editMarketJob.JobDescription = model.JobDescription;
                    editMarketJob.MaxBudget = model.MaxBudget;
                    if (model.FilesRemoved != null)
                    {
                        model.FilesRemoved.ToList().ForEach(x =>
                        {

                            var media = db.TenantJobRequestMedia.FirstOrDefault(y => y.Id == x);
                            if (media != null)
                            {
                                db.TenantJobRequestMedia.Remove(media);
                                MediaService.RemoveMediaFile(media.NewFileName);
                            }

                        });
                    }
                    var fileList = MediaService.SaveFiles(files, 5 - editMarketJob.TenantJobRequestMedia.Count(), AllowedFileType.Images).NewObject as List<MediaModel>;
                    if (fileList != null)
                    {
                        fileList.ForEach(x => editMarketJob.TenantJobRequestMedia.Add(new TenantJobRequestMedia { NewFileName = x.NewFileName, OldFileName = x.OldFileName }));
                    }
                }
                try
                {
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                }
                catch (Exception)
                {

                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }

            }

        }
        public static ServiceResponseResult AddMarketJobMedia(HttpFileCollectionBase files, int appId, string serverPath)
        {
            var allowedFiles = 5;
            if (files.Count > allowedFiles)
            {
                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = $"You can't add more than {allowedFiles} photos" };
            };
            if (files != null && files.Count > 0)
            {
                var numberOfFiles = files.Count < allowedFiles ? files.Count : allowedFiles;
                List<string> acceptedExtensions = new List<string> { ".pdf", ".doc", ".docx", ".jpg", ".png", ".gif", ".jpeg" };
                using (var db = new KeysEntities())
                {
                    for (int i = 0; i < numberOfFiles; i++)
                    {
                        var file = files[i];
                        var fileExtension = Path.GetExtension(file.FileName);
                        if (fileExtension != null && !acceptedExtensions.Contains(fileExtension.ToLower()))
                        {
                            return new ServiceResponseResult { IsSuccess = false, ErrorMessage = $"Supported file types are *.pdf, *.doc*, *.jpg, *.png, *.gif, *.jpeg" };
                        }
                        else
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var index = fileName.LastIndexOf(".");
                            var newFileName = fileName.Insert(index, $"{Guid.NewGuid()}");
                            var physicalPath = Path.Combine(serverPath, newFileName);
                            db.TenantJobRequestMedia.Add(new TenantJobRequestMedia
                            {
                                TenantJobRequestId = appId,
                                NewFileName = newFileName,
                                OldFileName = fileName,
                            });
                            file.SaveAs(physicalPath);
                        }
                    }         
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
            else
            {
                return new ServiceResponseResult { IsSuccess = true, ErrorMessage = "You have not specified a file." }; //Bug #2031(Part 3) : Uploading a file is optional
            }
        }

        public static ServiceResponseResult AddJob(JobModel model, HttpFileCollectionBase files, Login login)
        {
            var result = new ServiceResponseResult { IsSuccess = false };
            using (var db = new KeysEntities())
            {
                var newJob = new Job();
                var foundMarketJob = db.TenantJobRequest.Where(x => x.Id == model.Id).First();
                if (foundMarketJob == null)
                {
                    var errorMsg = "Cannot locate the Job in the Market Place";
                    result.ErrorMessage = errorMsg;
                    return result;
                }
                else
                {
                    foundMarketJob.JobStatusId = 3;
                    newJob.JobDescription = model.JobDescription;
                    newJob.PropertyId = model.PropertyId;
                    newJob.JobStatusId = 2; // should be pending
                    newJob.CreatedBy = login.Email;
                    newJob.CreatedOn = DateTime.UtcNow;
                    newJob.UpdatedOn = DateTime.UtcNow;
                    newJob.UpdatedBy = login.Email;
                    newJob.PaymentAmount = 0;
                    newJob.OwnerId = login.Id;
                    newJob.JobRequestId = model.Id;
                    newJob.PaymentAmount = 0;
                    db.Job.Add(newJob);
                };
                var mediaFiles = MediaService.SaveFiles(files, 5, AllowedFileType.AllFiles).NewObject as List<MediaModel>;
                mediaFiles.ForEach( x => { newJob.JobMedia.Add(new JobMedia { NewFileName = x.NewFileName, OldFileName = x.OldFileName }); });
                try
                {
                    db.SaveChanges();
                    //var jobRequestMedia = db.TenantJobRequestMedia.Where(x => x.TenantJobRequestId == model.Id).ToList();
                    //foreach (var mediaFile in jobRequestMedia)
                    //{
                    //    var newJobMedia = new JobMedia();
                    //    newJobMedia.JobId = newJob.Id;
                    //    newJobMedia.NewFileName = mediaFile.NewFileName;
                    //    newJobMedia.OldFileName = mediaFile.OldFileName;
                    //    newJobMedia.PropertyId = model.PropertyId;
                    //    newJobMedia.IsActive = true;
                    //    db.JobMedia.Add(newJobMedia);
                    //    db.SaveChanges();
                    //}
                    return new ServiceResponseResult { IsSuccess = true };

                }
                catch (Exception)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }

        public static Login GetLoginByJobQuoteId(JobAcceptedModel jobModel)
        {
            throw new NotImplementedException();
        }

        public static Person GetPersonByJobQuoteId(JobAcceptedModel jobModel)
        {
            using (var db = new KeysEntities())
            {
                var ownerId = db.JobQuote.Where(x => x.Id == jobModel.QuoteId).Select(y => y.ProviderId).FirstOrDefault();
                return db.Person.FirstOrDefault(x => x.Id == ownerId);
            }
        }

        public static int GetNewQuotesCount(Login login)
        {
            using (var db = new KeysEntities())
            {
                if (login == null)
                {
                    return 0;
                }
                var marketJobs = db.TenantJobRequest.Where(x => x.OwnerId == login.Id && x.JobStatusId == 1).ToList();

                var count = marketJobs.Sum(x => x.JobQuote.Where(y => !(y.IsViewed ?? false))?.Count() ?? 0);
                return count;
            }
        }

        public static ServiceResponseResult QuoteViewed(int quoteId)
        {
            using (var db = new KeysEntities())
            {
                var quote = db.JobQuote.FirstOrDefault(x => x.Id == quoteId);
                if (quote == null)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "Can note find quote" };
                }

                if (quote.IsViewed ?? false)
                {
                    return new ServiceResponseResult { IsSuccess = false };
                }
                quote.IsViewed = true;
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
        public static ServiceResponseResult AcceptQuote(JobAcceptedModel model, Login login)
        {
            using (var db = new KeysEntities())
            {
                var quote = db.JobQuote.FirstOrDefault(x => x.Id == model.QuoteId);
                if (quote == null)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "Can note find quote" };
                }
                quote.IsViewed = true;
                quote.Status = "accepted";
                var job = new Job
                {
                    ProviderId = quote.ProviderId,
                    PropertyId = quote.TenantJobRequest.PropertyId,
                    AcceptedQuote = quote.Amount,
                    JobDescription = model.JobDescription,
                    JobRequestId = model.JobRequestId,
                    JobStatusId = 2,
                    UpdatedBy = login.Email,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    CreatedBy = login.Email,
                    OwnerId = login.Id
                };
                db.Job.Add(job);
                var marketJob = db.TenantJobRequest.FirstOrDefault(x => x.Id == model.JobRequestId);

                if (marketJob == null)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "Can not find job!" };
                }
                marketJob.JobStatusId = 5;
                var otherQuotes = db.JobQuote.Where(x => x.JobRequestId == model.JobRequestId && x.Id != model.QuoteId).ToList();
                otherQuotes.ForEach(x => x.Status = "unsuccessful");
                try
                {
                    db.SaveChanges();

                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
                foreach (var item in otherQuotes)
                {
                    var nvc = new NameValueCollection();
                    nvc.Add("status", "unsuccessful");


                    string url = UtilService.UrlGenerator(System.Web.HttpContext.Current.Request, "/Companies/Manage/MyQuotes", UtilService.ToQueryString(nvc));
                    SendGridEmailModel mail = new SendGridEmailModel
                    {
                        RecipentName = item.ServiceProvider.Company.Name,
                        ButtonText = "",
                        ButtonUrl = url,
                        RecipentEmail = item.ServiceProvider.Company.CreatedBy,
                        JobTitle = item.TenantJobRequest.JobDescription ?? "No Description",
                    };
                    EmailService.SendEmailWithSendGrid(EmailType.DeclineQuote, mail);
                    }
                return new ServiceResponseResult { IsSuccess = true };
            }
        }

        public static IQueryable<JobQuote> GetJobQuotesByMarketJoaddbId(int marketJobId)
        {
            using (var db = new KeysEntities())
            {
                return db.JobQuote.Where(x => x.JobRequestId == marketJobId);
            }
        }

        public static IEnumerable<RequestStatus> GetAllRequestStatus()
        {
            using (var db = new KeysEntities())
            {
                return db.RequestStatus.ToList();
            }
        }

        public static ServiceResponseResult AddPropertyOwnerMarketJob(JobMarketModel marketJob, Login login, HttpFileCollectionBase files = null, string serverPath = null)
        {
            using (var db = new KeysEntities())
            {
                var newMarketJob = new TenantJobRequest();
                newMarketJob.JobDescription = marketJob.JobDescription;
                newMarketJob.PropertyId     = marketJob.PropertyId;
                newMarketJob.JobStatusId    = 1; // should be OPEN
                newMarketJob.CreatedBy      = login.Email;
                newMarketJob.CreatedOn      = DateTime.UtcNow;
                newMarketJob.UpdatedOn      = DateTime.UtcNow;
                newMarketJob.UpdatedBy      = login.Email;
                newMarketJob.MaxBudget      = marketJob.MaxBudget;
                newMarketJob.OwnerId        = login.Id;
                newMarketJob.Title          = marketJob.Title;
                db.TenantJobRequest.Add(newMarketJob);
                try
                {
                    db.SaveChanges();
                    var mediaResult = AddMarketJobMedia(files, newMarketJob.Id, serverPath);
                    return mediaResult.IsSuccess ? new ServiceResponseResult { IsSuccess = true }
                                                : new ServiceResponseResult { IsSuccess = false, ErrorMessage = mediaResult.ErrorMessage };

                }
                catch (Exception)
                {

                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }

        public static ServiceResponseResult AddDIYJob(JobModel model, Login login, HttpFileCollectionBase files,  string serverPath = null)
        {
            using (var db = new KeysEntities())
            { 
                try
                {
                    var newDIYJob = new Job
                    {
                        PropertyId = model.PropertyId,
                        JobDescription = model.JobDescription,
                        JobStatusId = 2,  // Should be in pending state as the job hasnt started yet
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = login.Email,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedBy = login.Email,
                        PercentDone = 0,
                        OwnerId = login.Id,
                        JobRequestId = model.JobRequestId,
                        PaymentAmount = 0,
                    };
                    db.Job.Add(newDIYJob);
                     var request = db.PropertyRequest.Where(x => x.Id == model.JobRequestId).FirstOrDefault();
                    request.RequestStatusId = (int)JobRequestStatus.Accepted;
                    request.IsUpdated = true;
                    //foreach (int fileId in model.FilesCopy)
                    //{
                    //    var propertyRequestMedia1 = db.PropertyRequestMedia.Where(x => x.Id == fileId).FirstOrDefault();
                    //    var newDIYJobMedia = new JobMedia();

                    //    newDIYJobMedia.JobId = newDIYJob.Id;
                    //    newDIYJobMedia.NewFileName = propertyRequestMedia1.NewFileName;
                    //    newDIYJobMedia.OldFileName = propertyRequestMedia1.OldFileName;
                    //    newDIYJobMedia.PropertyId = newDIYJob.PropertyId;
                    //    db.JobMedia.Add(newDIYJobMedia);

                    //}
                    
                    var mediaFiles = MediaService.SaveFiles(files, 5, AllowedFileType.Images).NewObject as List<MediaModel>;
                    mediaFiles.ForEach(x => newDIYJob.JobMedia.Add(new JobMedia
                    {
                        NewFileName = x.NewFileName,
                        OldFileName = x.OldFileName,
                    }));
                    db.SaveChanges();
                    return new ServiceResponseResult { IsSuccess = true };
                                               
                }
                catch (Exception ex)
                {
                    return new ServiceResponseResult { IsSuccess = false, ErrorMessage = _error };
                }
            }
        }
        
        public static JobMarketModel GetMarketJobById( int id)
        {
            using (var db = new KeysEntities())
            {
                var marketJob =  db.TenantJobRequest.FirstOrDefault(x => x.Id == id);
                if (marketJob == null) return null;
                var j = new JobMarketModel
                {
                    Id = marketJob.Id,
                    Title = marketJob.Title,
                    Address = new AddressViewModel
                    {
                        AddressId = marketJob.Property.Address.AddressId,
                        CountryId = marketJob.Property.Address.CountryId,
                        Number = marketJob.Property.Address.Number.Replace(" ", ""),
                        Street = marketJob.Property.Address.Street.Trim(),
                        City = marketJob.Property.Address.City.Trim(),
                        Suburb = marketJob.Property.Address.Suburb.Trim() ?? "",
                        PostCode = marketJob.Property.Address.PostCode.Replace(" ", ""),
                    },
                    JobDescription = marketJob.JobDescription,
                    MaxBudget = marketJob.MaxBudget,
                    PostedDate = marketJob.CreatedOn,
                    MediaFiles = marketJob.TenantJobRequestMedia.Select( y => new MediaModel {NewFileName = y.NewFileName, OldFileName = y.OldFileName, Id = y.Id }).ToList(),
                };

                return j;
            }
        }
        
        public static Person GetOwnerDetails(int jobRequestId)
        {
            using (var db = new KeysEntities())
            {
                var ownerId = db.TenantJobRequest.Where(x => x.Id == jobRequestId).Select(y => y.Property.OwnerProperty.FirstOrDefault().Person.Id).FirstOrDefault();
                return db.Person.FirstOrDefault(x => x.Id == ownerId);
            }

        }

        public static ServiceResponseResult DeleteWatchlistItemById(int watchlistId)
        {
            var result = new ServiceResponseResult { IsSuccess = false };
            using (var db = new KeysEntities())
            {
                var watchlist = db.JobWatchList.FirstOrDefault(x => x.Id == watchlistId);
                if (watchlist == null)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Sorry, watchlist not found!";
                    return result;
                }
                db.JobWatchList.Remove(watchlist);
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
    }
}

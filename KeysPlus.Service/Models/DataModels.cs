using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeysPlus.Service.Models
{
    public class PropertyModel
    {
        public PropertyModel()
        {
            MediaFiles = new List<MediaModel>();
            FilesRemoved = new List<int>();
        }
        public int Id { get; set; }
        public int PropertyTypeId { get; set; }
        public int AddressId { get; set; }
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public int? Bedroom { get; set; }
        public int? Bathroom { get; set; }
        public double? LandSqm { get; set; }
        public int? ParkingSpace { get; set; }
        public double? FloorArea { get; set; }
        public decimal? TargetRent { get; set; }
        public int TargetRentTypeId { get; set; }
        public int? YearBuilt { get; set; }
        public bool IsActive { get; set; }
        public bool? IsOwnerOccupied { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
        public List<int> FilesRemoved { get; set; }
        public AddressViewModel Address { get; set; }
    }
    public class InspectionModel
    {
        public InspectionModel()
        {
            MediaFiles = new List<MediaModel>();
            LandlordMedia = new List<MediaModel1>();
            FilesRemoved = new List<int>();
        }
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public int StatusId { get; set; }
        public string Message { get; set; }
        public bool IsUpdated { get; set; }
        public Nullable<bool> IsViewed { get; set; }
        public int RequestId { get; set; }
        public string Reason { get; set; }
        public int PercentDone { get; set; }
        public Nullable<bool> OwnerUpdate { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
        public List<int> FilesRemoved { get; set; }
        public List<MediaModel1> LandlordMedia { get; set; }
    }
    public class RentListingModel
    {
        public RentListingModel()
        {
            MediaFiles = new List<MediaModel>();
            FilesRemoved = new List<int>();
        }
        public int WatchListId { get; set; }
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string Description { get; set; }
        public decimal? MovingCost { get; set; }
        public decimal TargetRent { get; set; }
        public DateTime? AvailableDate { get; set; }
        public string Furnishing { get; set; }
        public string IdealTenant { get; set; }
        public int? OccupantCount { get; set; }        
        public string PetsAllowed { get; set; }
        public bool IsActive { get; set; }
        public int RentalStatusId { get; set; }
        public string Title { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
        public List<int> FilesRemoved { get; set; }

    }

    public class RentalApplicationModel
    {
        public RentalApplicationModel()
        {
            MediaFiles = new List<MediaModel>();
            FilesRemoved = new List<int>();
        }
        public int Id { get; set; }
        public int RentalListingId { get; set; }
        public int PersonId { get; set; }
        public string Note { get; set; }
        public int? TenantsCount { get; set; }
        public int ApplicationStatusId { get; set; }
        public bool? IsViewedByOwner { get; set; }
        public int PropertyId { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
        public List<int> FilesRemoved { get; set; }

    }
    public class RequestModel
    {
        public RequestModel()
        {
            MediaFiles = new List<MediaModel>();
            FilesRemoved = new List<int>();
        }
        public int Id { get; set; }
        public int RequestTypeId { get; set; }
        public int PropertyId { get; set; }
        public int? RecipientId { get; set; }
        public bool ToOwner { get; set; }
        public bool ToTenant { get; set; }
        public string RequestMessage { get; set; }
        public int RequestStatusId { get; set; }
        public bool IsViewed { get; set; }
        public bool IsActive { get; set; }
        public Nullable<bool> IsUpdated { get; set; }
        public Nullable<System.DateTime> DueDate { get; set; }
        public string Reason { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
        public List<int> FilesRemoved { get; set; }
    }

    public class JobModel
    {
        public JobModel()
        {
            MediaFiles = new List<MediaModel>();
            FilesRemoved = new List<int>();
        }
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public int? ProviderId { get; set; }
        public int? OwnerId { get; set; }
        public DateTime? JobStartDate { get; set; }
        public DateTime? JobEndDate { get; set; }
        public int? JobStatusId { get; set; }
        public int? JobRequestId { get; set; }
        public int? PercentDone { get; set; }
        public string Note { get; set; }
        public string JobDescription { get; set; }
        public decimal? AcceptedQuote { get; set; }
        public Nullable<bool> OwnerUpdate { get; set; }
        public Nullable<bool> ServiceUpdate { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
        public List<int> FilesRemoved { get; set; }

    }

    public class MarketJobModel
    {
        public MarketJobModel()
        {
            MediaFiles = new List<MediaModel>();
            FilesRemoved = new List<int>();
        }
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public int RequestId { get; set; }
        public string Title { get; set; }
        public string JobDescription { get; set; }
        public decimal? MaxBudget { get; set; }
        public List<int> FilesRemoved { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
    }

    public class JobQuoteModel
    {
        public JobQuoteModel()
        {
            MediaFiles = new List<MediaModel>();
            FilesRemoved = new List<int>();
        }
        public int Id { get; set; }
        public int JobRequestId { get; set; }
        public int ProviderId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public bool? IsViewed { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
        public List<int> FilesRemoved { get; set; }
    }
}

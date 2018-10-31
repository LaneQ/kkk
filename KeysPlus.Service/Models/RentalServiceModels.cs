using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KeysPlus.Service.Models
{
    public class PORentalListingSearchModel : SearchInputModel
    {
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }
        public bool CanListRental { get; set; }
    }
    public class RentalListingSearchModel : SearchInputModel
    {
        public bool IsUserTenant { get; set; }
        public bool IsTenantProfileComplete { get; set; }
        public int UserId { get; set; }

    }
    public class RentListingViewModel
    {
        public DateTime? CreatedOn { get; set; }
        public RentListingModel Model { get; set; }
        public AddressViewModel Address { get; set; }
        public string RentalPaymentType { get; set; }
        public int NewApplicationsCount { get; set; }
        public string PropertyAddress { get; set; }
        public string PropertyType { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public double? LandSqm { get; set; }
        public int? ParkingSpaces { get; set; }
        public double? FloorArea { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool IsInWatchlist { get; set; }
        public bool IsOwner { get; set; }
        public bool IsApplied { get; set; }
    }

    public class RentalListingModel : ItemModel
    {

        public RentalListingModel()
        {
            RentalFiles = new List<MediaModel>();
        }
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public int PropertyId { get; set; }
        [Required]
        public string Title { get; set; }
        public string RentalDescription { get; set; }
        public decimal MovingCost { get; set; }
        public decimal TargetRent { get; set; }
        public string RentType { get; set; }
        public DateTime? AvailableDate { get; set; }
        public string Furnishing { get; set; }
        public string IdealTenant { get; set; }
        public int? OccupantCount { get; set; }
        public string PetsAllowed { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedOn { get; set; }
        public string PropertyName { get; set; }
        public string PropertyAddress { get; set; }
        public string PropertyType { get; set; }
        public string RentalPaymentType { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public double? LandSqm { get; set; }
        public int? ParkingSpaces { get; set; }
        public double? FloorArea { get; set; }
        public bool IsActive { get; set; }
        public int NewApplicationsCount { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public List<int> RemoveList { get; set; }
        public bool IsTenant { get; set; }
        public string RentalStatus { get; set; }
        public List<MediaModel> RentalFiles { get; set; }
        public AddressViewModel Address { get; set; }
        public string WatchListText { get; set; }

    }

    public enum RentalStatus
    {
        Open = 1
    }

    public class PropertyAndAddressModel
    {
        public int Id { get; set; }
        public AddressViewModel Address { get; set; }
        public string AddressString { get; set; }
        public int YearBuilt { get; set; }
    }
    public class RequestTypeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class POSendRequestModel
    {
        public POSendRequestModel()
        {
            AvalableProperties = new List<PropertyAndAddressModel>();
        }
        public List<RequestTypeModel> RequestTypes { get; set; }
        public List<PropertyAndAddressModel> AvalableProperties { get; set; }
        public string ReturnUrl { get; set; }
    }
    public class MarketJobSearchModel : SearchInputModel
    {
        public bool IsOwnerView { get; set; }
        public bool IsUserServiceSupply { get; set; }
        public bool IsProfileComplete { get; set; }
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }
    }
    public class MarketJobViewModel
    {
        public MarketJobModel Model { get; set; }
        public string PropertyAddress { get; set; }
        public AddressViewModel Address { get; set; }
        public bool IsOwnedByUser { get; set; }
        public bool IsApplyByUser { get; set; }
        public int NewQuotesCount { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsInWatchlist { get; set; }
    }
    public class JobMarketModel 
    {
        public JobMarketModel()
        {
            JobQuotes = new List<QuoteModel>();
            MediaFiles = new List<MediaModel>();
        }
        public int WatchListId { get; set; }
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string Title { get; set; }
        public string JobDescription { get; set; }
        public string PropertyAddress { get; set; }
        public decimal? MaxBudget { get; set; }
        public bool IsOwnedByUser { get; set; }
        public DateTime PostedDate { get; set; }
        public int NewQuotesCount { get; set; }
        public List<QuoteModel> JobQuotes { get; set; }
        public IEnumerable<int> FilesRemoved { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
        public AddressViewModel Address { get; set; }
        public DateTime CreatedOn { get; internal set; }
    }
    public class RentAppViewModel
    {
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public RentalApplicationModel Model { get; set; }
        public AddressViewModel Address { get; set; }
        public RentListingModel RentalListing { get; set; }
    }

    public class RentalAppSearchModel : SearchInputModel
    {
        public RentalApplicationStatus? RentalStatus { get; set; }
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }

    }
    public class AcceptAndDeclineRentalApplicationModel
    {
        [Required]
        public int TenantId { get; set; }
        [Required]
        public int PropertyId { get; set; }
        [Required]
        public int RentalListingId { get; set; }
        [Required]
        public int ApplicationId { get; set; }

    }

    public enum RentalApplicationStatus
    {
        Applied = 1,
        Accepted = 2,
        Declined = 3,
    }

    public enum PropertyRequestStatus { Submitted = 1 , Accepted = 2, Pending = 3, Close = 4, Declined = 5}

}

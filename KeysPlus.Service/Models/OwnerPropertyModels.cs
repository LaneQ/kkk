using KeysPlus.Data;
using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeysPlus.Service.Models
{
    public class POPropSearchModel : SearchInputModel
    {
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }
        public bool CanListRental { get; set; }
        public bool IsFinanceDetailPage { get; set; }
        public bool IsPropertyIndexPage { get; set; }
    }
    public class PropViewModel
    {
        public PropertyModel Model { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? CurrentHomeValue { get; set; }
        public string PropertyTypeName { get; set; }
        public int TenantCount { get; set; }
        public int NewRequestsCount { get; set; }
        public int NewQuotesCount { get; set; }       
        public decimal Mortgage { get; set; }
        public DateTime CreatedOn { get; set; }

        public string PropertyAddress { get; set; }
    }
    public class POTenantRequestSearchModel : SearchInputModel
    {
        public PropertyRequestStatus? RequestStatus { get; set; }
    }

    public class POMyRequestsSearchModel : SearchInputModel
    {
        public int? PropertyId { get; set; }
        public PropertyRequestStatus? RequestStatus { get; set; }
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }
    }

    public class POMyTenantSearchModel : SearchInputModel
    {
        public int TenantId { get; set; }
        public int PropertyId { get; set; }
        public List<int> PropertyIds { get; set; }
        public PropertyTenantsModel PropertyTenantsModel { get; set; }
    }

    public class TenantRequestViewModel
    {
        public RequestModel Model { get; set; }
        public AddressViewModel Address { get; set; }
        public DateTime CreatedOn { get; set; }
        public int TenantId { get; set; }
        public string RequestType { get; set; }
        public string RequestStatus { get; set; }
        public string TenantName { get; set; }
        public string TenantPhoneNumber { get; set; }
        public string TenantProfileFoto { get; set; }
    }

    public class PropertyViewModel
    {
        public PropertyViewModel()
        {
            PhotoFiles = new List<MediaModel>();
            Address = new AddressViewModel();
            Finance = new List<PropertyFinancialViewModel>();
        }

        public int Id { get; set; }
        public int PropertyTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Bedroom { get; set; }
        public int? Bathroom { get; set; }
        public double? LandArea { get; set; }
        public double? FloorArea { get; set; }
        public int? ParkingSpace { get; set; }
        public int YearBuilt { get; set; }
        public decimal? TargetRent { get; set; }
        public decimal? PaymentAmount { get; set; }
        public int TargetRentType { get; set; }
        public string RentalPaymentType { get; set; }
        public string PropertyType { get; set; }
        public int? ResidentialType { get; set; }
        public int? CommercialType { get; set; }
        public bool IsActive { get; set; }
        public string CreatedOn { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? Mortgage { get; set; }
        public decimal? TotalRepayment { get; set; }
        public decimal? TotalExpenses { get; set; }
        public decimal? Yield { get; set; }
        public decimal? CurrentHomeValue { get; set; }
        public decimal? CapitalGain { get; set; }
        public decimal? ActualRentalPayments { get; set; }
        public bool? IsOwnerOccupied { get; set; }
        public string AddressString { get; set; }
        public int TenantCount { get; set; }
        public AddressViewModel Address { get; set; }
        public List<PropertyFinancialViewModel> Finance { get; set; }
        public List<MediaModel> PhotoFiles { get; set; }
        public List<int> FilesRemoved { get; set; }
        public int NewApplications { get; set; }
        public List<RentalApplicationsViewModel> RentalApplications { get; set; }
    }

    // ViewModel mapping to return the collection of it
    public class TenantDetailsViewModel
    {
        public string PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int TenantsCount { get; set; }
        public string Note { get; set; }
    }

    public class MediaModel
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public string Status { get; set; }
        public string NewFileName { get; set; }
        public string OldFileName { get; set; }
        public string Type { get; set; }
        public int MediaType { get; set; } //1 : Image , 2: Document
        public long Size { get; set; }
    }
    public class MediaModel1
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public string Status { get; set; }
        public string NewFileName { get; set; }
        public string OldFileName { get; set; }
        public string Type { get; set; }
        public int MediaType { get; set; } //1 : Image , 2: Document
        public long Size { get; set; }
    }

    public class PropertyFinancialViewModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? Mortgage { get; set; }
        public decimal? TotalRepayment { get; set; }
        public decimal? TotalExpenses { get; set; }
        public decimal? Yield { get; set; }
        public decimal? CurrentHomeValue { get; set; }
        public decimal? CapitalGain { get; set; }
        public decimal? ActualRentalPayments { get; set; }
        public int HomeValueType { get; set; }
    }

    public class AddTenantToPropertyModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string TenantEmail { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public String StartDate1 { get; set; }
        public String EndDate1 { get; set; }
        public int? PaymentFrequencyId { get; set; }
        [Required]
        public decimal? PaymentAmount { get; set; }
        [Required]
        public DateTime? PaymentStartDate { get; set; }
        public int PaymentDueDate { get; set; }
        [Required]
        public bool IsMainTenant { get; set; }
        public List<LiabilityModel> Liabilities { get; set; }
        public List<int> DeleteLiabilities { get; set; }
        public int? YearBuilt { get; set; }
        public string ReturnUrl { get; set; }

    }
    public class LiabilityModel
    {
        public int Id { get; set; }
        public int TenantPropertyId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public String Status { get; set; }
    }
    public class PropertyOnboardModel
    {
        public int Id { get; set; }
        public int PropertyTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Bedroom { get; set; }
        public int? Bathroom { get; set; }
        public double? LandArea { get; set; }
        public double? FloorArea { get; set; }
        public int? ParkingSpace { get; set; }
        public int YearBuilt { get; set; }
        public decimal? TargetRent { get; set; }
        public decimal? PaymentAmount { get; set; }
        public int TargetRentType { get; set; }
        public string PropertyType { get; set; }
        public int? ResidentialType { get; set; }
        public int? CommercialType { get; set; }
        public bool IsActive { get; set; }
        public string CreatedOn { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? Mortgage { get; set; }
        public decimal? TotalRepayment { get; set; }
        public decimal? TotalExpenses { get; set; }
        public decimal? Yield { get; set; }
        public decimal? CurrentHomeValue { get; set; }
        public decimal? CapitalGain { get; set; }
        public decimal? ActualRentalPayments { get; set; }
        public bool IsOwnerOccupied { get; set; }
        public AddressViewModel Address { get; set; }
    }

    public class ExpenseViewModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public bool ToBeDeleted { get; set; }
    }

    public class RepaymentViewModel
    {
        public int Id { get; set; }
        public string IsActive { get; set; }
        public decimal TotalRepaymentsForPeriod { get; set; }
        public int PropertyId { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public int FrequencyType { get; set; }
        public string FrequencyName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? EndDate { get; set; }
        public bool ToBeDeleted { get; set; }

    }

    public class RentalPaymentViewModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int FrequencyTypeId { get; set; }
        public string FrequencyType { get; set; }
        public bool ToBeDeleted { get; set; }
    }

    public class PropertyAndRequestsModel
    {
        public PropertyAndRequestsModel()
        {
            TenantJobRequests = new List<TenantJobRequestModel>();
        }
        public int PropertyId { get; set; }
        public AddressViewModel Address { get; set; }
        public string PropertyAddress { get; set; }
        public string TenantName { get; set; }
        public string TenantContactNumber { get; set; }
        public int NewRequestsCount { get; set; }
        public List<MediaModel> PropertyImages { get; set; }
        public List<TenantJobRequestModel> TenantJobRequests { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class PORentAppsSearchModelModel : SearchInputModel
    {
        public int? RentalListingId { get; set; }
        public AddressViewModel Address { get; set; }
    }
    public class RentalApplicationsViewModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public int RentalListingId { get; set; }
        public int TenantId { get; set; }
        public string PropertyAddress { get; set; }
        public string Description { get; set; }
        public string TenantName { get; set; }
        public string TenantContactNumber { get; set; }
        public bool IsViewedByOwner { get; set; }
        public int? TenantsCount { get; set; }
        public string Note { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
        public List<MediaModel> DocFiles { get; set; }
        public System.DateTime CreatedOn { get; set; }
    }

    public class TenantJobRequestModel
    {
        public TenantJobRequestModel()
        {
            MediaFiles = new List<MediaModel>();
            FilesCopy = new List<int>();  //Bug #2031 (Part 3)
        }
        [Required(ErrorMessage = "Something went wrong ( No property) !!")]
        public int PropertyId { get; set; }
        [Required(ErrorMessage = "Please Enter The MaxBudget")]
        public decimal MaxBudget { get; set; }
        public int TenantJobRequestId { get; set; }
        public string RequestType { get; set; }
        [Required(ErrorMessage = "Please Enter The Job Description")]
        [MinLength(10, ErrorMessage = "Please Enter minimum of 10 letters  for Job Description")]
        public string JobDescription { get; set; }
        public bool IsViewed { get; set; }
        public bool IsAccepted { get; set; }
        public string RequestStatus { get; set; }
        public bool ToOwner { get; set; }
        public bool ToTenant { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public DateTime DateCreated { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
        public List<int> FilesCopy { get; set; }
        public Address Address { get; set; }
        public string Title { get; set; }
    }

    public class RequestReplyModel
    {
        public RequestReplyModel()
        {
            MediaFiles = new List<MediaModel>();
        }
        [Required(ErrorMessage = "Something went wrong No Request Id !!")]
        public int RequestId { get; set; }
        [Required(ErrorMessage = "Please your Reply")]
        [MinLength(10, ErrorMessage = "Please Enter minimum of 10 letters  for Reply")]
        public string RequestReply { get; set; }
        public int PropertyId { get; set; }
        public int PercentDone { get; set; }
        public List<MediaModel> MediaFiles { get; set; }

    }


    public class ViewRequestReplyModel
    {
        public ViewRequestReplyModel()
        {
            MediaFiles = new List<MediaModel>();
        }
        [Required(ErrorMessage = "Something went wrong No Request Id !!")]
        public int RequestId { get; set; }
        [Required(ErrorMessage = "Please your Reply")]
        [MinLength(10, ErrorMessage = "Please Enter minimum of 10 letters  for Reply")]
        public string RequestReply { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public string TenantName { get; set; }
        public string TenantPhoneNumber { get; set; }
        public List<MediaModel> MediaFiles { get; set; }

    }



    public class PropertyMyOnboardModel
    {
        [Required(ErrorMessage = "Please Enter PropertyId")]
        public int Id { get; set; }
        public int PropertyTypeId { get; set; }
        [Required(ErrorMessage = "Please Enter PropertyName")]
        public string PropertyName { get; set; }
        [Required(ErrorMessage = "Please Enter Job Description")]
        [MinLength(10, ErrorMessage = "Please Enter minimum of 10 letters for Job Description")]
        public string Description { get; set; }
        public int? Bedroom { get; set; }
        public int? Bathroom { get; set; }
        public double? LandArea { get; set; }
        public double? FloorArea { get; set; }
        public int? ParkingSpace { get; set; }
        [Required(ErrorMessage = "Please Enter YearBuilt")]
        public int YearBuilt { get; set; }
        public decimal? TargetRent { get; set; }
        public int TargetRentType { get; set; }
        public string PropertyType { get; set; }
        public int? ResidentialType { get; set; }
        public int? CommercialType { get; set; }
        public bool IsActive { get; set; }
        public string CreatedOn { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? Mortgage { get; set; }
        public decimal? TotalRepayment { get; set; }
        public decimal? TotalExpenses { get; set; }
        public decimal? Yield { get; set; }
        public decimal? CurrentHomeValue { get; set; }
        public decimal? CapitalGain { get; set; }
        public decimal? ActualRentalPayments { get; set; }
        public int HomeValueType { get; set; }
        public bool IsOwnerOccupied { get; set; }
        public AddressViewModel Address { get; set; }
        public List<ExpenseViewModel> Expenses { get; set; }
        public List<RepaymentViewModel> Repayments { get; set; }
        public AddTenantToPropertyModel TenantToPropertyModel { get; set; }
        public List<LiabilityModel> LiabilityValues { get; set; }
    }
    public class FinancialModel
    {
        public FinancialModel()
        {
            HomeValues = new List<HomeValueViewModel>();
        }

        public int PropId { get; set; }
        public int YearBuilt { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? Mortgage { get; set; }
        public decimal? TotalRepayment { get; set; }
        public decimal? TotalExpenses { get; set; }
        public decimal? CurrentHomeValue { get; set; }
        public int HomeValueType { get; set; }
        public string PropertyValueType { get; set; }
        public bool IsOwnerOccupied { get; set; }
        public List<ExpenseViewModel> Expenses { get; set; }
        public List<RepaymentViewModel> Repayments { get; set; }
        public IEnumerable<RentalPaymentViewModel> RentalPayments { get; set; }
        public IEnumerable<HomeValueViewModel> HomeValues { get; set; }
        public PropertyFinanceResultModel FinanceReport { get; set; }
        public AddTenantToPropertyModel TenantToPropertyModel { get; set; }
    }

    public class HomeValueViewModel
    {
        public int PropertyId { get; set; }
        public int Id { get; set; }
        public decimal Value { get; set; }
        public int TypeId { get; set; }
        public string ValueType { get; set; }
        public DateTime Date { get; set; }
        public bool ToBeDeleted { get; set; }
        public bool IsActive { get; set; }
    }
    // this class for rental application table purpose
    public class PropertyDetails
    {

        public int RentListingId { get; set; }
        public int PropertyId { get; set; }
        public string Name { get; set; }
        public string PropertyAddress { get; set; }
        public decimal? TargetRent { get; set; }
        public int OccupentsCount { get; set; }
        public int NewApplicationsCount { get; set; }
        public List<MediaModel> MediaFiles { get; set; }

    }


    public class PODashBoardModel
    {
        public PropDashboardModel PropDashboardData { get; set; }
        public RentAppDashboardModel RentAppsDashboardData { get; set; }
        public JobsDashboardModel JobsDashboardData { get; set; }
        public TenantRequestDashboardModel RequestDashboardData { get; set; }
        public JobQuotesDashboardModel JobQuotesDashboardData { get; set; }
        public int IntroSteps { get; set; }
    }

    public class PropDashboardModel
    {
        public int Occupied { get; set; }
        public int Vacant { get; set; }
    }
    public class RentAppDashboardModel
    {
        public int NewItems { get; set; }
        public int Approved { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
    }

    public class JobsDashboardModel
    {
        public int NewItems { get; set; }
        public int InProgress { get; set; }
        public int Resolved { get; set; }
    }
    public class TenantRequestDashboardModel
    {

        public int Current { get; set; }

        public int Accepted { get; set; }
        //public int Pending { get; set; }
        public int Rejected { get; set; }
    }
    public class JobQuotesDashboardModel
    {
        public int NewItems { get; set; }
        public int Accepted { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
    }

    public class PropDataModel
    {
        public string ReturnUrl { get; set; }
        public IEnumerable<PropertyAndAddressModel> Properties { get; set; }
    }

    public class PropertyInspectionModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public int StatusId { get; set; } //for POST
        public string StatusName { get; set; } //for GET
        public string Message { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsUpdated { get; set; }
        public bool IsViewed { get; set; }
        public bool Isactive { get; set; }
        public int RequestId { get; set; }
        public string Reason { get; set; }
        public int PercentDone { get; set; }
        public string Address { get; set; }
        public DateTime? DueDate { get; set; }
        public List<MediaModel> InspectionMedia { get; set; }
        public AddressViewModel PropertyAddress { get; set; }
        public string RequestStatus { get; set; }
        public List<MediaModel1> PropertyRequestMedia { get; set; }
    }

    public class POJobSearchModel : SearchInputModel
    {
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }
    }
    public class POInspectionsSearchModel : SearchInputModel
    {
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }
    }

    public class PaymentTracking {
        public int Id { get; set; }
        public Address Address {get; set;}
        public string AddressString { get; set; }
        public string TenantName { get; set; }
        public DateTime DueDate { get; set; }
        public bool PaymentComplete { get; set; }
    }
}

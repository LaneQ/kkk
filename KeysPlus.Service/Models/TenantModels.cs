using KeysPlus.Data;
using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace KeysPlus.Service.Models
{
    public class RegisterToBeTenantModel
    {
        [Required]
        [Display(Name = "Email Address")]
        [System.Web.Mvc.Remote("IsEmailAvailable", "Account", ErrorMessage = "This Email already in use !!")]
        [RegularExpression(@"^(?("")("".+?(?<!\\)""@)|(([0-9a-zA-Z]((\.(?!\.))|[-'\w])*)(?<=[0-9a-zA-Z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][\w]*[0-9a-zA-Z]*\.)+[A-Za-z0-9]{0,22}[A-Za-z0-9]))$", ErrorMessage = "Invalid Email Address")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword
        {
            get
            { return Password; }
            set { this.Password = Password; }
        }

        [Required(ErrorMessage = "Please enter First Name")]
        [Display(Name = "First Name")]
        [RegularExpression("^[a-zA-ZàáâäãåąčćęèéêëėįìíîïłńòóôöõøùúûüųūÿýżźñçčšžÀÁÂÄÃÅĄĆČĖĘÈÉÊËÌÍÎÏĮŁŃÒÓÔÖÕØÙÚÛÜŲŪŸÝŻŹÑßÇŒÆČŠŽ∂ð ,.'-]{2,30}$",
            ErrorMessage = "First Name,Please enter valid name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter Last Name")]
        [Display(Name = "Last Name")]
        [RegularExpression("^[a-zA-ZàáâäãåąčćęèéêëėįìíîïłńòóôöõøùúûüųūÿýżźñçčšžÀÁÂÄÃÅĄĆČĖĘÈÉÊËÌÍÎÏĮŁŃÒÓÔÖÕØÙÚÛÜŲŪŸÝŻŹÑßÇŒÆČŠŽ∂ð ,.'-]{2,30}$",
            ErrorMessage = "Last Name,Please enter valid name")]
        public string LastName { get; set; }


        [Required]
        public int PropertyId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PaymentFrequencyId { get; set; }
        public decimal? PaymentAmount { get; set; }
    }

    public class ActivateTobeTenantModel
    {
        public int TenantId { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Token { get; set; }
        public int PropertyId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public String StartDate1 { get; set; }
        public String EndDate1 { get; set; }
        public int? PaymentFrequencyId { get; set; }
        public decimal? PaymentAmount { get; set; }
    }

    public class TenantDetailsModel
    {
        [Required]
        public DateTime DateOfBirth { get; set; }
        public string HomePhoneNumber { get; set; }
        public string MobilePhoneNumber { get; set; }
        public AddressViewModel Address { get; set; }
    }

    public class TenantRentalViewModel
    {
        public MyRentalsModel Model { get; set; }
        public string RentalPaymentType { get; set; }
        public AddressViewModel Address { get; set; }
        public string AddressString { get; set; }
        public int LandlordId { get; set; }
        public string Landlordname { get; set; }
        public string LandlordPhone { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? NextPaymenDate { get; set; }
    }
    //public class MyRentalsViewModel
    //{
    //    public IEnumerable<TenantRentalViewModel> Items { get; set; }
    //}
    public class MyRentalsSearchModel: SearchInputModel
    {        
    }

    public class RentalInfoModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public int? LandlordId { get; set; }
        public int TenantId { get; set; }
        public int? PaymentFrequencyId { get; set; }
        public AddressViewModel PropertyAddress { get; set; }
        public string Landlordname { get; set; }
        public string LandlordPhone { get; set; }
        public string RentalPaymentType { get; set; }
        public DateTime? PaymentStartDate { get; set; }
        public int? PaymentDueDate { get; set; }
        public decimal? PaymentAmount { get; set; }
        public DateTime? NextPaymenDate { get; set; }
        public string Address { get; set; }
    }
    public class MyRentalsModel
    {
        public MyRentalsModel()
        {
            MediaFiles = new List<MediaModel>();
        }
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public int TenantId { get; set; }
        public int? PaymentFrequencyId { get; set; }
        public string Address { get; set; }
        public AddressViewModel PropertyAddress { get; set; }
        public string Landlordname { get; set; }
        public string LandlordPhone { get; set; }
        public int LandlordId { get; set; }
        public decimal? TargetRent { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string RentalPaymentType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? PaymentStartDate { get; set; }
        public int? PaymentDueDate { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
        public string AddressString { get; set; }
        public int OwnerId { get; set; }
       
    }

    public class PropertyTenantsModel
    {
        public PropertyTenantsModel()
        {
            ApplicationFiles = new List<MediaModel>();
       }
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyAddress { get; set; }
        public string StreetAddress { get; set; }
        public string CitySub { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string TenantPhone { get; set; }
        public string TenantEmail { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string RentFrequency { get; set; }
        public decimal? RentAmount { get; set; }
        public string ProfilePicture { get; set; }
        public List<MediaModel> ApplicationFiles { get; set; }
        public decimal? PaymentAmount { get; set; }

    }

    public class TenantDashBoardModel
    {
        public List<RentalInfoModel> TenantRentalDashboardData { get; set; }
        public TenantRentAppDashboardModel RentAppsDashboardData { get; set; }
        public TenantRequestDashboardModel TenantRequestDashboardData { get; set; }
        public LandLordRequestDashboardModel LandLordRequestDashboardData { get; set; }
        public int IntroSteps { get; set; }
    }

    public class LandLordRequestDashboardModel
    {
        public int NewItems { get; set; }
        public int Accepted { get; set; }
        public int Rejected { get; set; }
    }

    public class TenantRentAppDashboardModel
    {
        public int NewItems { get; set; }
        public int Accepted { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
    }
    
    public class MyRequestSearchModel : SearchInputModel
    {
        public PropertyRequestStatus? RequestStatus { get; set; }
        public int? PropertyId { get; set; }
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }
    }
    public class MyRequestViewModel
    {
        public RequestModel Model { get; set; }
        public string PropertyAddress { get; set; }
        public AddressViewModel Address { get; set;}
        public string LandlordName { get; set; }
        public string LandlordContactNumber { get; set; }
        public string RequestType { get; set; }
        public string RequestStatus { get; set; }
        public DateTime? CreatedOn { get; set; }

    }

    public enum JobRequestStatus
    {
        Submitted = 1,
        Accepted = 2,
        Pending = 3,
        close = 4
    }

    public class LandlordRequestsSearchModel : SearchInputModel
    { 
        public int? PropId { get; set; }
        public PropertyRequestStatus? RequestStatus { get; set; }
    }

    public class TenantInsPectionViewModel
    {
        public InspectionModel Model { get; set; }
        public AddressViewModel Address { get; set; }
        public DateTime? DueDate { get; set; }
        public string LandlordName { get; set; }
        public string Status { get; set; }
        public string LandlordPhone { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool CanBeEdited { get; set; }
    }
    public class TenantInspectionSearchModel : SearchInputModel
    {
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }
    }

    // TenantModels.cs
    public class TenantPropertyViewModel
    {
        public int PropertyId { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
    }

    public class TenantSendRequestModel
    {
        public List<RentalInfoModel> RentalProperties { get; set; }
        public List<RequestTypeModel> RequestTypes { get; set; }
        public string ReturnUrl { get; set; }
    }
}
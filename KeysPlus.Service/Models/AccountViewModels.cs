using KeysPlus.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace KeysPlus.Service.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [System.Web.Mvc.AllowHtml]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [System.Web.Mvc.AllowHtml]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        //[Required]
        //[EmailAddress]
        //[Display(Name = "Email")]
        //public string Email { get; set; }
        //[Required]
        //[Display(Name = "First name")]
        //public string FirstName { get; set; }
        //[Required]
        //[Display(Name = "Last name")]
        //public string LastName { get; set; }      
        [Required(ErrorMessage = "The email address is required")]
        //[EmailAddress(ErrorMessage = "Invalid email Address")]
        //[DataType(DataType.EmailAddress , ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email Address")]
        //    [System.Web.Mvc.Remote("IsEmailAvailable", "Account", ErrorMessage = "Email already in use.")]
        //[RegularExpression(@"^(?("")("".+?(?<!\\)""@)|(([0-9a-zA-Z]((\.(?!\.))|[-'\w])*)(?<=[0-9a-zA-Z])@))" +
        //@"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][\w]*[0-9a-zA-Z]*\.)+[A-Za-z0-9]{0,22}[A-Za-z0-9]))$", ErrorMessage = "Invalid Email Address")]
        //[RegularExpression("^[a-zA-Z0-9.]+@([a-zA-Z0-9]+.)+[a-zA-Z]{2,6}$", ErrorMessage = "Invalid Email Address")]    
        //[RegularExpression("^([0-9a-zA-Z]([-\\.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$", ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"^[a-zA-Z0-9._\-]+@([a-zA-Z0-9]+.){1,2}[a-zA-Z]{2,6}$", ErrorMessage = "Invalid email Address")] // fixed bug Bug #2440
        public string UserName { get; set; }
        [Required(ErrorMessage ="Require a password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword
        { get
            { return Password; }
            set {  this.Password=Password; } }
        [Required(ErrorMessage ="Please select account type")]
        //[Display(Name ="Category")]
        public int RoleId { get; set; }
        [Required (ErrorMessage="Please enter First Name")]
        [Display(Name="First Name")]
        //[RegularExpression("^[a-zA-ZàáâäãåąčćęèéêëėįìíîïłńòóôöõøùúûüųūÿýżźñçčšžÀÁÂÄÃÅĄĆČĖĘÈÉÊËÌÍÎÏĮŁŃÒÓÔÖÕØÙÚÛÜŲŪŸÝŻŹÑßÇŒÆČŠŽ∂ð ,.'-]{2,30}$",
        //    ErrorMessage = "First Name,Please enter valid name")]
        [RegularExpression("^([A-Za-z]+[,.]?[ ]?|[A-Za-z]+['-]?)+$",ErrorMessage ="First Name,Please enter valid name")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        public string FirstName { get; set; }
        //[Required(ErrorMessage = "Please enter Middle Name")]
        [Display(Name = "Middle Name")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "Please enter Last Name")]
        [Display(Name = "Last Name")]
        //[RegularExpression("^[a-zA-ZàáâäãåąčćęèéêëėįìíîïłńòóôöõøùúûüųūÿýżźñçčšžÀÁÂÄÃÅĄĆČĖĘÈÉÊËÌÍÎÏĮŁŃÒÓÔÖÕØÙÚÛÜŲŪŸÝŻŹÑßÇŒÆČŠŽ∂ð ,.'-]{2,30}$",
        //    ErrorMessage = "Last Name,Please enter valid name")]
        [RegularExpression("^([A-Za-z]+[,.]?[ ]?|[A-Za-z]+['-]?)+$", ErrorMessage = "Please enter valid last name")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        public string LastName { get; set; }
        //[DataType(DataType.PhoneNumber, ErrorMessage = "Please enter vaild Mobile Number")]
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid Phone number")]
        //[Display(Name = "Mobile Number")]
        //public string MobileNum { get; set; }
    }
    
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class SideBarViewModel
    {
        public IEnumerable<int> Roles { get; set; }
        public bool IsPropertyOwner { get; set; }
        public bool IsTenant { get; set; }
        public bool IsServiceSupplier { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsPropManager { get; set; }
    }
    
    public class EmailAttribute : RegularExpressionAttribute
    {
        public EmailAttribute()
            : base(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,3}")
        {
            this.ErrorMessage = "Please provide a valid email address";
            
        }
    }
    public class CompanyDetailViewModel
    {
        public CompanyDetailViewModel()
        {
            PhysicalAddress = new AddressViewModel();
            BillingAddress = new AddressViewModel();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public AddressViewModel PhysicalAddress { get; set; }
        public AddressViewModel BillingAddress { get; set; }
        public HttpPostedFileWrapper NewCompPhoto { get; set; }
        public MediaModel ProfilePhoto { get; set; }
        public bool IsCompShipSame { get; set; }
        public bool RemoveProfilePhoto { get; set; }
    }
    public class AccountOverView
    {

        public ProfileOverView Profile { get; set; }
        public bool IsServiceSupplier { get; set; }
        public CompanyDetailViewModel Company { get; set; }
    }

    public class ProfileOverView
    {
        public ProfileOverView()
        {
            PhysicalAddress = new AddressViewModel();
            BillingAddress = new AddressViewModel();
            MediaFiles = new List<MediaModel>();
            FilesRemoved = new List<int>();
        }
        public int Id { get; set; }
        public int LoginId { get; set; }
        public string PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
        public bool IsShipSame { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public string FullUserName { get; set; }
        [Required]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 5)]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Language { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Occupation { get; set; }
        public HttpPostedFileWrapper NewPhoto { get; set; }
        public string LinkedinUrl { get; set; }
        public string CreatedOn { get; set; }
        // public HttpPostedFileWrapper Image { get; set; }
        public MediaModel ProfilePhotoModel { get; set; }
        public AddressViewModel PhysicalAddress { get; set; }
        public AddressViewModel BillingAddress { get; set; }
        //  public List<RoleViewModel> UserRoleList { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
        public List<int> FilesRemoved { get; set; }
        public bool RemoveUserPhoto { get; set; }
    }
}

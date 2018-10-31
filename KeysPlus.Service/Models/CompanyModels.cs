using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeysPlus.Service.Models;
using System.ComponentModel.DataAnnotations;
using PagedList;
using KeysPlus.Data;

namespace KeysPlus.Service.Models
{
    public class SSMyJobsSearchModel : SearchInputModel
    {
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }
    }
    public class QuotesSearchViewModel : SearchInputModel
    {
        public int? MarketJobId { get; set; }
        public MarketJobModel MarketJob { get; set; }
        public string Status { get; set; }
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }
        public string Address { get; set; }
    }

    public class QuoteModel
    {
        public QuoteModel()
        {
            MediaFiles = new List<MediaModel>();
        }

        public int Id { get; set; }
        public int JobRequestId { get; set; }
        public string PropertyAddress { get; set; }
        public string StreetAddress { get; set; }
        public string CitySub { get; set; }
        public AddressViewModel Address { get; set; }
        public string JobDescription { get; set; }
        [Required]
        [RegularExpression(@"^\d+\.\d{0,2}$", ErrorMessage = "Maximum Two Decimal Points.")]
        [Range(0, 999999999.99, ErrorMessage = "Invalid Amount.")]
        public decimal Amount { get; set; }
        public string JobQuoteStatus { get; set; }
        public string PropertyName { get; set; }
        public int? ProviderId { get; set; }
        public string ProviderName { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyProfilePhoto { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyNumber { get; set; }
        public string CompanyWebsite { get; set; }
        public decimal? MaxBudget { get; set; }
        public string Title { get; set; }
        [Required]
        [MinLength(10)]
        public string Note { get; set; }
        public string Status { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
        public List<int> RemovedFiles { get; set; }
        public bool IsViewed { get; set; }
    }
    
    public class CompanyViewModel
    {
        public CompanyViewModel()
        {

            PhysicalAddress = new AddressViewModel();
            BillingAddress = new AddressViewModel();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsShipSame { get; set; }
        public string CreatedBy { get; set; }
        public string PhotoProfile { get; set; }
        public HttpPostedFileBase Files { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsActive { get; set; }
        public AddressViewModel PhysicalAddress { get; set; }
        public AddressViewModel BillingAddress { get; set; }

    }
}

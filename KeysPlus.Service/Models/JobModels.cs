using System;
using System.ComponentModel.DataAnnotations;

namespace KeysPlus.Service.Models
{
    public class JobViewModel
    {
        public JobModel Model { get; set; }
        public AddressViewModel Address { get; set; }
        public string ProviderCompany { get; set; }
        public string ProviderName { get; set; }
        public string JobStatusName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatededOn { get; set; }
        public string PropertyAddress { get; set; }
        public string CompanyAddress { get; set; }
       
    }

    public class JobAcceptedModel
    {
        [Required]
        public int? QuoteId { get; set;}
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 10)]
        public String JobDescription { get; set; }
        [Required]
        public int JobRequestId { get; set; }
    }
}

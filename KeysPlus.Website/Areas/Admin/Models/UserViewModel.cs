using KeysPlus.Service.Models;
using KeysPlus.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeysPlus.Website.Areas.Admin.Models
{
    public class UserViewModel
    {
        public UserViewModel()
        {

            UserRoleList = new List<RoleViewModel>();
            PhysicalAddress = new AddressViewModel();
            BillingAddress = new AddressViewModel();
        }

        public int Id { get; set; }
        public int LoginId { get; set; }

        public bool? IsActive { get; set; }
        public bool IsShipSame { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Language { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Occupation { get; set; }
        public string Linkedin { get; set; }
        public string CreatedOn { get; set; }
        public string ProfilePhoto { get; set; }

        public AddressViewModel PhysicalAddress { get; set; }
        public AddressViewModel BillingAddress { get; set; }
        public List<RoleViewModel> UserRoleList { get; set; }
    }
}
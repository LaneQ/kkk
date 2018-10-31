using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace KeysPlus.Website.Areas.Admin.Models
{
    public class AdminHomeViewModel
    {
        public IPagedList<UserViewModel> UserViewModels { get; set; }
        public string OrderBy { get; set; } 

    }
}
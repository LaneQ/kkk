using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KeysPlus.Website.Areas.Admin.Models
{
    public class RoleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int loginId { get; set; }       
        public bool? IsActive { get ; set; }
        public bool? PendingApproval { get; set; }       
    }
}
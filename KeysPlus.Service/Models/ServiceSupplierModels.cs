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
    public class ServiceSupplierDashboardModel
    {
        public ServiceSupplierQuoteDashboardModel QuoteDashboardData { get; set; }
        public ServiceSupplierJobDashboardModel JobDashboardData { get; set; }
        public int IntroSteps { get; set; }

    }
    public class ServiceSupplierQuoteDashboardModel
    {
        public int NewItems { get; set; }
        public int Pending { get; set; }
        public int Accepted { get; set; }
        public int Rejected { get; set; }
    }

    public class ServiceSupplierJobDashboardModel
    {
        public int NewItems { get; set; }
        public int InProgress { get; set; }
        public int Resolved { get; set; }
    }
    

}

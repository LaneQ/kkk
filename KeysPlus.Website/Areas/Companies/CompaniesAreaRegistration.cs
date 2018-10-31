using System.Web.Mvc;

namespace KeysPlus.Website.Areas.Companies
{
    public class CompaniesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Companies";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Companies_default",
                "Companies/{controller}/{action}/{id}",
                new { controller = "Home",action = "Index",id = UrlParameter.Optional,area = "Companies" },
                namespaces: new[] { "KeysPlus.Website.Areas.Companies.Controllers" }
            );
        }
    }
}
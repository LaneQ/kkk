using System.Web.Mvc;

namespace KeysPlus.Website.Areas.PropertyOwners
{
    public class PropertyOwnersAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "PropertyOwners";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "PropertyOwner_default",
                "PropertyOwners/{controller}/{action}/{id}",
                new { controller = "Home",action = "Index",id = UrlParameter.Optional,area = "PropertyOwners" },
                namespaces: new[] { "KeysPlus.Website.Areas.PropertyOwners.Controllers" }
            );
        }
    }
}
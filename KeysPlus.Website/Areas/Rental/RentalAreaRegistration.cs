using System.Web.Mvc;

namespace KeysPlus.Website.Areas.Rental
{
    public class RentalAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Rental";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Rental_default",
                "Rental/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional, area = "Rental" },
                namespaces: new[] { "KeysPlus.Website.Areas.Rental.Controllers" }
            );
        }
    }
}
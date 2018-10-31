using System.Web;
using System.Web.Optimization;

namespace KeysPlus.Website
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.1.1.min.js",
                        "~/Scripts/jquery.json-2.4.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));
            bundles.Add(new ScriptBundle("~/bundles/layoutLibrary").Include(
                        "~/Scripts/Globals/libscripts.bundle.js",
                        "~/Scripts/Globals/vendorscripts.bundle.js",
                        "~/Scripts/Globals/mainscripts.bundle.js"
                        
                        ));
            bundles.Add(new ScriptBundle("~/bundles/chartLibrary").Include(
                        "~/Scripts/chartjs/Chart.bundle.min.js",
                        "~/Scripts/chartjs/jquery.sparkline.min.js",
                        "~/Scripts/chartjs/KeysChart.js"
                        ));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));
            bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/knockout.mapping-latest.js",
                "~/Scripts/knockout.validation.js"));
            bundles.Add(new ScriptBundle("~/bundles/moment").Include(
                "~/Scripts/moment-with-locales.js"));
            bundles.Add(new StyleBundle("~/content/pagedList").Include(
          "~/Content/stylesheets/PagedList.css"));
            bundles.Add(new StyleBundle("~/content/loginModuleStyle").Include(
            "~/Content/styles/authentication.css"));
            bundles.Add(new ScriptBundle("~/bundles/globals").Include(
                    "~/Scripts/Globals/keys.js",
                    "~/Scripts/Globals/keysAddress.js",
                    "~/Scripts/Globals/Commons.js",
                    "~/Scripts/introjs/intro.min.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/propertyOwners").Include(
                    "~/Scripts/PropertyOwners/Home/Index.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/users").Include(
                    "~/Scripts/Users/Home/Index.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/login").Include(
                    "~/Scripts/loginform.js"));

            bundles.Add(new ScriptBundle("~/bundles/jobs").Include(
                    "~/Areas/Jobs/Scripts/Index.js",
                    "~/Areas/Jobs/Scripts/ViewModel.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/profile").Include(
                    "~/Areas/Profile/Scripts/Index.js"
                ));
            bundles.Add(new StyleBundle("~/content/onboarding").Include(
            "~/Content/onboarding.css"));
            //BundleTable.EnableOptimizations = true;

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/Site.css",
                       "~/Content/introjs/introjs.css"));
            bundles.Add(new ScriptBundle("~/bundles/notification").Include(
                    "~/Scripts/NotificationStyle/js/classie.js",
                    "~/Scripts/NotificationStyle/js/modernizr.custom.js",
                    "~/Scripts/NotificationStyle/js/notificationFx.js",
                    "~/Scripts/NotificationStyle/js/snap.svg-min.js"
                ));
            bundles.Add(new StyleBundle("~/notification/css").Include(
                      "~/Scripts/NotificationStyle/css/normalize.css",
                     "~/Scripts/NotificationStyle/css/ns-default.css",
                     "~/Scripts/NotificationStyle/css/ns-style-attached.css",
                     "~/Scripts/NotificationStyle/css/ns-style-bar.css",
                     "~/Scripts/NotificationStyle/css/ns-style-growl.css",
                     "~/Scripts/NotificationStyle/css/ns-style-other.css"));
        }
    }
}

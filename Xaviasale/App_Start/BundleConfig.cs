using System.Web.Optimization;

namespace Xaviasale
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            //Scripts
            bundles.Add(new ScriptBundle("~/bundles/topJS").Include(
                "~/scripts/jquery.min.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/botJS").Include(
                "~/scripts/owl.carousel.min.js",
                "~/scripts/jquery.fancybox.min.js",
                "~/scripts/toastr.min.js",
                "~/scripts/sweetalert.min.js",
                "~/scripts/cart-controller.js",
                "~/scripts/jquery.validate.min.js",
                "~/scripts/jquery.unobtrusive-ajax.min.js",
                "~/scripts/jquery.validate.unobtrusive.min.js",
                "~/scripts/jquery.main.js"
                ));
            //CSS
            bundles.Add(new StyleBundle("~/bundles/style").Include(
                "~/css/owl.carousel.min.css",
                "~/css/jquery.fancybox.min.css",
                "~/css/toastr.min.css",
                "~/css/theme.css",
                "~/css/landing.css",
                "~/css/appreview.css",
                "~/css/appupsell.css",
                "~/css/site.css"
                ));
            BundleTable.EnableOptimizations = true;
        }
    }
}
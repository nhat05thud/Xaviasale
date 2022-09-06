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
                "~/Scripts/popper.min.js",
                "~/scripts/bootstrap.min.js",
                "~/scripts/jquery.panel.mobile.js",
                "~/libs/owl-carousel/scripts/owl.carousel.min.js",
                "~/libs/malihu/scripts/jquery.mCustomScrollbar.min.js",
                "~/libs/fancybox/scripts/jquery.fancybox.min.js",
                "~/Scripts/sweetalert.min.js",
                "~/Scripts/toastr.min.js",
                "~/scripts/jquery.validate.min.js",
                "~/scripts/jquery.unobtrusive-ajax.min.js",
                "~/scripts/jquery.validate.unobtrusive.min.js",
                "~/Scripts/cartController.js",
                "~/Scripts/filterProductController.js",
                "~/scripts/jquery.main.js"
                ));
            //CSS
            bundles.Add(new StyleBundle("~/bundles/style").Include(
                "~/css/bootstrap.min.css",
                "~/css/font-awesome.min.css",
                "~/libs/owl-carousel/css/owl.carousel.min.css",
                "~/libs/malihu/css/jquery.mCustomScrollbar.min.css",
                "~/libs/fancybox/css/jquery.fancybox.min.css",
                "~/css/toastr.min.css",
                "~/css/fonts.css",
                "~/images/mysprite.sprite.css",
                "~/css/site.css",
                "~/css/custom-master.css"
                ));
            BundleTable.EnableOptimizations = true;
        }
    }
}
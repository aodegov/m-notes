using System.Web;
using System.Web.Optimization;

namespace MedNotes
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.1.1.min.js",
                        "~/Scripts/jquery-ui-1.12.1.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/Scripts").Include(
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/respond.min.js",
                      "~/Scripts/moment.min.js",
                      "~/Scripts/bootstrap-datetimepicker.min.js",
                      "~/Scripts/jszip.min.js",
                      "~/Scripts/pdfmake.min.js",
                      "~/Scripts/vfs_fonts.js",
                      "~/Scripts/bootstrap-select.min.js",
                      "~/Scripts/select2.js"));

            bundles.Add(new ScriptBundle("~/Scripts/DataTables/js").Include(
                    "~/Scripts/DataTables/jquery.dataTables.js",
                    "~/Scripts/DataTables/dataTables.buttons.min.js",
                    "~/Scripts/DataTables/buttons.flash.min.js",
                    "~/Scripts/DataTables/buttons.html5.min.js",
                    "~/Scripts/DataTables/buttons.print.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/Site.css",
                      "~/Content/bootstrap-datetimepicker.min.css",
                      "~/Content/bootstrap-select.min.css",
                      "~/Content/font_awesome_css/font-awesome.min.css",
                      "~/Content/css/select2.css"));



            bundles.Add(new StyleBundle("~/Content/DataTables/css").Include(
                     "~/Content/DataTables/css/jquery.dataTables.min.css",
                      "~/Content/DataTables/css/buttons.dataTables.min.css"));

        }
    }
}

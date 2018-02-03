using System.Web;
using System.Web.Optimization;

namespace BugTracker
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //          "~/Content/bootstrap.css",
            //          "~/Content/site.css"));
            
            //I renamed this bundle and moved the Bootstrap files to a new folder to deal with a
            // "Failed to load resource: the server responded with a status of 403 (Forbidden)" error
            // This worked to get my published site back up and I am not sure why
            bundles.Add(new StyleBundle("~/Content/sitestyle").Include(
                      "~/Content/bs-style/bootstrap.css",
                      "~/Content/bs-style/site.css"));
        }
    }
}

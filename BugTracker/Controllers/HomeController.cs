using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return RedirectToAction("Dashboard", "Tickets", null);
            //return View();
        }

        public ActionResult About()
        {
            return View();
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookieTempDataDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Submit(string name)
        {
            TempData["name"] = name;
            return RedirectToAction("Index");
        }
    }
}

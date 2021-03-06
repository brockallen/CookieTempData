﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CookieTempDataDemo.Models;
using System.Web.Security;

namespace CookieTempDataDemo.Controllers
{
    public class HomeController : Controller
    {
        IFoo foo;

        public HomeController(IFoo foo)
        {
            this.foo = foo;
        }

        public ActionResult Index()
        {
            var data = TempData["data"] as Data;
            if (data != null)
            {
                ViewBag.Name = data.Name;
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(string name, string username)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("name", "Name was required.");
                TempData["error"] = ModelState;
                return RedirectToAction("Error");
            }
            
            var data = new Data();
            data.Name = name;
            data.Self = data;
            TempData["data"] = data;

            if (username != null)
            {
                FormsAuthentication.SetAuthCookie(username, false);
            }
            
            return RedirectToAction("Index");
        }

        public ActionResult Error()
        {
            var errors = TempData["error"] as ModelStateDictionary;
            return View("Error", errors);
        }
    }

    [Serializable]
    public class Data
    {
        public Data Self { get; set; }
        public string Name { get; set; }
    }
}

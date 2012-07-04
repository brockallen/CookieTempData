using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: PreApplicationStartMethod(typeof(BrockAllen.CookieTempData.AppStart), "Start")]

namespace BrockAllen.CookieTempData
{
    public class AppStart
    {
        public static void Start()
        {
            var currentFactory = ControllerBuilder.Current.GetControllerFactory();
            ControllerBuilder.Current.SetControllerFactory(new CookieTempDataControllerFactory(currentFactory));
        }
    }
}

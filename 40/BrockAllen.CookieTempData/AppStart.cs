using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            DynamicModuleUtility.RegisterModule(typeof(SetFactoryModule));
        }
    }

    public class SetFactoryModule : IHttpModule
    {
        static bool hasRun = false;
        static object theLock = new object();
        
        public void Init(HttpApplication app)
        {
            if (hasRun) return;
            lock (theLock)
            {
                if (hasRun) return;

                var currentFactory = ControllerBuilder.Current.GetControllerFactory();
                if (!(currentFactory is CookieTempDataControllerFactory))
                {
                    ControllerBuilder.Current.SetControllerFactory(new CookieTempDataControllerFactory(currentFactory));
                }

                hasRun = true;
            }
        }

        public void Dispose()
        {
        }
    }
}

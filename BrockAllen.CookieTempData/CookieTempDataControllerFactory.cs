using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace BrockAllen.CookieTempData
{
    class CookieTempDataControllerFactory : IControllerFactory
    {
        IControllerFactory _inner;
        public CookieTempDataControllerFactory(IControllerFactory inner)
        {
            _inner = inner;
        }

        public IController CreateController(System.Web.Routing.RequestContext requestContext, string controllerName)
        {
            var controllerInterface = _inner.CreateController(requestContext, controllerName);
            var controller = controllerInterface as Controller;
            if (controller != null)
            {
                controller.TempDataProvider = new CookieTempDataProvider();
            }
            return controller;
        }

        public System.Web.SessionState.SessionStateBehavior GetControllerSessionBehavior(System.Web.Routing.RequestContext requestContext, string controllerName)
        {
            return _inner.GetControllerSessionBehavior(requestContext, controllerName);
        }

        public void ReleaseController(IController controller)
        {
            _inner.ReleaseController(controller);
        }
    }
}

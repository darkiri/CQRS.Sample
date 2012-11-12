using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CQRS.Sample.Bootstrapping;
using StructureMap;

namespace CQRS.Sample.GUI
{
    public class MvcApplication : HttpApplication
    {
        private IDisposable _environment;

        protected void Application_Start()
        {
            _environment = Bootstrapper
                .WithRavenStore()
                .WithAggregatesIn(typeof (Bootstrapper).Assembly)
                .WithPluginsIn(typeof (MvcApplication).Assembly)
                .Start();

            ControllerBuilder.Current.SetControllerFactory(new StructureMapControllerFactory());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_End()
        {
            _environment.Dispose();
        }
    }

    public class StructureMapControllerFactory : DefaultControllerFactory
    {
        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            return ObjectFactory.GetNamedInstance<IController>(controllerName + "Controller");
        }
    }
}
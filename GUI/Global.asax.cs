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
        protected void Application_Start()
        {
            Bootstrapper
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
    }

    public class StructureMapControllerFactory : DefaultControllerFactory
    {
        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            return ObjectFactory.GetNamedInstance<IController>(controllerName + "Controller");
        }
    }
}
using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CQRS.Sample.Bootstrapping;
using CQRS.Sample.Bus;
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

            ObjectFactory.GetInstance<IServiceBus>().Subscribe<NotificationProjection>();
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

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            Context.InitPrincipal();
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
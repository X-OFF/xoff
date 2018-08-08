using Autofac;
using XOFF.Core;
using XOFF.Core.ChangeQueue;
using XOFF.Core.Remote;
using XOFF.Core.Remote.Http;
using XOFF.Core.Repositories;
using XOFF.iOS.Reachability;

namespace XOFF.iOS.DI
{

    public class XoffAutoFaciOSModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //service locators
            builder.RegisterType<XOFFConnectivityCheckeriOS>().As<IConnectivityChecker>().SingleInstance();
            builder.RegisterType<NsUserDefaultConnectionActivityStore>().As<IConnectionActivityStore>();
            builder.RegisterType<XOFFReachabilityService>().As<IReachabilityService>();
        }
    }
}

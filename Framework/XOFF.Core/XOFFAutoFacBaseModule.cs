using Autofac;
using XOFF.Core;
using XOFF.Core.Repositories;
using XOFF.Core.Settings;
using XOFF.SQLite;

namespace XOFF.Autofac
{

    public class XOFFAutoFacBaseModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
			builder.RegisterType<AutoFacHttpHandlersServiceLocator>().As<IHttpHandlersServiceLocator>();

            builder.RegisterType<XOFFAlwaysOfflineConnectivityChecker>().As<IConnectivityChecker>();//todo replace with real imp
            builder.RegisterGeneric(typeof(SyncedRepository<,>)).As(typeof(ISyncedRepository<,>));
        }
    }
}

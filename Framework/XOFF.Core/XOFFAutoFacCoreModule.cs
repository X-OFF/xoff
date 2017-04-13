using Autofac;
using XOFF.Core;
using XOFF.Core.ChangeQueue;
using XOFF.Core.Remote;
using XOFF.Core.Remote.Http;
using XOFF.Core.Repositories;
using XOFF.Core.Settings;
using XOFF.SQLite;

namespace XOFF.Autofac
{

    public class XOFFAutoFacCoreModule : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AutoFacHttpHandlersServiceLocator>().As<IRemoteHandlersServiceLocator>();

			//service locators
			builder.RegisterType<AutoFacHttpHandlersServiceLocator>().As<IRemoteHandlersServiceLocator>();
			builder.RegisterType<AutofacRepositoryServiceLocator>().As<IRepositoryServiceLocator>();
   
			builder.RegisterType<XOFFAlwaysOfflineConnectivityChecker>().As<IConnectivityChecker>();//todo replace with real imp

            builder.RegisterGeneric(typeof(SyncedRepository<,>)).As(typeof(ISyncedRepository<,>));
            builder.RegisterGeneric(typeof(XOFFChangeQueue<,>)).As(typeof(IChangeQueue<,>));
            builder.RegisterGeneric(typeof(XOFFHttpGetHandler<,>)).As(typeof(IRemoteEntityGetHandler<,>));
            
			builder.RegisterType<QueueProcessor>().As<IQueueProcessor>().SingleInstance();
			builder.RegisterType<XOFFTimerRemoteEntityRefreshCoordinator>().As<IRemoteEntityRefreshCoordinator>().SingleInstance();
		}
    }
}

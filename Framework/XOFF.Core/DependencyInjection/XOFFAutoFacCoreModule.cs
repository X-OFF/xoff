using Autofac;
using XOFF.Core;
using XOFF.Core.ChangeQueue;
using XOFF.Core.Logging;
using XOFF.Core.Remote;
using XOFF.Core.Remote.Http;
using XOFF.Core.Repositories;

namespace XOFF.Autofac
{

    public class XOFFAutoFacCoreModule : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
            //service locators
            builder.RegisterType<AutofacRemoteHandlersServiceLocator>().As<IRemoteHandlersServiceLocator>();
            builder.RegisterType<AutofacRepositoryServiceLocator>().As<IRepositoryServiceLocator>();
        
         
            builder.RegisterGeneric(typeof(SyncedRepository<,>)).As(typeof(ISyncedRepository<,>));
            builder.RegisterGeneric(typeof(XOFFChangeQueue<,>)).As(typeof(IChangeQueue<,>));
            builder.RegisterGeneric(typeof(XOFFGetHandler<,>)).As(typeof(IRemoteEntityGetHandler<,>));
            
			builder.RegisterType<QueueProcessor>().As<IQueueProcessor>().SingleInstance();
			builder.RegisterType<XOFFTimerRemoteEntityRefreshCoordinator>().As<IRemoteEntityRefreshCoordinator>().SingleInstance();

            builder.RegisterType<XOFFFileLogger>().As<IXOFFLogger>();
        }
    }
}

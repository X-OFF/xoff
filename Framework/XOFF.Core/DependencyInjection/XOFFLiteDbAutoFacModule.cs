using Autofac;
using XOFF.Core;
using XOFF.Core.ChangeQueue;
using XOFF.Core.Repositories;
using XOFF.Core.Repositories.Settings;
using XOFF.LiteDB;

namespace XOFF.Autofac
{

	public class XOFFLiteDbAutoFacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<LiteDbConnectionProvider>().As<ILiteDbConnectionProvider>().SingleInstance();
            builder.RegisterType<SyncRepositorySettings>().SingleInstance();
            builder.RegisterType<ChangeQueueSettings>().SingleInstance();
			builder.RegisterGeneric(typeof(LiteDBRepository<,>)).As(typeof(IRepository<,>));

			builder.RegisterModule<XOFFAutoFacCoreModule>();
		}
	}
}

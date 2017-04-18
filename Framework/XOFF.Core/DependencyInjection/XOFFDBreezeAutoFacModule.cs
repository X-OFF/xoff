using Autofac;
using XOFF.Core.Repositories;
using XOFF.Core.Repositories.Settings;
using XOFF.DBreeze;

namespace XOFF.Autofac
{

	public class XOFFDBreezeAutoFacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<DBReezeConnectionProvider>().As<IDBreezeConnectionProvider>().SingleInstance();
			builder.RegisterType<SyncRepositorySettings>().SingleInstance();
			builder.RegisterGeneric(typeof(DBreezeRepository<,>)).As(typeof(IRepository<,>));

			builder.RegisterModule<XOFFAutoFacCoreModule>();
		}
	}
}

using Autofac;
using XOFF.Core;
using XOFF.Core.Repositories;
using XOFF.Core.Settings;
using XOFF.DBreeze;
using XOFF.LiteDB;
using XOFF.SQLite;

namespace XOFF.Autofac
{

	public class XOFFDBreezeAutoFacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<DBReezeConnectionProvider>().As<IDBreezeConnectionProvider>();
			builder.RegisterType<SyncRepositorySettings>().SingleInstance();
			builder.RegisterGeneric(typeof(DBreezeRepository<,>)).As(typeof(IRepository<,>));

			builder.RegisterModule<XOFFAutoFacCoreModule>();
		}
	}
    
}

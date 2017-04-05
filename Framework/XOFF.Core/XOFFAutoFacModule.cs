using Autofac;
using XOFF.Core;
using XOFF.Core.Repositories;
using XOFF.Core.Settings;
using XOFF.SQLite;

namespace XOFF.Autofac
{
    public class XOFFSQLiteAutoFacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
			builder.RegisterType<SQLiteConnectionProvider>().As<ISQLiteConnectionProvider>();
            builder.RegisterType<SyncRepositorySettings>().SingleInstance();
			builder.RegisterGeneric(typeof(SQLiteRepository<,>)).As(typeof(IRepository<,>));

            builder.RegisterModule<XOFFAutoFacCoreModule>();
        }
    }
	
    
}

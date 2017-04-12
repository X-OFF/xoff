using System.IO;
using LiteDB;

namespace XOFF.LiteDB
{
	public class LiteDbConnectionProvider : ILiteDbConnectionProvider
	{

		public LiteDbConnectionProvider()
		{

			//  Database = database;
		}

		public LiteDatabase Database
		{
			get
			{
				var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
				var libraryPath = Path.Combine(documentsPath, "..", "Library");
				var databasePath = Path.Combine(libraryPath, "widgets.liteDb");

				LiteDatabase database = new LiteDatabase(databasePath);
				return database;
			}
		}
	}
}
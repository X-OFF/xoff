using System.IO;
using LiteDB;

namespace XOFF.LiteDB
{
	public class LiteDbConnectionProvider : ILiteDbConnectionProvider
	{
	    private LiteDatabase _database;

	    public LiteDbConnectionProvider()
		{

			//  Database = database;
		}

		public LiteDatabase Database
		{
			get
			{
			    if (_database == null)
			    {
			        var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			        var libraryPath = Path.Combine(documentsPath, "..", "Library");
			        var databasePath = Path.Combine(libraryPath, "widgets.liteDb");

			        _database = new LiteDatabase(databasePath);
			    }
			    return _database;
			}
		}
	}
}
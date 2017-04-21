using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using LiteDB;

namespace XOFF.LiteDB
{
	public class LiteDbConnectionProvider : ILiteDbConnectionProvider
	{
	    private LiteDatabase _database;
        private Semaphore _semaphore;

        public LiteDbConnectionProvider()
		{
            _semaphore = new Semaphore(1, 1);
            //  Database = database;
        }

        public LiteDatabase Database
        {
            get
            {
               if (_database == null)
                {
                   var libraryPath = Path.Combine("/..", "Library");
                    var databasePath = "someWidgets.liteDb";
                    var connStr = $"filename=\"{databasePath}\"; journal =true;";//when this is set to true, file not found exceptions get thrown 
                   
                    _database = new LiteDatabase(connStr);
                }
                return _database;

            }
        }

        public bool WaitOne([CallerMemberName]string name = "")
        {
            Debug.WriteLine(string.Format("###### WAITING SEMAPHORE: {0} ######", name));
            return _semaphore.WaitOne();
        }
        public void Release([CallerMemberName] string name = "")
        {
            Debug.WriteLine(string.Format("###### RELEASING SEMAPHORE: {0} ######", name));
            _semaphore.Release();
        }
    }
}
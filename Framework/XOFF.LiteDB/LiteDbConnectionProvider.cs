using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using LiteDB;
using LiteDB.Platform;
using XOFF.Core.Logging;

namespace XOFF.LiteDB
{
	public class LiteDbConnectionProvider : ILiteDbConnectionProvider
	{
	    protected readonly string _fileName;
	    private LiteDatabase _database;
        private Semaphore _semaphore;

        public LiteDbConnectionProvider(string databaseName = "database")
        {
            _fileName = databaseName;
            _semaphore = new Semaphore(1, 1);
           
        }

        public virtual LiteDatabase Database
        {
            get
            {
               if (_database == null)
                {
                    var databasePath = $"{_fileName}.liteDb";
                    var connStr = $"filename={databasePath}; journal =true;";//when this is set to true, file not found exceptions get thrown 
                   	LitePlatform.Initialize(new LitePlatformiOS());
                    
					var mapper = BsonMapper.Global;
					mapper.SerializeNullValues = true;
					_database = new LiteDatabase(connStr);
                }
                return _database;

            }
        }

        public bool WaitOne([CallerMemberName]string name = "")
        {
            //XOFFLoggerSingleton.Instance.LogMessage("ldb cp wait one",string.Format("{0} WAITING", name)); 
            return _semaphore.WaitOne();
        }
        public void Release([CallerMemberName] string name = "")
        {
			try
			{
                //XOFFLoggerSingleton.Instance.LogMessage("ldb cp release", string.Format("{0} SEMAPHORE", name));
				_semaphore.Release();
			}
			catch (Exception ex) 
			{
				_semaphore = new Semaphore(1, 1);
				XOFFLoggerSingleton.Instance.LogMessage("ldb cp release", string.Format("Released Too Many Times", name));

			}
        }
    }
}

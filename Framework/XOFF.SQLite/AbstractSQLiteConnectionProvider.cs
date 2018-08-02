using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using SQLite.Net;

namespace XOFF.SQLite
{
    public abstract class AbstractSQLiteConnectionProvider : ISQLiteConnectionProvider
    {

        public AbstractSQLiteConnectionProvider()
        {
            _semaphore = new Semaphore(1, 1);
        }

        /*
            * Note(Jackson) if you are using a sqlite transaction in this application you need to use this functionality. 
            * 
            * Calling wait one will wait for any other request that has requested a wait to then run its transaction and then released the semaphore 
            * 
            * example 
            * 
            *    try
                    {
                        var exists = existingIds.Contains(item.Id);
                        _connectionProvider.WaitOne();
                        Connection.RunInTransaction(() =>
                        {
                            UpsertObject(item, mappings, insert: !exists);
                        });                
                    }
                    finally
                    {
                        _connectionProvider.Release(); // !IMPORTANT this must be in the finally because if an exception is thrown in your transaction you can cause a deadlock by not releasing the semaphore
                    }
            */
        public bool WaitOne([CallerMemberName]string name = "")
        {
            Debug.WriteLine(string.Format("###### WAITING SEMAPHORE: {0} ######", name));
            return _semaphore.WaitOne();
        }

        public void RunInTransaction(Action actionToRun)
        {
            Connection.RunInTransaction(actionToRun);
        }

        public void Release([CallerMemberName] string name = "")
        {
            Debug.WriteLine(string.Format("###### RELEASING SEMAPHORE: {0} ######", name));
            _semaphore.Release();
        }

        public void Dispose()
        {
            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
            }
            if (_semaphore != null)
            {
                _semaphore = null;
            }
        }

        private Semaphore _semaphore;
        public abstract SQLiteConnection Connection { get; }
        
        public string DbPath { get; set; }
    }
}
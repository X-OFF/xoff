using System;
using System.Runtime.CompilerServices;
using SQLite.Net;

namespace XOFF.SQLite
{
    public interface ISQLiteConnectionProvider: IDisposable
    {
        SQLiteConnection Connection { get; }
        bool WaitOne([CallerMemberName]string name = "");
        void Release([CallerMemberName]string name = "");
        void RunInTransaction(Action actionToRun);

      
    }
}
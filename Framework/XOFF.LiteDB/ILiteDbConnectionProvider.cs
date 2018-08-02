using System.Runtime.CompilerServices;
using LiteDB;

namespace XOFF.LiteDB
{
	public interface ILiteDbConnectionProvider
	{
		LiteDatabase Database { get; }

        void Release([CallerMemberName] string name = "");
        bool WaitOne([CallerMemberName]string name = "");
    }
}
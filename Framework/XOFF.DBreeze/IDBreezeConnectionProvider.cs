using System.Runtime.CompilerServices;
using DBreeze;

namespace XOFF.DBreeze
{
	public interface IDBreezeConnectionProvider
	{
		DBreezeEngine Engine { get; }
	    void Release([CallerMemberName] string name = "");
	    bool WaitOne([CallerMemberName]string name = "");
	}
}
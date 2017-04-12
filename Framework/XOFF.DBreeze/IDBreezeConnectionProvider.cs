using DBreeze;

namespace XOFF.DBreeze
{
	public interface IDBreezeConnectionProvider
	{
		DBreezeEngine Engine { get; }
	}
}
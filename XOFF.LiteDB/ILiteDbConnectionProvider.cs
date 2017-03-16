using LiteDB;

namespace XOFF.LiteDB
{
	public interface ILiteDbConnectionProvider
	{
		LiteDatabase Database { get; }
	}
}
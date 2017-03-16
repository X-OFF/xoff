using LiteDB;

namespace XOFF.LiteDB
{
    public class LiteDbConnectionProvider : ILiteDbConnectionProvider
    {

        public LiteDbConnectionProvider(LiteDatabase database)
        {
            Database = database;
        }

        public LiteDatabase Database { get; }
    }
}
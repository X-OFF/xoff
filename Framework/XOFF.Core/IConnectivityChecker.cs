namespace XOFF.Core
{
    public interface IConnectivityChecker
    {
        bool Connected { get; }
    }

    public class XOFFAlwaysOfflineConnectivityChecker : IConnectivityChecker
    {
        public bool Connected {
            get { return false; }
        }
    }
}
using System;

namespace XOFF.Core
{
    public interface IConnectivityChecker : IDisposable
    {
        bool Connected { get; }
    }

    public class XOFFAlwaysOfflineConnectivityChecker : IConnectivityChecker
    {
        public bool Connected {
            get { return false; }
        }

        public void Dispose()
        {
        }
    }
}
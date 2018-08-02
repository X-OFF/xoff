using System;
using System.Net;
using SystemConfiguration;
using CoreFoundation;
using Foundation;
using XOFF.Core;
using Plugin.Connectivity;

namespace XOFF.iOS.Reachability
{
    /// <summary>
    /// https://github.com/xamarin/monotouch-samples/blob/master/ReachabilitySample/reachability.cs
    /// </summary>
    public class XOFFConnectivityCheckeriOS : IConnectivityChecker
    {
        public bool Connected
        {
            get { 
				return CrossConnectivity.Current.IsConnected; 
			}
        }

		public void Dispose()
		{
			
		}
	}
}
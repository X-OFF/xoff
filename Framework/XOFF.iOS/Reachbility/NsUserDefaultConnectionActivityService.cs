using System;
using Foundation;
using Newtonsoft.Json;
using Xamarin.ObjCRuntime;

namespace XOFF.iOS.Reachability
{
    public class NsUserDefaultConnectionActivityStore : IConnectionActivityStore
    {
        private DateFormatHandling _dateFormatDandling = DateFormatHandling.MicrosoftDateFormat;
        private JsonSerializerSettings _serializerSettings;


        public NsUserDefaultConnectionActivityStore()
        {
            _serializerSettings = new JsonSerializerSettings()
            {
                DateFormatHandling = _dateFormatDandling
            };
        }



        public void SetPing()
        {
            var date = DateTime.UtcNow;
            var dateString = JsonConvert.SerializeObject(date, _serializerSettings);
            NSUserDefaults.StandardUserDefaults.SetString(dateString, PingKey);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        private const string PingKey = "XOFF_MOST_Recent_Ping";


        public DateTime GetMostRecentPing()
        {
            var pingString = NSUserDefaults.StandardUserDefaults.StringForKey(PingKey);
            if (pingString == null)
            {
                return DateTime.MinValue;

            }
            var ping = JsonConvert.DeserializeObject<DateTime>(pingString, _serializerSettings);
            return ping;
        }
    }

    public interface IConnectionActivityStore
    {
        void SetPing();
        DateTime GetMostRecentPing();
    }

    public enum NetworkStatus
    {
        NotReachable = 0,
        ReachableViaCarrierDataNetwork = 1,
        ReachableViaWiFiNetwork = 2
    }
}
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using XOFF.Core;

namespace XOFF.iOS.Reachability
{
    public interface IReachabilityService
    {
        Task<bool> IsReachable();
    }

    public class XOFFReachabilityService : IReachabilityService
    {
        private readonly string _url;

        public XOFFReachabilityService()
        {
        }

        public XOFFReachabilityService(string url)
        {
            _url = url;
        }

        public async Task<bool> IsReachable()
        {
            if (string.IsNullOrEmpty(_url))
            {
                return true;//if not set don't worry about it, the default state in DI is to not set this, the user of XOFF will inject a URL parameter if they want this to be used
            }

            try
            {
                using (var client = GetClient())
                {
                    var response = await client.GetAsync("");
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
               // does this neede to be an operation result todo? 
            }
            return false;
        }
        public HttpClient GetClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(_url);
            return client;
        }
    }
}
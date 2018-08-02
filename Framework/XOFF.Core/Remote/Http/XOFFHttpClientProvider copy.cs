using System;
using System.Net.Http;

namespace XOFF.Core.Remote.Http
{
    public class XOFFHttpClientProvider : IHttpClientProvider
    {
        protected string BaseUrl;

        public XOFFHttpClientProvider(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public HttpClient GetClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(BaseUrl);
            return client;
        }

        public void SetBaseUrl(string baseUrl)
        {
            BaseUrl = baseUrl;
        }
    }
}
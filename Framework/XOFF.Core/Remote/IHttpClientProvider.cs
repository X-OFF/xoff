using System;
using System.Net.Http;

namespace XOFF.Core.Remote
{
	public interface IHttpClientProvider
	{
		HttpClient GetClient();
	}

    public class XOFFHttpClientProvider : IHttpClientProvider
    {
        private readonly string _baseUrl;

        public XOFFHttpClientProvider(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public HttpClient GetClient()
        {  
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(_baseUrl);
            return client;
        }
    }
}
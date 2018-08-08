using System;
using System.Net.Http;
using System.Net.Http.Headers;
using ModernHttpClient;

namespace XOFF.Core.Remote.Http
{
    public class XOFFHttpClientProvider : IHttpClientProvider
    {
        protected string BaseUrl;
		protected string _token;
        private string lastBaseUrl;
        protected HttpClient client;

		public XOFFHttpClientProvider(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public HttpClient GetClient()
        {
            if (client == null && string.Compare(lastBaseUrl,BaseUrl,StringComparison.CurrentCultureIgnoreCase) != 0)
            {
                client = new HttpClient(new NativeMessageHandler());
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(
					new MediaTypeWithQualityHeaderValue("application/json"));
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
				client.BaseAddress = new Uri(BaseUrl);
            }
           
          
            return client;
        }

        public void SetBaseUrl(string baseUrl)
        {
			BaseUrl = baseUrl;
        }

		public void SetToken(string token)
		{
			_token = token;
		}
	}
}
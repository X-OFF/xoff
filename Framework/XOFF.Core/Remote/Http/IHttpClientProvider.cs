using System;
using System.Net.Http;

namespace XOFF.Core.Remote
{
	public interface IHttpClientProvider
	{
		HttpClient GetClient();
		void SetBaseUrl(string baseUrl);
		void SetToken(string token);
	}


}
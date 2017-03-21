using System.Net.Http;

namespace XOFF.Core.Remote
{
	public interface IHttpClientProvider
	{
		HttpClient GetClient();
	}
}
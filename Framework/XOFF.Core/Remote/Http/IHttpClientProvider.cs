using System.Net.Http;

namespace XOFF.Core.Remote.Http
{
    public interface IHttpClientProvider
    {
        HttpClient GetClient();
    }
}
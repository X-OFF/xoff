using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using XOFF.Core.ChangeQueue;

namespace XOFF.Core.Remote.Http
{
    public class XOFFHttpEntityUpdateHandler<TModel, TIdentifier> : IRemoteEntityUpdateHandler<TModel, TIdentifier> where TModel : IModel<TIdentifier>
    {
        readonly string _endpointUri;
        private readonly IHttpClientProvider _httpClientProvider;

        public XOFFHttpEntityUpdateHandler(IHttpClientProvider httpClientProvider, string endpointUri)
        {
            _endpointUri = endpointUri;
            _httpClientProvider = httpClientProvider;
        }

        public async Task<OperationResult> Update(ChangeQueueItem queueItem)
        {
            try
            {
                using (var client = _httpClientProvider.GetClient())
                {
                    var response = await client.PutAsync(_endpointUri, new StringContent(queueItem.ChangedItemJson, Encoding.UTF8, "application/json"));

                    if (response.IsSuccessStatusCode)
                    {
                        var itemJson = await response.Content.ReadAsStringAsync();
                        return OperationResult.CreateSuccessResult(itemJson);
                    }
                    else
                    {
                        return OperationResult.CreateFailure(response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(ex);
            }
        }
    }
}
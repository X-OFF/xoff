using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using XOFF.Core.ChangeQueue;

namespace XOFF.Core.Remote.Http
{
    public class XOFFHttpEntityCreateHandler<TModel, TIdentifier> : IRemoteEntityCreateHandler<TModel, TIdentifier> where TModel : IModel<TIdentifier>
    {
        readonly string _endpointUri;
        private readonly IHttpClientProvider _httpClientProvider; 

        public XOFFHttpEntityCreateHandler(IHttpClientProvider httpClientProvider, string endpointUri) 
        {
            _endpointUri = endpointUri;
            _httpClientProvider = httpClientProvider;
        }

        public async Task<OperationResult<string>> Create(ChangeQueueItem queueItem)
        {
            try
            {
                using (var client = _httpClientProvider.GetClient())
                {
                    var response = await client.PostAsync(_endpointUri, new StringContent(queueItem.ChangedItemJson, Encoding.UTF8, "application/json" ));
                    var itemJson = await response.Content.ReadAsStringAsync();
                    return OperationResult<string>.CreateSuccessResult(itemJson);
                }
            }
            catch (Exception ex) 
            {
                return OperationResult<string>.CreateFailure(ex);
            }
        }
    }
}
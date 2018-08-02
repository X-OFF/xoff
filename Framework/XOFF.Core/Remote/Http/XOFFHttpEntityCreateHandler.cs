using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using XOFF.Core.ChangeQueue;
using XOFF.Core.Logging;

namespace XOFF.Core.Remote.Http
{
    public class XOFFHttpEntityCreateHandler<TModel, TIdentifier> : IRemoteEntityCreateHandler<TModel, TIdentifier> where TModel : IModel<TIdentifier>
    {
      
        private readonly IHttpClientProvider _httpClientProvider;
        private string _endpointUrl;
        protected virtual string EndpointUrl => _endpointUrl;
        public XOFFHttpEntityCreateHandler(IHttpClientProvider httpClientProvider, string endpointUri) 
        {
            _endpointUrl = endpointUri;
            _httpClientProvider = httpClientProvider;
        }

        public virtual async Task<XOFFOperationResult<string>> Create(ChangeQueueItem queueItem)
        {
            try
            {
                var client = _httpClientProvider.GetClient();

                    var response = await client.PostAsync(EndpointUrl, new StringContent(queueItem.ChangedItemJson, Encoding.UTF8, "application/json" ));

                    if (response.IsSuccessStatusCode)
                    {
                        var itemJson = await response.Content.ReadAsStringAsync();
                        return XOFFOperationResult<string>.CreateSuccessResult(itemJson);
                    }
                    else
                    {
						XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityCreateHandler<typeof(TModel).FullName>",$"Creation Failed for type{typeof(TModel).FullName}");
						XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityCreateHandler<typeof(TModel).FullName>",response.Content.ReadAsStringAsync().Result);
						XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityCreateHandler<typeof(TModel).FullName>",response.RequestMessage.ToString());
                        return XOFFOperationResult<string>.CreateFailure(response.ReasonPhrase);
                    }

            }
            catch (Exception ex) 
            {
				XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityCreateHandler<typeof(TModel).FullName>",$"Creation Failed for type{typeof(TModel).FullName}");
				XOFFLoggerSingleton.Instance.LogException($"XoffHttpEntityCreateHandler<typeof(TModel).FullName>",ex, XOFFErrorSeverity.Warning);
                return XOFFOperationResult<string>.CreateFailure(ex);
            }
        }
    }
}
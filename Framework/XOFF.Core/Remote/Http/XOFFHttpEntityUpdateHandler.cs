using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using XOFF.Core.ChangeQueue;
using XOFF.Core.Logging;

namespace XOFF.Core.Remote.Http
{
    public class XOFFHttpEntityUpdateHandler<TModel, TIdentifier> : IRemoteEntityUpdateHandler<TModel, TIdentifier> where TModel : IModel<TIdentifier>
    {
        readonly string _endpointUri;
        private readonly bool _usePost;
        private readonly bool _useIdBasedFormatString;
        private readonly IHttpClientProvider _httpClientProvider;

        public XOFFHttpEntityUpdateHandler(IHttpClientProvider httpClientProvider, string endpointUri, bool usePost = false, bool useIdBasedFormatString = false)
        {
            _endpointUri = endpointUri;
            _usePost = usePost;
            _useIdBasedFormatString = useIdBasedFormatString;
            _httpClientProvider = httpClientProvider;
        }

        public async Task<XOFFOperationResult> Update(ChangeQueueItem queueItem)
        {
            try
            {
                var client = _httpClientProvider.GetClient();

					var endpoint = _useIdBasedFormatString ? string.Format(_endpointUri, queueItem.ChangedItemId) : _endpointUri;
                    HttpResponseMessage response;
                    if (_usePost)
                    {
						response = await client.PostAsync(endpoint,
                            new StringContent(queueItem.ChangedItemJson, Encoding.UTF8, "application/json"));
                    }
                    else
                    {
                        response = await client.PutAsync(endpoint,
                            new StringContent(queueItem.ChangedItemJson, Encoding.UTF8, "application/json"));
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        var itemJson = await response.Content.ReadAsStringAsync();
                        return XOFFOperationResult.CreateSuccessResult(itemJson);
                    }
                    else
                    {
						XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityGetter<typeof(TModel).FullName>",$"Update Failed for type{typeof(TModel).FullName}");
						XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityGetter<typeof(TModel).FullName>",response.Content.ReadAsStringAsync().Result);
						XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityGetter<typeof(TModel).FullName>",response.RequestMessage.ToString());

                        return XOFFOperationResult.CreateFailure(response.ReasonPhrase);
                    }

            }
            catch (Exception ex)
            {
				XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityGetter<typeof(TModel).FullName>",$"Update Failed for type{typeof(TModel).FullName}");
				XOFFLoggerSingleton.Instance.LogException($"XoffHttpEntityGetter<typeof(TModel).FullName>",ex, XOFFErrorSeverity.Warning);
				
                return XOFFOperationResult.CreateFailure(ex);
            }
        }
    }
}
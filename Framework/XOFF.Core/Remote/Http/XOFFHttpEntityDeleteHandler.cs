using System;
using System.Diagnostics;
using System.Threading.Tasks;
using XOFF.Core.ChangeQueue;
using XOFF.Core.Logging;

namespace XOFF.Core.Remote.Http
{
    public class XOFFHttpEntityDeleteHandler<TModel, TIdentifier> : IRemoteEntityDeleteHandler<TModel, TIdentifier> where TModel : IModel<TIdentifier>
    {
        
        private readonly IHttpClientProvider _httpClientProvider;
        private readonly string _endPointFormatString;

        public XOFFHttpEntityDeleteHandler(IHttpClientProvider httpClientProvider, string endPointFormatString)
        {
            _httpClientProvider = httpClientProvider;
            _endPointFormatString = endPointFormatString;
        }

        public async Task<XOFFOperationResult> Delete(ChangeQueueItem queueItem)
        {
            try
            {
                var client = _httpClientProvider.GetClient();
				
					var endPoint = string.Format(_endPointFormatString, queueItem.ChangedItemId);
					var response = await client.DeleteAsync(endPoint);

                    if (response.IsSuccessStatusCode)
                    {

                        return XOFFOperationResult.CreateSuccessResult($"Successfully Deleted item {queueItem.ChangedItemId}");
                    }
                    else
                    {
                        XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityDeleteHandler<{typeof(TModel).FullName}>",$"Creation Failed for type{typeof(TModel).FullName}");
                        XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityDeleteHandler<{typeof(TModel).FullName}>",response.Content.ReadAsStringAsync().Result);
                        XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityDeleteHandler<{typeof(TModel).FullName}>",response.RequestMessage.ToString());
                        return XOFFOperationResult.CreateFailure(response.ReasonPhrase);
                    }
                
            }
            catch (Exception ex)
            {
				XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityDeleteHandler<typeof(TModel).FullName>",$"Creation Failed for type{typeof(TModel).FullName}");//todo fix this line 
				XOFFLoggerSingleton.Instance.LogException($"XoffHttpEntityDeleteHandler<typeof(TModel).FullName>",ex, XOFFErrorSeverity.Warning);
				
                return XOFFOperationResult.CreateFailure(ex);
            }
        }
    }
}
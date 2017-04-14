using System;
using System.Threading.Tasks;
using XOFF.Core.ChangeQueue;

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

        public async Task<OperationResult> Delete(ChangeQueueItem queueItem)
        {
            try
            {
                using (var client = _httpClientProvider.GetClient())
                {
                    var response = await client.DeleteAsync(string.Format(_endPointFormatString, queueItem.ChangedItemId));

                    if (response.IsSuccessStatusCode)
                    {

                        return OperationResult.CreateSuccessResult($"Successfully Deleted item {queueItem.ChangedItemId}");
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
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XOFF.Core.ChangeQueue;

namespace XOFF.Core.Remote
{

	public interface IHttpEntityCreateHandler<TModel, TIdentifier> : IHttpCreateHandler where TModel : IModel<TIdentifier>
	{
		new Task<OperationResult<string>> Create(ChangeQueueItem model);
	}

	public class XOFFHttpEntityCreateHandler<TModel, TIdentifier> : IHttpEntityCreateHandler<TModel, TIdentifier> where TModel : IModel<TIdentifier>
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XOFF.Core.Logging;

namespace XOFF.Core.Remote.Http
{

	public class XOFFHttpEntityGetter<TModel, TIdentifier> : IRemoteEntityGetter<TModel, TIdentifier> where TModel : IModel<TIdentifier>
	{
		protected readonly IHttpClientProvider _httpClientProvider;
		private readonly string _getAllEndpointUri;
		private readonly string _getOneFormatString;



		/// <summary>
		/// /
		/// </summary>
		/// <param name="httpClientProvider"></param>
		/// <param name="getAllEndPointUri"></param>
		/// <param name="getOneFormatString">A format string that will be used to create the url for the get request. Example "widgets/{0}" </param>
		public XOFFHttpEntityGetter(IHttpClientProvider httpClientProvider, string getAllEndPointUri, string getOneFormatString)
		{
			_httpClientProvider = httpClientProvider;
			_getAllEndpointUri = getAllEndPointUri;
			_getOneFormatString = getOneFormatString;
		}

		protected virtual string GetAllEndpoint => _getAllEndpointUri;

		protected virtual string BuildGetOneEndpoint(string id)
		{
			return string.Format(_getOneFormatString, id);
		}

		public virtual async Task<XOFFOperationResult<IList<TModel>>> Get()
		{
			try
			{
                var client = _httpClientProvider.GetClient();

					var endpoint = GetAllEndpoint;
                    XOFFLoggerSingleton.Instance.LogMessage("Xoff http get handler",$"Get {typeof(TModel)} started");
					var response = await client.GetAsync(endpoint);
					XOFFLoggerSingleton.Instance.LogMessage("Xoff http get handler", $"Get {typeof(TModel)} responded");
					if (response.IsSuccessStatusCode)
					{
						var itemJson = await response.Content.ReadAsStringAsync();
						IList<TModel> items = SerializeListOfitems(itemJson);
						for (int i = 0; i < items.Count; i++)
						{
							items[i].ApiSortOrder = i;
						}
                        items = await GetCompleted(items);
                        return XOFFOperationResult<IList<TModel>>.CreateSuccessResult(items);
					}
					else
					{
						XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityGetter<typeof(TModel).FullName>",$"get Failed for type{typeof(TModel).FullName}");
						XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityGetter<typeof(TModel).FullName>",response.Content.ReadAsStringAsync().Result);
						XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityGetter<typeof(TModel).FullName>",response.RequestMessage.ToString());

						return XOFFOperationResult<IList<TModel>>.CreateFailure(response.ReasonPhrase);//todo return http result
					}
				
			}
			catch (Exception ex)
			{
				XOFFLoggerSingleton.Instance.LogMessage($"XoffHttpEntityGetter<typeof(TModel).FullName>",$"get Failed for type{typeof(TModel).FullName}");
				XOFFLoggerSingleton.Instance.LogException($"XoffHttpEntityGetter<typeof(TModel).FullName>",ex, XOFFErrorSeverity.Warning);

				return XOFFOperationResult<IList<TModel>>.CreateFailure(ex);
			}
		}

        /// <summary>
        /// This method is here because the method is virtual to allow a call back
        /// This base implementation is essentially "Do nothing" in a task and give the list back 
        /// </summary>
        /// <returns>The completed.</returns>
        /// <param name="items">Items.</param>
        protected virtual async Task<IList<TModel>> GetCompleted(IList<TModel> items)
        {
            await Task.FromResult(new object());
            return items; 
        }

        public virtual List<TModel> SerializeListOfitems(string itemJson)
        {
            return JsonConvert.DeserializeObject<List<TModel>>(itemJson);
        }

        public virtual async Task<XOFFOperationResult<TModel>> Get(TIdentifier id)
		{
			try
			{
                var client = _httpClientProvider.GetClient();
				
					var endPoint = BuildGetOneEndpoint(id.ToString());
					var response = await client.GetAsync(endPoint);
					if (response.IsSuccessStatusCode)
					{
						var itemJson = await response.Content.ReadAsStringAsync();
						TModel items = SerializeOne(itemJson);
						return XOFFOperationResult<TModel>.CreateSuccessResult(items);
					}
					else 
					{
						return XOFFOperationResult<TModel>.CreateFailure(response.ReasonPhrase);//todo return http result
					}
				
			}
			catch (Exception ex)
			{
				return XOFFOperationResult<TModel>.CreateFailure(ex);
			}
		}

		public virtual TModel SerializeOne(string itemJson)
		{
			return JsonConvert.DeserializeObject<TModel>(itemJson);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace XOFF.Core.Remote.Http
{
    
    public class XOFFHttpEntityGetter<TModel, TIdentifier> : IRemoteEntityGetter<TModel, TIdentifier> where TModel : IModel<TIdentifier>
    {
        private readonly IHttpClientProvider _httpClientProvider;
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

        public async Task<OperationResult<IList<TModel>>> Get()
        {
            try
            {
                using (var client = _httpClientProvider.GetClient())
                {
                    var response = await client.GetAsync(_getAllEndpointUri);
                    var itemJson = await response.Content.ReadAsStringAsync();
                    var items = JsonConvert.DeserializeObject<List<TModel>>(itemJson);
                    return OperationResult<IList<TModel>>.CreateSuccessResult(items);
                }
            }
            catch (Exception ex)
            {
                return OperationResult<IList<TModel>>.CreateFailure(ex);
            }
        }

        public async Task<OperationResult<TModel>> Get(TIdentifier id)
        {
            try
            {
                using (var client = _httpClientProvider.GetClient())
                {
                    var response = await client.GetAsync(string.Format(_getOneFormatString, id));
                    var itemJson = await response.Content.ReadAsStringAsync();
                    var items = JsonConvert.DeserializeObject<TModel>(itemJson);
                    return OperationResult<TModel>.CreateSuccessResult(items);
                }
            }
            catch (Exception ex)
            {
                return OperationResult<TModel>.CreateFailure(ex);
            }
        }
    }
}
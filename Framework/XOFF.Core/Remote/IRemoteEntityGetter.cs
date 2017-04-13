using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace XOFF.Core.Remote
{
    public interface IRemoteEntityGetter<TModel, TIdentifier>  where TModel : IModel<TIdentifier>
    {
        Task<OperationResult<IList<TModel>>> Get();
        Task<OperationResult<TModel>> Get(TIdentifier id);
    }
}
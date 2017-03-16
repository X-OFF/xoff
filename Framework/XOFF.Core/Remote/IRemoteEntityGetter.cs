using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XOFF.Core;

using System.Threading.Tasks;

namespace XOFF.Core.Remote
{
    public interface IRemoteEntityGetter<TModel, TIdentifier> where TModel : IModel<TIdentifier>
    {
        Task<OperationResult<IList<TModel>>> Get();
        Task<OperationResult<TModel>> Get(Guid id);

    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OfflineFirstReferenceArch.Models;
using XOFF.Core;
using XOFF.Core.Remote;

namespace OfflineFirstReferenceArch.Widgets
{
    public class RemoteWidgetGetter: IRemoteEntityGetter<Widget, Guid> 
    {
        public Task<OperationResult<IList<Widget>>> Get()
        {
            return Task.Run(() => { return OperationResult<IList<Widget>>.CreateSuccessResult(new List<Widget>()); });//todo implement 
        }

        public Task<OperationResult<Widget>> Get(Guid id)
        {
            return Task.Run(() => { return OperationResult<Widget>.CreateSuccessResult(new Widget(){Id = id}); });//todo implement 
        }
    }
}
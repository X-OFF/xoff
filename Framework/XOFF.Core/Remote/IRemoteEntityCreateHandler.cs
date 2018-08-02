using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XOFF.Core.ChangeQueue;

namespace XOFF.Core.Remote
{

	public interface IRemoteEntityCreateHandler<TModel, TIdentifier> : IRemoteCreateHandler where TModel : IModel<TIdentifier>
	{
		new Task<XOFFOperationResult<string>> Create(ChangeQueueItem model);
	}

    
}
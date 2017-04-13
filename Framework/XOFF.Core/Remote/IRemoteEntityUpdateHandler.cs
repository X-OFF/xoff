using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XOFF.Core.Remote
{

	public interface IRemoteEntityUpdateHandler<TModel, TIdentifier> : IRemoteUpdateHandler where TModel : IModel<TIdentifier>
	{
		Task<OperationResult> Update(TModel model);
	}
}
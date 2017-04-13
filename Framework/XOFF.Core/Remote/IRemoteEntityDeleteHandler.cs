using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XOFF.Core.Remote
{

	public interface IRemoteEntityDeleteHandler<TModel, TIdentifier> : IRemoteDeleteHandler where TModel : IModel<TIdentifier>
	{
		Task<OperationResult> Delete(TModel model);
		Task<OperationResult> DeleteById(TIdentifier id);
	}
	
}
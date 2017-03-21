using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XOFF.Core.Remote
{

	public interface IHttpEntityUpdateHandler<TModel, TIdentifier> : IHttpUpdateHandler where TModel : IModel<TIdentifier>
	{
		Task<OperationResult> Update(TModel model);
	}
}
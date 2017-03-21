using System;
using XOFF.Core.Repositories;

namespace XOFF.Core.ChangeQueue
{
	public interface IChangeQueue<TModel, TIdentifier> where TModel : IModel<TIdentifier>
	{
		OperationResult QueueCreate(TModel model);
		OperationResult QueueUpdate(TModel model);
		OperationResult QueueDelete(TIdentifier id);
		OperationResult QueueDelete(TModel model);
	}
}

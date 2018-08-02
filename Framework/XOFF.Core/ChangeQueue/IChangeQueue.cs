using System;
using XOFF.Core.Repositories;

namespace XOFF.Core.ChangeQueue
{
	public interface IChangeQueue<TModel, TIdentifier> where TModel : IModel<TIdentifier>
	{
		XOFFOperationResult QueueCreate(TModel model, string createJson = null);
		XOFFOperationResult QueueUpdate(TModel model);
		XOFFOperationResult QueueDelete(TIdentifier localId, string id);
		XOFFOperationResult QueueDelete(TModel model);
	}
}

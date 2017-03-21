using System;
using Newtonsoft.Json;
using XOFF.Core.Repositories;

namespace XOFF.Core.ChangeQueue
{

	public class XOFFChangeQueue<TModel, TIdentifier> : IChangeQueue<TModel, TIdentifier> where TModel : IModel<TIdentifier>
	{
		readonly IRepository<ChangeQueueItem, Guid> _repository;
	

		public XOFFChangeQueue(IRepository<ChangeQueueItem,Guid> repository) 
		{
			_repository = repository;
		
		}

		public OperationResult QueueCreate(TModel model)
		{
			try
			{
				var queueItem = CreateQueueItem(model.Id.ToString(), JsonConvert.SerializeObject(model), ChangeTypeStrings.Created);
				_repository.Upsert(queueItem);
				return OperationResult.CreateSuccessResult("Successfully Queued");
			}
			catch (Exception ex) 
			{
				return OperationResult.CreateFailure(ex);
			}
		}

		public OperationResult QueueDelete(TModel model)
		{
			try
			{
				var queueItem = CreateQueueItem(model.Id.ToString(),JsonConvert.SerializeObject(model) ,ChangeTypeStrings.Deleted);
				_repository.Upsert(queueItem);
				return OperationResult.CreateSuccessResult("Successfully Queued");
			}
			catch (Exception ex)
			{
				return OperationResult.CreateFailure(ex);
			}
		}

		public OperationResult QueueDelete(TIdentifier id)
		{
			try
			{
				var queueItem = CreateQueueItem(id.ToString(), null,  ChangeTypeStrings.Deleted);
				_repository.Upsert(queueItem);
				return OperationResult.CreateSuccessResult("Successfully Queued");
			}
			catch (Exception ex)
			{
				return OperationResult.CreateFailure(ex);
			}
		}

		public OperationResult QueueUpdate(TModel model)
		{
			try
			{
				var queueItem = CreateQueueItem(model.Id.ToString(), JsonConvert.SerializeObject(model), ChangeTypeStrings.Deleted);
				_repository.Upsert(queueItem);
				return OperationResult.CreateSuccessResult("Successfully Queued");
			}
			catch (Exception ex)
			{
				return OperationResult.CreateFailure(ex);
			}
		}

		private ChangeQueueItem CreateQueueItem(string modelId, string modelJson , string changeType) 
		{
			var queueItem = new ChangeQueueItem()
			{
				Id = Guid.NewGuid(),
				LastTimeSynced = DateTime.UtcNow,
				ChangedItemId = modelId,
				ChangeType = changeType,
				ChangedItemJson = modelJson
			};

			return queueItem;
		}
	}
}

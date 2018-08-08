using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using XOFF.Core.Logging;
using XOFF.Core.Repositories;
using XOFF.LiteDB;

namespace XOFF.Core.ChangeQueue
{

	public class XOFFChangeQueue<TModel, TIdentifier> : IChangeQueue<TModel, TIdentifier> where TModel : IModel<TIdentifier>
	{
		readonly IRepository<ChangeQueueItem, Guid> _repository;
        private readonly ChangeQueueSettings _queueSettings;

        public XOFFChangeQueue(IRepository<ChangeQueueItem,Guid> repository, ChangeQueueSettings queueSettings) 
		{
			_repository = repository;
            _queueSettings = queueSettings;
        }

		public XOFFOperationResult QueueCreate(TModel model, string createJson = null)
		{
			try
			{
			    if (createJson == null)
			    {
			        createJson = JsonConvert.SerializeObject(model);
                }

				var queueItem = CreateQueueItem(model.RemoteId, createJson, ChangeTypeStrings.Created, model.LocalId.ToString());
                return CheckForDuplicatesAndUpsert(model, queueItem).ToOperationResult();
			}
			catch (Exception ex) 
			{
				return XOFFOperationResult.CreateFailure(ex);
			}
		}

	    private XOFFOperationResult<ChangeQueueItem> CheckForDuplicatesAndUpsert(TModel model, ChangeQueueItem queueItem)
	    {


	        var existingQueueItemResult = _repository
                .All(x => x.ChangedItemLocalId == model.LocalId.ToString() && x.FailedAttempts < _queueSettings.FailedAttemptLimit && !x.SuccessfullyProcessed);
            
            if (queueItem.ChangeType != ChangeTypeStrings.Deleted && existingQueueItemResult.Success && existingQueueItemResult.Result.Any())// deletes should not be grouped creates and updates should be grouped
	        {
	            var existing = existingQueueItemResult.Result.FirstOrDefault();
                if (existing != null && existing.ChangeItemTypeString != ChangeTypeStrings.Deleted)// existing deletes should not be merged
	            {
	                existing.ChangedItemJson = queueItem.ChangedItemJson;
	                queueItem = existing;
	            }
	        }
	       return  _repository.Upsert(queueItem);
	    }

	    public XOFFOperationResult QueueDelete(TModel model)
		{
			try
			{
				var queueItem = CreateQueueItem(model.RemoteId,JsonConvert.SerializeObject(model) ,ChangeTypeStrings.Deleted, model.LocalId.ToString());
				CheckForDuplicatesAndUpsert(model,queueItem);
				return XOFFOperationResult.CreateSuccessResult("Successfully Queued");
			}
			catch (Exception ex)
			{
				return XOFFOperationResult.CreateFailure(ex);
			}
		}

		public XOFFOperationResult QueueDelete(TIdentifier localId, string id)
		{
			try
			{
				var queueItem = CreateQueueItem(id, string.Empty,  ChangeTypeStrings.Deleted, localId.ToString());
				_repository.Upsert(queueItem);
				return XOFFOperationResult.CreateSuccessResult("Successfully Queued");
			}
			catch (Exception ex)
			{
				return XOFFOperationResult.CreateFailure(ex);
			}
		}

		public XOFFOperationResult QueueUpdate(TModel model)
		{
			try
			{
				var queueItem = CreateQueueItem(model.RemoteId, JsonConvert.SerializeObject(model), ChangeTypeStrings.Updated, model.LocalId.ToString());
			    CheckForDuplicatesAndUpsert(model, queueItem);
				return XOFFOperationResult.CreateSuccessResult("Successfully Queued");
			}
			catch (Exception ex)
			{
				return XOFFOperationResult.CreateFailure(ex);
			}
		}

		private ChangeQueueItem CreateQueueItem(string modelId, string modelJson , string changeType, string localId) 
		{

            XOFFLoggerSingleton.Instance.LogMessage($"Change Queue", $"Item Queued type: {typeof(TModel).FullName}, change type: {changeType} model id: {modelId}: localId {localId}");
            XOFFLoggerSingleton.Instance.LogMessage($"Change Queue", $"json {modelJson}");

			var idString = string.Empty;
			if (modelId != null)
			{
			    idString = modelId;
			}

			var queueItem = new ChangeQueueItem()
			{
				LocalId = Guid.NewGuid(),
				ChangedItemLocalId = localId,
				LastTimeSynced = DateTime.UtcNow,
				CreateDateTime = DateTime.UtcNow,
				ChangedItemId = idString,
				ChangeType = changeType,
				ChangedItemJson = modelJson,
				ChangedItemIdentifierTypeString = typeof(TIdentifier).FullName,
				ChangeItemTypeString = typeof(TModel).FullName
			};

			return queueItem;
		}
	}
}

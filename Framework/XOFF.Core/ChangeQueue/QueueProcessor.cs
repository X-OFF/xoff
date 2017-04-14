using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XOFF.Core.ChangeQueue;
using XOFF.Core.Repositories;

namespace XOFF.Core
{
	public interface IQueueProcessor
	{
		Task ProcessQueue();
	}

	public class QueueProcessor : IQueueProcessor
	{
		readonly IRepository<ChangeQueueItem, Guid> _chagneQueueRepository;
		readonly IRemoteHandlersServiceLocator _serviceLocator;
		readonly IRepositoryServiceLocator _repositoryServiceLocator;

		public QueueProcessor(IRepository<ChangeQueueItem, Guid> repository, IRemoteHandlersServiceLocator serviceLocator, IRepositoryServiceLocator repositoryServiceLocator)
		{
			_repositoryServiceLocator = repositoryServiceLocator;
			_chagneQueueRepository = repository;
			_serviceLocator = serviceLocator;
		}

		public async Task ProcessQueue()
		{
			var queueItemsQueryResult = _chagneQueueRepository.All();
            Debug.WriteLine($"----- Processing Queue -----");
            while (queueItemsQueryResult.Success && queueItemsQueryResult.Result.Any())
            {
                Debug.WriteLine($"-----{queueItemsQueryResult.Result.Count} Queue Items Found ------");
                foreach (var queueItem in queueItemsQueryResult.Result)
				{
					if (queueItem.ChangeType == ChangeTypeStrings.Created)
					{
						await ProcessCreate(queueItem);
					}
					if (queueItem.ChangeType == ChangeTypeStrings.Updated)
					{
						await ProcessUpdate(queueItem);
					}
					if (queueItem.ChangeType == ChangeTypeStrings.Deleted)
					{
						await ProcessDelete(queueItem);
					}
				}
				queueItemsQueryResult = _chagneQueueRepository.All();
			}
		}


		async Task ProcessDelete(ChangeQueueItem queueItem)
		{
			try
			{
				var deleteHandler = _serviceLocator.ResolveDeleteHandler(queueItem.ChangedItemType, queueItem.ChangedItemIdentifierType);
				var result = await deleteHandler.Delete(queueItem);

                UpdateQueueItem(queueItem, result.Success);

                if (result.Success)
                {
                    var respository = _repositoryServiceLocator.ResolveRepository(queueItem.ChangedItemType,
                        queueItem.ChangedItemIdentifierType);

                    var itemJson = JsonConvert.DeserializeObject(queueItem.ChangedItemId, queueItem.ChangedItemType);
                    respository.Delete(itemJson);

                }
            }
			catch (Exception ex)
			{
				//handle exceptions todo. 
			}
		}

	    private void UpdateQueueItem(ChangeQueueItem queueItem, bool remoteOperationSuccesful)
	    {
            //todo review how should failures be handled on updating / deleting these objects
	        if (remoteOperationSuccesful)
	        {
	            _chagneQueueRepository.Delete(queueItem.Id);
	        }
	        else
	        {
	            queueItem.FailedAttempts++;
	            _chagneQueueRepository.Upsert(queueItem);
	        }
	    }

	    async Task ProcessUpdate(ChangeQueueItem queueItem)
		{
			try
			{
				var updateHandler = _serviceLocator.ResolveUpdateHandler(queueItem.ChangedItemType, queueItem.ChangedItemIdentifierType);
				var result = await updateHandler.Update(queueItem);
                UpdateQueueItem(queueItem,result.Success);
			}
			catch (Exception ex)
			{
				//handle exceptions todo. 
			}
		}

		async Task ProcessCreate(ChangeQueueItem queueItem)
		{
			try
			{
				var createHandler = _serviceLocator.ResolveCreateHandler(queueItem.ChangedItemType, queueItem.ChangedItemIdentifierType);
				var result = await createHandler.Create(queueItem);
                UpdateQueueItem(queueItem, result.Success);

                //should these be to functions? and or have thier own try catches?
			    if (result.Success)
			    {
			        var respository = _repositoryServiceLocator.ResolveRepository(queueItem.ChangedItemType,
			            queueItem.ChangedItemIdentifierType);
			        var newItem = JsonConvert.DeserializeObject(result.Result, queueItem.ChangedItemType);
			        respository.Upsert(newItem);
			    }
			}
			catch (Exception ex)
			{
				//handle exceptions todo. 
			}
		}
	}
}

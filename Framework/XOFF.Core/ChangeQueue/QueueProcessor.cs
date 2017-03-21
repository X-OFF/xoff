using System;
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
		readonly IHttpHandlersServiceLocator _serviceLocator;
		readonly IRepositoryServiceLocator _repositoryServiceLocator;

		public QueueProcessor(IRepository<ChangeQueueItem, Guid> repository, IHttpHandlersServiceLocator serviceLocator, IRepositoryServiceLocator repositoryServiceLocator)
		{
			_repositoryServiceLocator = repositoryServiceLocator;
			_chagneQueueRepository = repository;
			_serviceLocator = serviceLocator;
		}

		public async Task ProcessQueue()
		{
			var queueItemsQueryResult = _chagneQueueRepository.All();
			while (queueItemsQueryResult.Success && queueItemsQueryResult.Result.Any())
			{
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
				await deleteHandler.DeleteById(queueItem.Id);
			}
			catch (Exception ex)
			{
				//handle exceptions todo. 
			}
		}

		async Task ProcessUpdate(ChangeQueueItem queueItem)
		{
			try
			{
				var updateHandler = _serviceLocator.ResolveUpdateHandler(queueItem.ChangedItemType, queueItem.ChangedItemIdentifierType);
				await updateHandler.Update(queueItem.Id);
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
				var newItem = JsonConvert.DeserializeObject(result.Result, queueItem.ChangedItemType);


				var respository = _repositoryServiceLocator.ResolveRepository(queueItem.ChangedItemType, queueItem.ChangedItemIdentifierType);

				respository.Upsert(newItem);
				_chagneQueueRepository.Delete(queueItem.Id);

			}
			catch (Exception ex)
			{
				//handle exceptions todo. 
			}
		}
	}
}

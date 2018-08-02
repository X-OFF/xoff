using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XOFF.Core.ChangeQueue;
using XOFF.Core.Logging;
using XOFF.Core.Repositories;

namespace XOFF.Core
{
	public interface IQueueProcessor //todo(Jackson) Disposable?
	{
		Task ProcessQueue();
		Dictionary<Type, Type> QueueItemTypes { get; set; }
	}

	public class QueueProcessor : IQueueProcessor
	{
		readonly IRepository<ChangeQueueItem, Guid> _changeQueueRepository;
		readonly IRemoteHandlersServiceLocator _serviceLocator;
		readonly IRepositoryServiceLocator _repositoryServiceLocator;

		readonly IConnectivityChecker _connectivityChecker;

		public QueueProcessor(IRepository<ChangeQueueItem, Guid> repository, IRemoteHandlersServiceLocator serviceLocator, IRepositoryServiceLocator repositoryServiceLocator, IConnectivityChecker connectivityChecker)
		{
			_connectivityChecker = connectivityChecker;
			_repositoryServiceLocator = repositoryServiceLocator;
			_changeQueueRepository = repository;
			_serviceLocator = serviceLocator;
			QueueItemTypes = new Dictionary<Type, Type>();
		}

		public Dictionary<Type, Type> QueueItemTypes { get; set; }//todo(XOFF) get these types by reflection?

		private XOFFOperationResult<IList<ChangeQueueItem>> GetQueueItems()
		{
			return _changeQueueRepository.All(x => x.FailedAttempts < 3 && !x.SuccessfullyProcessed, x => x.OrderBy(y => y.CreateDateTime));
		}


		public async Task ProcessQueue()
		{
            try
            {

                var queueItemsQueryResult = GetQueueItems();
                XOFFLoggerSingleton.Instance.LogMessage("Queue Processor", $"----- Processing Queue -----");
                while (queueItemsQueryResult.Success && queueItemsQueryResult.Result.Any())
                {

                    XOFFLoggerSingleton.Instance.LogMessage($"Queue Processor", $"-----{queueItemsQueryResult.Result.Count} Queue Items Found ------");
                    foreach (var queueItem in queueItemsQueryResult.Result)
                    {
                        if (!_connectivityChecker.Connected)
                        {

                            XOFFLoggerSingleton.Instance.LogMessage($"Queue Processor", $"----- Offline not processing item ------");
                            continue;
                        }
                        bool success = false;
                        if (queueItem.SuccessfullyProcessed)
                        {
                            continue;//skip bad data 
                        }

                        if (queueItem.ChangeType == ChangeTypeStrings.Deleted)
                        {
                            success = await ProcessDelete(queueItem);
                            string successOrFailure = success ? "Successfully" : "Unsuccessfully";

                            XOFFLoggerSingleton.Instance.LogMessage($"Queue Processor", $"----Queue Item Deleted {successOrFailure} -----");
                        }
                        else if (!string.IsNullOrWhiteSpace(queueItem.ChangedItemId))
                        {
                            success = await ProcessUpdate(queueItem);
                            string successOrFailure = success ? "Successfully" : "Unsuccessfully";
                            XOFFLoggerSingleton.Instance.LogMessage($"Queue Processor", $"----Queue Item Updated {successOrFailure} -----");
                        }
                        else if (string.IsNullOrWhiteSpace(queueItem.ChangedItemId))
                        {
                            success = await ProcessCreate(queueItem);
                            string successOrFailure = success ? "Successfully" : "Unsuccessfully";
                            XOFFLoggerSingleton.Instance.LogMessage($"Queue Processor", $"----Queue Item Created {successOrFailure} -----");
                        }
                    }

                    if (_connectivityChecker.Connected)
                    {
                        queueItemsQueryResult = GetQueueItems();
                    }
                    else
                    {
                        break;
                    }

                }
            }
            catch(Exception ex)
            {
                //queue should run in background and not throw exceptions to next level up
            }
		}

		KeyValuePair<Type, Type> GetQueueItemTypes(ChangeQueueItem queueItem)
		{
			var valuePair = QueueItemTypes.FirstOrDefault(x => x.Key.FullName == queueItem.ChangeItemTypeString);
			return valuePair;
		}

		async Task<bool> ProcessDelete(ChangeQueueItem queueItem)
		{
			try
			{
				KeyValuePair<Type, Type> typeKeyValuePair = GetQueueItemTypes(queueItem);

				var deleteHandler = _serviceLocator.ResolveDeleteHandler(typeKeyValuePair.Key, typeKeyValuePair.Value);
				var result = await deleteHandler.Delete(queueItem);

				UpdateQueueItem(queueItem, result.Success);

                //NOTE(Jackson) Not needed because synced repository deletes the local item before queueing. 
				//if (result.Success)
				//{ 
					//var respository = _repositoryServiceLocator.ResolveRepository(typeKeyValuePair.Key, typeKeyValuePair.Value);

      //              if (typeKeyValuePair.Value == typeof(Guid))
      //              {
      //                  var id = Guid.Empty;
      //                  var success = Guid.TryParse(queueItem.ChangedItemLocalId, out id);
      //                  if (success)
      //                  {
      //                      var deleteResult = respository.Delete(id);
      //                      return deleteResult.Success;
      //                  }
      //              }
      //              else
      //              {
      //                  var itemId = JsonConvert.DeserializeObject(queueItem.ChangedItemLocalId, typeKeyValuePair.Value);
						//var deleteResult = respository.Delete(itemId);
						//return deleteResult.Success;
                    //}
			//	}
				return result.Success;
			}
			catch (Exception ex)
			{
				//handle exceptions todo. 
			}
			return false;
		}


		async Task<bool> ProcessUpdate(ChangeQueueItem queueItem)
		{
			try
			{
				KeyValuePair<Type, Type> typeKeyValuePair = GetQueueItemTypes(queueItem);
				var updateHandler = _serviceLocator.ResolveUpdateHandler(typeKeyValuePair.Key, typeKeyValuePair.Value);
				var result = await updateHandler.Update(queueItem);
				UpdateQueueItem(queueItem, result.Success);
				return result.Success;
			}
			catch (Exception ex)
			{
				//handle exceptions todo. 
			}
			return false;
		}

		async Task<bool> ProcessCreate(ChangeQueueItem queueItem)
		{
			try
			{
				KeyValuePair<Type, Type> typeKeyValuePair = GetQueueItemTypes(queueItem);
				var createHandler = _serviceLocator.ResolveCreateHandler(typeKeyValuePair.Key, typeKeyValuePair.Value);
				var result = await createHandler.Create(queueItem);
				UpdateQueueItem(queueItem, result.Success);

				//should these be to functions? and or have thier own try catches?
				if (result.Success)
				{
					var respository = _repositoryServiceLocator.ResolveRepository(typeKeyValuePair.Key, typeKeyValuePair.Value);

					var callbackResult = respository.InsertCallBack(result.Result, queueItem.ChangedItemLocalId);

                    var changedItemLocalId = Guid.Empty;
                    Guid.TryParse(queueItem.ChangedItemLocalId,out changedItemLocalId);

					var otherQueueItemsForTheSameObject = _changeQueueRepository.All(x => x.ChangeItemTypeString == queueItem.ChangeItemTypeString
																					 && x.LocalId == changedItemLocalId);
					if (otherQueueItemsForTheSameObject.Success)
					{
						foreach (var changeQueueItem in otherQueueItemsForTheSameObject.Result)
						{
							changeQueueItem.ChangedItemId = callbackResult.Result;
							_changeQueueRepository.Upsert(changeQueueItem);
						}
					}
				}
				return result.Success;
			}
			catch (Exception ex)
			{
                XOFFLoggerSingleton.Instance.LogException(ex, XOFFErrorSeverity.Warning);
			}
			return false;
		}


		private void UpdateQueueItem(ChangeQueueItem queueItem, bool remoteOperationSuccesful)
		{
			//todo review how should failures be handled on updating / deleting these objects
			if (remoteOperationSuccesful)
			{
				queueItem.FailedAttempts = 0;
				queueItem.SuccessfullyProcessed = true;
				_changeQueueRepository.Delete(queueItem.LocalId);
			    _changeQueueRepository.DeleteAll(
			        x =>
			            x.ChangedItemId == queueItem.ChangedItemId && x.ChangeItemTypeString == queueItem.ChangeItemTypeString &&
			            x.CreateDateTime < queueItem.CreateDateTime);

			}
			else
			{
				queueItem.FailedAttempts++;
				_changeQueueRepository.Upsert(queueItem);
			}
		}
	}
}

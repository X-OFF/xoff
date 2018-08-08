using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XOFF.Core.ChangeQueue;
using XOFF.Core.Remote;
using XOFF.Core.Repositories.Settings;

namespace XOFF.Core.Repositories
{
    public class SyncedRepository<TModel, TIdentifier> : ISyncedRepository<TModel, TIdentifier> where TModel : class, IModel<TIdentifier>, new()
    {
        private readonly IRepository<TModel, TIdentifier> _repository;
        private readonly SyncRepositorySettings _settings;
        private readonly IRemoteEntityGetter<TModel, TIdentifier> _remoteEntityGetter;
        private readonly IConnectivityChecker _connectivityChecker;
		private readonly IChangeQueue<TModel, TIdentifier> _changeQueue;

		public SyncedRepository(IRepository<TModel, TIdentifier> repository, 
		                        SyncRepositorySettings settings,
		                        IRemoteEntityGetter<TModel,TIdentifier> remoteEntityGetter,
		                        IConnectivityChecker connectivityChecker,
		                        IChangeQueue<TModel, TIdentifier> changeQueue)
        {
			_repository = repository;
			_settings = settings;
			_remoteEntityGetter = remoteEntityGetter;
			_connectivityChecker = connectivityChecker;
			_changeQueue = changeQueue;
        }

        public async Task<XOFFOperationResult<IList<TModel>>> Get()
        {
            if (_settings.RefreshDataMode == RefreshDataMode.RefreshIfStale)
            {
                var itemsResult = _repository.All();
                if (itemsResult.Success && itemsResult.Result.Any())
                {
                    var oldestItem = itemsResult.Result.OrderBy(x => x.LastTimeSynced).First();
                    var secondsSinceLastSync = (DateTime.UtcNow - oldestItem.LastTimeSynced).TotalSeconds;
                    if (secondsSinceLastSync > _settings.RefreshSecondsThreshold)
                    {
                        await Refresh();
                    }
                    else
                    {
                        return itemsResult;
                    }
                }
                else
                {
					 await Refresh();
                }
            }
            else if (_settings.RefreshDataMode == RefreshDataMode.RefreshIfOnline)
            {
                await Refresh();
            }
            /*else if (_settings.RefreshSettings == RefreshData.OnlyOnRefresh)
            {	
            	// no logic needed yet
            }*/
            return _repository.All();
        }

        public async Task<XOFFOperationResult<TModel>> Get(TIdentifier id)
        {
            if (_settings.RefreshDataMode == RefreshDataMode.RefreshIfOnline)
            {
                await Refresh(id);
            }
            else if (_settings.RefreshDataMode == RefreshDataMode.RefreshIfStale)
            {
                var itemsResult = _repository.Get(id ,true,true);
                if (itemsResult.Success && itemsResult.Result != null)
                {  
                    if ((DateTime.UtcNow - itemsResult.Result.LastTimeSynced).Seconds > _settings.RefreshSecondsThreshold)
                    {
                        await Refresh(id);
                    }
                    else
                    {
                        return itemsResult;
                    }
                }
                else
                {
					// await Refresh(id); todo(JACKSON) reinclude this
                }
            }
            /*else if (_settings.RefreshSettings == RefreshData.OnlyOnRefresh)
             {
                 // no logic needed yet
             }*/
            return _repository.Get(id,true,true);
        }

		 
        public XOFFOperationResult Delete(TIdentifier id)
        {
			var repositoryResult = _repository.Delete(id);
            if (!repositoryResult.Success)
            {
				return repositoryResult.ToOperationResult();
            }
			var queueResult = _changeQueue.QueueDelete(repositoryResult.Result.LocalId, repositoryResult.Result.RemoteId);
            if (!queueResult.Success)
            {
                return queueResult;
            }
            return XOFFOperationResult.CreateSuccessResult("Success");
        }        

		public XOFFOperationResult<TModel> Update(TModel entity)
		{

			if (string.IsNullOrWhiteSpace(entity.RemoteId))
			{
				var existingResult = _repository.Get(entity.LocalId);
				if (existingResult.Success) 
				{
					entity.RemoteId = existingResult.Result.RemoteId;
				}
			}
			    
			var repositoryResult = _repository.Upsert(entity);
            if (!repositoryResult.Success)
            {
                return repositoryResult;
            }

           
            var queueResult = _changeQueue.QueueUpdate(entity);
            if (!queueResult.Success)
            {
                return XOFFOperationResult<TModel>.CreateFailure(queueResult.Exception);
            }

		    return repositoryResult;
		}

		public void Update(ICollection<TModel> items)
		{
			_repository.UpsertCollection(items);
			foreach (var item in items)
			{
				_changeQueue.QueueUpdate(item);
			}
		}

		public XOFFOperationResult<TModel> Insert(TModel entity, string queueJson = null, bool putOnQueue = true)
		{
			var repositoryResult = _repository.Upsert(entity);
            if (!repositoryResult.Success)
            {
                return repositoryResult;
            }
		    if (putOnQueue)
		    {
		        var queueResult = _changeQueue.QueueCreate(entity, queueJson);
		        if (!queueResult.Success)
		        {
		            return XOFFOperationResult<TModel>.CreateFailure(queueResult.Exception);
		        }
		    }
		    return repositoryResult;
		}

		public void Insert(ICollection<TModel> items)
		{
			_repository.UpsertCollection(items);
			foreach (var item in items)
			{
				_changeQueue.QueueCreate(item);
			}
		}

		public async Task Refresh()
		{
			if (_connectivityChecker.Connected)
			{
				var results = await _remoteEntityGetter.Get();
				if (results.Success)
				{	
					_repository.ReplaceAll(results.Result);
				}
			}
		}

		public async Task Refresh(TIdentifier id)
		{
			if (_connectivityChecker.Connected)
			{
				var results = await _remoteEntityGetter.Get(id);
				if (results.Success)
				{
					_repository.Upsert(results.Result);
				}
			}
		}
	}
}
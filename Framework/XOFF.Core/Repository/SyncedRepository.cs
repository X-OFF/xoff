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

        public async Task<OperationResult<IList<TModel>>> Get()
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

        public async Task<OperationResult<TModel>> Get(TIdentifier id)
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
                    await Refresh(id);
                }
            }
            /*else if (_settings.RefreshSettings == RefreshData.OnlyOnRefresh)
             {
                 // no logic needed yet
             }*/
            return _repository.Get(id,true,true);
        }

		 
        public void Delete(TIdentifier id)
        {
			_repository.Delete(id);
			_changeQueue.QueueDelete(id);
        }        

		public void Update(TModel entity)
		{
            _repository.Upsert(entity);
			_changeQueue.QueueUpdate(entity);
		}

		public void Update(ICollection<TModel> items)
		{
			_repository.Upsert(items);
			foreach (var item in items)
			{
				_changeQueue.QueueUpdate(item);
			}
		}

		public void Insert(TModel entity)
		{
			_repository.Upsert(entity);
			_changeQueue.QueueCreate(entity);
		}

		public void Insert(ICollection<TModel> items)
		{
			_repository.Upsert(items);
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
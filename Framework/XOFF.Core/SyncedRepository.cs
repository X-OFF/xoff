
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XOFF.Core.Remote;
using XOFF.Core.Settings;

namespace XOFF.Core.Repositories
{
    public class SyncedRepository<TModel, TIdentifier> : ISyncedRepository<TModel, TIdentifier> where TModel : class, IModel<TIdentifier>
    {
        private readonly IRepository<TModel, TIdentifier> _repository;
        private readonly SyncRepositorySettings _settings;
        private readonly IRemoteEntityGetter<TModel, TIdentifier> _remoteEntityGetter;
        private readonly IConnectivityChecker _connectivityChecker;

        public SyncedRepository(IRepository<TModel, TIdentifier> repository, SyncRepositorySettings settings, IRemoteEntityGetter<TModel,TIdentifier> remoteEntityGetter, IConnectivityChecker connectivityChecker)
        {
			_repository = repository;
			_settings = settings;
			_remoteEntityGetter = remoteEntityGetter;
			_connectivityChecker = connectivityChecker;
        }

        public OperationResult<IList<TModel>> Get()
        {
            if (_settings.RefreshDataMode == RefreshDataMode.IfStale)
            {
                var itemsResult = _repository.All();
                if (itemsResult.Success)
                {
                    var oldestItem = itemsResult.Result.OrderBy(x => x.LastTimeSynced).FirstOrDefault();
					if (oldestItem == null || (DateTime.UtcNow - oldestItem.LastTimeSynced).Seconds > _settings.RefreshSecondsThreshold)
                    {
                        Refresh();
                    }
                    else
                    {
                        return itemsResult;
                    }
                }
                else
                {
                    Refresh();
                }
            }
            else if (_settings.RefreshDataMode == RefreshDataMode.IfOnline)
            {
                Refresh();
            }
            /*else if (_settings.RefreshSettings == RefreshData.OnlyOnRefresh)
            {
                return _repository.All(); // no logic needed yet
            }*/
            return _repository.All();
        }

        public OperationResult<TModel> Get(TIdentifier id)
        {
            if (_settings.RefreshDataMode == RefreshDataMode.IfOnline)
            {
                Refresh();
            }
            else if (_settings.RefreshDataMode == RefreshDataMode.IfStale)
            {
                var itemsResult = _repository.Get(id ,true,true);
                if (itemsResult.Success && itemsResult.Result != null)
                {  
                    if ((DateTime.UtcNow - itemsResult.Result.LastTimeSynced).Seconds > _settings.RefreshSecondsThreshold)
                    {
                        Refresh();
                    }
                    else
                    {
                        return itemsResult;
                    }
                }
                else
                {
                    Refresh();
                }
            }
            /*else if (_settings.RefreshSettings == RefreshData.OnlyOnRefresh)
             {
                 return _repository.Get(id,true,true); // no logic needed yet
             }*/
            return _repository.Get(id,true,true);
        }

        public void Upsert(TModel entity)
        {
            //todo: queue up offline queue to send to server. 
            _repository.Upsert(entity);
        }

        public void Upsert(ICollection<TModel> items)
        { 
            //todo queue up offline queue to send to server for sync
            _repository.Upsert(items);
        }

        public void Delete(TIdentifier id)
        {
            //todo queue up offline queue to send to server for sync
            _repository.Delete(id);
        }

        public async Task Refresh()
        {
            if (_connectivityChecker.Connected)
            {
                var results = await _remoteEntityGetter.Get();
                if (results.Success)
                {
                    _repository.DeleteAll(null, recursive: true);
                    _repository.Upsert(results.Result);
                }
            }
        }
    }
}
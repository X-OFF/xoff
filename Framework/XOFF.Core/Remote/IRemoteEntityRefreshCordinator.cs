using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XOFF.Core.Remote
{
    public interface IRemoteEntityRefreshCoordinator
    {
        void RegisterTypeToRefresh(Type modelType, Type identifierType);
        void Start();
        void Stop();
    }

    public class XOFFTimerRemoteEntityRefreshCoordinator : IRemoteEntityRefreshCoordinator
    {
        private readonly IRemoteHandlersServiceLocator _serviceLocator;
        List<KeyValuePair<Type,Type>> _typesToRefresh;
        private Task _task;
        private CancellationTokenSource _tokenSource;

        public XOFFTimerRemoteEntityRefreshCoordinator(IRemoteHandlersServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _typesToRefresh = new List<KeyValuePair<Type, Type>>();
        }

        public void RegisterTypeToRefresh(Type modelType, Type identifierType) 
        {
           _typesToRefresh.Add(new KeyValuePair<Type, Type>(modelType,identifierType));
        }

        public void Start()
        {
             _tokenSource = new CancellationTokenSource();
            _task = Task.Factory.StartNew(RefreshEntities, _tokenSource.Token);
        }

        public void Stop()
        {
            _tokenSource.Cancel();
            _task.Dispose();
        }

        private async void RefreshEntities()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                foreach (var keyValuePair in _typesToRefresh)
                {
                    var handler = _serviceLocator.ResolveGetHandler(keyValuePair.Key, keyValuePair.Value);
                    var result = await handler.GetAll();
                    //Do I care about thre result?

                    //todo update the UI things have been updated

                }
                await Task.Delay(10000);
            }
        }
    }
}
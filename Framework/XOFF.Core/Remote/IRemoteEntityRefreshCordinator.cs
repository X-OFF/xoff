using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XOFF.Core.Logging;

namespace XOFF.Core.Remote
{
    public interface IRemoteEntityRefreshCoordinator
    {
        void RegisterTypeToRefresh(Type modelType, Type identifierType);
        void Start();
        void Stop();
		void ClearRegisteredTypes();
		Task<bool> RefreshEntities();
		Task<bool> RefreshEntities(List<KeyValuePair<Type, Type>> types);
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
            _task = Task.Factory.StartNew(RefreshLoop, _tokenSource.Token);
        }

        public void Stop()
        {
            _tokenSource.Cancel();
            _task.Dispose();
        }

        private async void RefreshLoop()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
				try
				{
					//LoggerSingleton.Instance.LogMessage("Entity Refresh Loop", "Wait");
                    await Task.Delay(300000);
			        //LoggerSingleton.Instance.LogMessage("Entity Refresh Loop", "Done Waiting, Refresh ");
					await RefreshEntities();
					//LoggerSingleton.Instance.LogMessage("Entity Refresh Loop", "Done Refreshing ");

				}
				catch (Exception ex) 
				{
					//make sure background process doesn't crash the app
				}
            }
        }

		public async Task<bool> RefreshEntities()
		{			
			return await RefreshEntities(_typesToRefresh.ToList());			
		}

		public async Task<bool> RefreshEntities(List<KeyValuePair<Type, Type>> types)
		{
			try
			{
	            XOFFLoggerSingleton.Instance.LogMessage("Refresh Coordinator", "RefreshEntities");
				var allSucessful = true;
				foreach (var keyValuePair in types)
				{
					var handler = _serviceLocator.ResolveGetHandler(keyValuePair.Key, keyValuePair.Value);
					var result = await handler.GetAll();
					//todo alert the UI things have been updated
					allSucessful = allSucessful && result.Success;
                    XOFFLoggerSingleton.Instance.LogMessage("Refresh Coordinator", $"Refresh allsuccess: {allSucessful}");
				}
			    return allSucessful;
			}
			catch (Exception ex)
			{
				//there was an error inform the calling function but eat the exception
                XOFFLoggerSingleton.Instance.LogException("Refresh Coordinator Refresh Entities",ex, XOFFErrorSeverity.Warning);
			}
			return false;
		}

		public void ClearRegisteredTypes()
		{
			_typesToRefresh?.Clear();
		}
	}
}
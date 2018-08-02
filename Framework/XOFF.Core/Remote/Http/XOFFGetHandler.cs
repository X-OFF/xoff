using System;
using System.Linq;
using System.Threading.Tasks;
using XOFF.Core.Repositories;

namespace XOFF.Core.Remote.Http
{
    public class XOFFGetHandler<TModel, TIdentifier> : IRemoteEntityGetHandler<TModel, TIdentifier> where TModel : class, IModel<TIdentifier>, new()
    {
        private readonly IRepository<TModel, TIdentifier> _repository;
        private readonly IRemoteEntityGetter<TModel, TIdentifier> _getter;

        public XOFFGetHandler(IRepository<TModel,TIdentifier> repository, IRemoteEntityGetter<TModel,TIdentifier> getter)
        {
            _repository = repository;
            _getter = getter;
        }

        public virtual async Task<XOFFOperationResult> GetAll()
        {
            try
            {
                var getResult = await _getter.Get();
                if (!getResult.Success)
                {
					return XOFFOperationResult.CreateFailure(getResult.Message);
                }
                if (getResult.Result != null)
                {
					var upsertResult = _repository.ReplaceAll(getResult.Result);
                    if (!upsertResult.Success)
                    {
                        return XOFFOperationResult.CreateFailure(upsertResult.Exception);
                    }
                }
                return XOFFOperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return XOFFOperationResult.CreateFailure(ex);
            }
        }



        public virtual async Task<XOFFOperationResult> GetById<T>(T id)
        {
            if (typeof(T) != typeof(TIdentifier))
            {
                throw new ArgumentException($"Identifier passed is not of type {typeof(T)}");
            }
            try
            {
                var typedId = (TIdentifier)(object)id;//Todo have someone review this
                var getResult = await _getter.Get(typedId);
                if (!getResult.Success)
                {
                    return XOFFOperationResult.CreateFailure(getResult.Exception);
                }
                var upsertResult = _repository.Upsert(getResult.Result);
                if (!upsertResult.Success)
                {
                    return XOFFOperationResult.CreateFailure(upsertResult.Exception);
                }
                return XOFFOperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return XOFFOperationResult.CreateFailure(ex);
            }


        }
    }
}
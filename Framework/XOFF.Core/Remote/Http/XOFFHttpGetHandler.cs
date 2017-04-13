using System;
using System.Linq;
using System.Threading.Tasks;
using XOFF.Core.Repositories;

namespace XOFF.Core.Remote.Http
{
    public class XOFFHttpGetHandler<TModel, TIdentifier> : IRemoteEntityGetHandler<TModel, TIdentifier> where TModel : class, IModel<TIdentifier>
    {
        private readonly IRepository<TModel, TIdentifier> _repository;
        private readonly IRemoteEntityGetter<TModel, TIdentifier> _getter;

        public XOFFHttpGetHandler(IRepository<TModel,TIdentifier> repository, IRemoteEntityGetter<TModel,TIdentifier> getter)
        {
            _repository = repository;
            _getter = getter;
        }

        public async Task<OperationResult> GetAll()
        {
            try
            {
                var getResult = await _getter.Get();
                if (!getResult.Success)
                {
                    return OperationResult.CreateFailure(getResult.Exception);
                }
                if (getResult.Result != null && getResult.Result.Any())
                {
                    var upsertResult = _repository.Upsert(getResult.Result);
                    if (!upsertResult.Success)
                    {
                        return OperationResult.CreateFailure(upsertResult.Exception);
                    }
                }
                return OperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(ex);
            }
        }



        public async Task<OperationResult> GetById<T>(T id)
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
                    return OperationResult.CreateFailure(getResult.Exception);
                }
                var upsertResult = _repository.Upsert(getResult.Result);
                if (!upsertResult.Success)
                {
                    return OperationResult.CreateFailure(upsertResult.Exception);
                }
                return OperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(ex);
            }


        }
    }
}
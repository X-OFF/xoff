using System.Threading.Tasks;

namespace XOFF.Core.Remote
{
    public interface IRemoteEntityGetHandler<TModel, TIdentifer> : IRemoteGetHandler where TModel : IModel<TIdentifer>
    {
    }

    public interface IRemoteGetHandler
    {
        Task<OperationResult> GetAll();
        Task<OperationResult> GetById<T>(T id);
    }

   
}
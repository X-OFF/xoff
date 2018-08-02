using System.Threading.Tasks;

namespace XOFF.Core.Remote
{
    public interface IRemoteEntityGetHandler<TModel, TIdentifer> : IRemoteGetHandler where TModel : IModel<TIdentifer>
    {
    }

    public interface IRemoteGetHandler
    {
        Task<XOFFOperationResult> GetAll();
        Task<XOFFOperationResult> GetById<T>(T id);
    }

   
}
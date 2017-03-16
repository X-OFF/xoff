using System.Collections.Generic;
using System.Threading.Tasks;

namespace XOFF.Core.Repositories
{
    public interface ISyncedRepository<TModel, TIdentifier> where TModel: class, IModel<TIdentifier>
    {
        OperationResult<IList<TModel>> Get();
        OperationResult<TModel> Get(TIdentifier id);
        void Upsert(TModel entity);
        void Upsert(ICollection<TModel> items);
        void Delete(TIdentifier id);
        /// <summary>
        /// Gets data from the "server" and completely deletes and restores the local data
        /// </summary>
        Task Refresh();
    }
}
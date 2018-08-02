using System.Collections.Generic;
using System.Threading.Tasks;

namespace XOFF.Core.Repositories
{
    public interface ISyncedRepository<TModel, TIdentifier> where TModel: class, IModel<TIdentifier>
    {
        Task<XOFFOperationResult<IList<TModel>>> Get();
        Task<XOFFOperationResult<TModel>> Get(TIdentifier id);
        XOFFOperationResult<TModel> Update(TModel entity);
        void Update(ICollection<TModel> items);

		XOFFOperationResult<TModel> Insert(TModel entity, string queueJson = null, bool putOnQueue = true);
		void Insert(ICollection<TModel> items);


        XOFFOperationResult Delete(TIdentifier id);
        /// <summary>
        /// Gets data from the "server" and completely deletes and restores the local data
        /// </summary>
        Task Refresh();
		Task Refresh(TIdentifier id);
    }
}
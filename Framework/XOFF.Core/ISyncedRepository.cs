using System.Collections.Generic;
using System.Threading.Tasks;

namespace XOFF.Core.Repositories
{
    public interface ISyncedRepository<TModel, TIdentifier> where TModel: class, IModel<TIdentifier>
    {
        Task<OperationResult<IList<TModel>>> Get();
        Task<OperationResult<TModel>> Get(TIdentifier id);
        void Update(TModel entity);
        void Update(ICollection<TModel> items);

		void Insert(TModel entity);
		void Insert(ICollection<TModel> items);


        void Delete(TIdentifier id);
        /// <summary>
        /// Gets data from the "server" and completely deletes and restores the local data
        /// </summary>
        Task Refresh();
		Task Refresh(TIdentifier id);
    }
}
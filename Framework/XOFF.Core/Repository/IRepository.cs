using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XOFF.Core;

namespace XOFF.Core.Repositories
{
	public interface IObjectRepository
	{
		XOFFOperationResult UpsertObject(object item);//Need a non generic way to insert objects for dependency resolution would be good to find a better way to do this. see the usage of this method
		XOFFOperationResult Delete<T>(T id);//Need a non generic way to insert objects for dependency resolution would be good to find a better way to do this. see the usage of this method
        /// <summary>
        /// preforms any updates needed after a remote creation
        /// </summary>
        /// <param name="resultResult"></param>
        /// <param name="localId"></param>
        /// <returns>the remote id of the newly created object</returns>
        XOFFOperationResult<string> InsertCallBack(string resultResult, string localId);
		XOFFOperationResult<int> Count();
	}

	public interface IRepository<TModel, TIdentifier> : IObjectRepository where TModel : class, IModel<TIdentifier>, new()
	{

		void Initialize();
		XOFFOperationResult<IList<TModel>> All(Expression<Func<TModel, bool>> filter = null, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null, bool recursive = false);//
		XOFFOperationResult<IList<TModel>> Get(List<TIdentifier> ids, bool withChildren = false, bool recursive = false);
		XOFFOperationResult<TModel> Get(TIdentifier id, bool withChildren = false, bool recursive = false);


		XOFFOperationResult DeleteAll(Expression<Func<TModel, bool>> filter = null, bool recursive = false);
		XOFFOperationResult DeleteAllInTransaction(ICollection<TModel> items, bool recursive = false);
		XOFFOperationResult<TModel> Delete(TIdentifier id);


		XOFFOperationResult<TModel> Upsert(TModel entity);
		XOFFOperationResult UpsertCollection(ICollection<TModel> items);
		XOFFOperationResult ReplaceAll(ICollection<TModel> items);
	}
}
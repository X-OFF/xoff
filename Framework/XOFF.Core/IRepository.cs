using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XOFF.Core;

namespace XOFF.Core.Repositories
{
    public interface IRepository<TModel, TIdentifier> where TModel : class, IModel<TIdentifier>
    {
        int ExpirationMinutes { get; }
        void Initialize();
        OperationResult<IList<TModel>> All(Expression<Func<TModel, bool>> filter = null, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null, bool recursive = false);//
        OperationResult<IList<TModel>> Get(List<TIdentifier> ids, bool withChildren = false, bool recursive = false);
        OperationResult<TModel> Get(TIdentifier id, bool withChildren = false, bool recursive = false);


        OperationResult DeleteAll(Expression<Func<TModel, bool>> filter = null, bool recursive = false);
        OperationResult DeleteAllInTransaction(ICollection<TModel> items, bool recursive = false);
        OperationResult Delete(TIdentifier id);


        OperationResult Upsert(TModel entity);
        OperationResult Upsert(ICollection<TModel> items);


        void OnReplaceComplete(ICollection<TModel> items);
    }
}
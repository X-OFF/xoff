using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LiteDB;
using Newtonsoft.Json.Bson;
using XOFF.Core;
using XOFF.Core.Repositories;

namespace XOFF.LiteDB
{
	public interface ILiteDBRepository<TModel, TIdentifier> : IRepository<TModel,TIdentifier> where TModel : class, IModel<TIdentifier>
	{
	
	}


	public class LiteDBRepository<TModel, TIdentifier> : ILiteDBRepository<TModel,TIdentifier> where TModel : class, IModel<TIdentifier> where TIdentifier : BsonValue
	{
		protected LiteDatabase Connection;
		readonly ILiteDbConnectionProvider _connectionProvider;
        LiteCollection<TModel> _collection => Connection.GetCollection<TModel>(typeof(TModel).Name);

	    public LiteDBRepository(ILiteDbConnectionProvider connectionProvider)
		{
			_connectionProvider = connectionProvider;
			Connection = _connectionProvider.Database;
		}

		public OperationResult<IList<TModel>> All(Expression<Func<TModel, bool>> filter = null, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null, bool recursive = false)
		{
		    try
		    {
		        IEnumerable<TModel> items = new List<TModel>();
		        if (filter != null)
		        {
		            items = Connection.GetCollection<TModel>().Find(filter);
		        }

		        //recursive is by default because this stuff is being stored as json documents 

		        if (orderBy != null)
		        {
		            items = orderBy(items.AsQueryable()).ToList();
		        }
		        return OperationResult<IList<TModel>>.CreateSuccessResult(items.ToList());
		    }
		    catch (Exception ex)
		    {
		        return OperationResult<IList<TModel>>.CreateFailure(ex);
		    }
		}

		public OperationResult Delete(TIdentifier id)
		{
		    try
		    {
		        Connection.GetCollection<TModel>().Delete(x => x.Id.Equals(id));
		        return OperationResult.CreateSuccessResult("Success");
		    }
		    catch (Exception ex)
		    {
		        return OperationResult.CreateFailure(ex);
		    }
		}

		public OperationResult DeleteAll(Expression<Func<TModel, bool>> filter = null, bool recursive = false)
		{

            try
            {
                if (filter == null)
                {
                    Connection.DropCollection(typeof(TModel).Name);
                }
                else
                {
                    _collection.Delete(filter);
                }
                return OperationResult.CreateSuccessResult("Success");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(ex);
            }
          
		}

		public OperationResult DeleteAllInTransaction(ICollection<TModel> items, bool recursive = false)
		{

		    try
		    {
		        
		        using (var transaction = Connection.BeginTrans())
		        {
		            try
		            {
		                foreach (var model in items)
		                {
		                    _collection.Delete(x => x.Id.Equals(model.Id));
		                }
		                transaction.Commit();
		            }
		            catch (Exception)
		            {
		                transaction.Rollback();
		                throw;
		            }
		        }
                return OperationResult.CreateSuccessResult("Success");
		    }
		    catch (Exception ex)
		    {
		        return OperationResult.CreateFailure(ex);
		    }
		}

		public OperationResult<TModel> Get(TIdentifier id, bool withChildren = false, bool recursive = false)
		{
		    try
		    {
		        var item = _collection.FindById(id);
                return OperationResult<TModel>.CreateSuccessResult(item);
		    }
		    catch (Exception ex)
		    {
		        return OperationResult<TModel>.CreateFailure(ex);
		        throw;
		    }
		}

		public OperationResult<IList<TModel>> Get(List<TIdentifier> ids, bool withChildren = false, bool recursive = false)
		{
            try
            {
                var items = _collection.Find(x=>ids.Contains(x.Id)).ToList();
                return OperationResult<IList<TModel>>.CreateSuccessResult(items);
            }
            catch (Exception ex)
            {
                return OperationResult<IList<TModel>>.CreateFailure(ex);
                throw;
            }
        }

		public void Initialize()
		{
			//nothing needed here yet
		}

		public OperationResult Upsert(ICollection<TModel> items)
        {
            try
            {
                foreach (var model in items)
                {
                    Upsert(model);
                }
                return OperationResult.CreateSuccessResult("Success");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(ex);
            }
		}

		public OperationResult Upsert(TModel entity)
		{
		    try
		    {
		        var exists = _collection.Exists(x => x.Id.Equals(entity.Id));
		        if (exists)
		        {
		            _collection.Update(entity);
		        }
		        else
		        {
		            _collection.Insert(entity);
		        }
                return OperationResult.CreateSuccessResult("Success");
            }
		    catch (Exception ex)
		    {
                return OperationResult.CreateFailure(ex);
            }
		}
	}
}

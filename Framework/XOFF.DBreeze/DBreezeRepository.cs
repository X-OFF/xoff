using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using XOFF.Core;
using XOFF.Core.Repositories;
using DBreeze.Utils;
using DBreeze;
using System.IO;
using DBreeze.Objects;
using Newtonsoft.Json;

namespace XOFF.DBreeze
{
    
	public class DBreezeRepository<TModel, TIdentifier> : IDBreezeRepository<TModel, TIdentifier> where TModel : class, IModel<TIdentifier>, new()
	{
	    private DBreezeEngine Engine
	    {
	        get
	        {
	            _provider.WaitOne();//you must release this after using the engine
	            return _provider.Engine;
	        }
	    }

	    readonly string _tableName;
	    private readonly IDBreezeConnectionProvider _provider;

	    public DBreezeRepository(IDBreezeConnectionProvider provider, string tableName = null)
	    {
	        _provider = provider;
	       _tableName = tableName ?? typeof(TModel).FullName;
        } 

	    public XOFFOperationResult<IList<TModel>> All(Expression<Func<TModel, bool>> filter = null, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null, bool recursive = false)
		{
			try
			{
				//todo this likely could be more efficient
			    using (var engine = Engine)
			    {
			        using (var transaction = engine.GetTransaction())
			        {
                       
                        var itemsStrs = transaction.SelectForward<string, string>(_tableName).Select(x => x.Value).ToList();
			            var items = itemsStrs.Select(JsonConvert.DeserializeObject<TModel>);

			            if (filter != null)
			            {
			                items = items.AsQueryable().Where(filter);
			            }

			            if (orderBy != null)
			            {
			                items = orderBy(items.AsQueryable()).ToList();
			            }

			            return XOFFOperationResult<IList<TModel>>.CreateSuccessResult(items.ToList());
			        }
			    }
			}
			catch (Exception ex)
			{
				return XOFFOperationResult<IList<TModel>>.CreateFailure(ex);
			}
            finally
		    {
		        _provider.Release();
		    }
		}

		public XOFFOperationResult Delete(TIdentifier id)
		{
            try
            {
              
                using (var engine = Engine)
                {
                    using (var transaction = engine.GetTransaction())
                    {
                        transaction.RemoveKey(_tableName, id.ToString());
                        transaction.Commit();
                        return XOFFOperationResult.CreateSuccessResult();
                    }
                }
            }
            catch (Exception ex)
            {
                return XOFFOperationResult.CreateFailure(ex);
            }
            finally
            {
                _provider.Release();
            }
        }

        public XOFFOperationResult DeleteAll(Expression<Func<TModel, bool>> filter = null, bool recursive = false)
        {
            if (filter == null)
            {
                return DeleteAllWithoutFilter();
            }
            else
            {
                return DeleteAllWithFilter(filter, recursive);
            }
        }

        private XOFFOperationResult DeleteAllWithFilter(Expression<Func<TModel, bool>> filter, bool recursive)
	    {
	        var itemResult = All(filter);
	        if (!itemResult.Success)
	        {
	            return XOFFOperationResult.CreateFailure(itemResult.Exception);
	        }

	        var ids = itemResult.Result.Select(x => x.Id.ToString());
            try
            {
                using (var engine = Engine)
                {
                    using (var transaction = engine.GetTransaction())
                    {
                        foreach (var identifier in ids)
                        {
                            transaction.RemoveKey(_tableName, identifier);
                        }
                        transaction.Commit();
                    }
                }
                return XOFFOperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return XOFFOperationResult.CreateFailure(ex);
            }
            finally
            {
                _provider.Release();
            }
        }

	    private XOFFOperationResult DeleteAllWithoutFilter()
	    {
	        try
	        {
	            using (var engine = Engine)
	            {
	                using (var transaction = engine.GetTransaction())
	                {
	                    {
	                        transaction.RemoveAllKeys(_tableName, false);
                            transaction.Commit();
	                    }
	                }
                    return XOFFOperationResult.CreateSuccessResult();
                }
	        }
            catch (Exception ex)
            {
                return XOFFOperationResult.CreateFailure(ex);
            }
            finally
            {
                _provider.Release();
            }
        }

	    public XOFFOperationResult DeleteAllInTransaction(ICollection<TModel> items, bool recursive = false)
		{
            try
            {
                using (var engine = Engine)
                {
                    using (var transaction = engine.GetTransaction())
                    {
                        try
                        {
                            foreach (var item in items)
                            {
                                transaction.RemoveKey(_tableName, item.id);
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                return XOFFOperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {

                return XOFFOperationResult.CreateFailure(ex);
            }
            finally
            {
                _provider.Release();
            }
        }

		public XOFFOperationResult<TModel> Get(TIdentifier id, bool withChildren = false, bool recursive = false)
		{
			try
			{
                
			    using (var engine = Engine)
			    {
			        using (var transaction = engine.GetTransaction())
			        {
			            var row = transaction.Select<TIdentifier, TModel>(_tableName, id);
			            return XOFFOperationResult<TModel>.CreateSuccessResult(row.Value);
			        }
			    }
			}
			catch (Exception ex)
			{
				return XOFFOperationResult<TModel>.CreateFailure(ex);
			}
            finally
            {
                _provider.Release();
            }
        }

	    public XOFFOperationResult<IList<TModel>> Get(List<TIdentifier> ids, bool withChildren = false, bool recursive = false)
	    {
	        try
	        {
	          
	            using (var engine = Engine)
	            {
	                using (var transaction = engine.GetTransaction())
	                {
	                    List<TModel> items = new List<TModel>();
	                    foreach (var id in ids)
	                    {
	                        var row = transaction.Select<TIdentifier, string>(_tableName, id);
	                        items.Add(JsonConvert.DeserializeObject<TModel>(row.Value));
	                    }

	                    return XOFFOperationResult<IList<TModel>>.CreateSuccessResult(items.ToList());
	                }
	            }
	        }
	        catch (Exception ex)
	        {
	            return XOFFOperationResult<IList<TModel>>.CreateFailure(ex);
	        }
	        finally
	        {
	            _provider.Release();
	        }
	    }

	    public void Initialize()
		{
			
		}

		public XOFFOperationResult Upsert(object item)
		{
			if (!(item is TModel))
			{
				throw new InvalidOperationException($"Item is not of type {typeof(TModel).FullName}");
			}
			var model = (TModel)item;
			return Upsert(model);
		}

		public XOFFOperationResult Upsert(ICollection<TModel> items)
		{
			try
			{
				foreach (var model in items)
				{
					model.LastTimeSynced = DateTime.UtcNow;
					Upsert(model);
				}
				return XOFFOperationResult.CreateSuccessResult("Success");
			}
			catch (Exception ex)
			{
				return XOFFOperationResult.CreateFailure(ex);
			}
		}

		public XOFFOperationResult Upsert(TModel entity)
		{
			try
			{
				entity.LastTimeSynced = DateTime.UtcNow;
			    using (var engine = Engine)
			    {
			        using (var transaction = engine.GetTransaction())
			        {

			            //var exists = transaction.Select<string, string>(_tableName, entity.Id.ToString()) != null;
                        transaction.Insert(_tableName,entity.Id.ToString(), JsonConvert.SerializeObject(entity));//this method is really an upsert, one of the out parameters is "was object updated" and there is an option to only insert and fail if exists 
                        transaction.Commit();
			        }
                }
                return XOFFOperationResult.CreateSuccessResult("Success");
            }
			catch (Exception ex)
			{
				return XOFFOperationResult.CreateFailure(ex);
			}
            finally
            {
                _provider.Release();
            }
        }

	    public XOFFOperationResult Delete<T>(T id)
	    {
	        if (typeof(T) != typeof(TIdentifier))
	        {
	            throw new ArgumentException($"Id is not of type {typeof(TIdentifier)}");
	        }
	        return Delete((TIdentifier)(object) id);
	    }

	    public XOFFOperationResult ReplaceAll(ICollection<TModel> items)
	    {
	        var deleteResult = DeleteAll();
	        if (!deleteResult.Success)
	        {
	            return deleteResult;
	        }
	        var upsertResult = Upsert(items);
	        return upsertResult;
	    }
	}
}

/*
 *Try this if you ever try to use objects again...
 * if (exists)
                     {
                         transaction.RemoveKey(_tableName, entity.Id);
                     }*/

/*  var wrapper = new DBreezeObject<string>
  {
      Entity = JsonConvert.SerializeObject(entity),
      NewEntity = !exists,
      Indexes = new List<DBreezeIndex>
      {
      //to Get customer by ID
          new DBreezeIndex(1,entity.Id.ToString()) { PrimaryIndex = true }
      }

   // transaction.ObjectInsert(_tableName, wrapper);
  };*/

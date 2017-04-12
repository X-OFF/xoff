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
    
	public class DBreezeRepository<TModel, TIdentifier> : IDBreezeRepository<TModel, TIdentifier> where TModel : class, IModel<TIdentifier>
	{
	    private DBreezeEngine Engine
	    {
	        get
	        {
	            _provider.WaitOne();
	            return _provider.Engine;
               
	        }
	    }

	    readonly string _tableName;
	    private readonly IDBreezeConnectionProvider _provider;

	    public DBreezeRepository(IDBreezeConnectionProvider provider, string tableName = null)
	    {
	        _provider = provider;
	        CustomSerializator.ByteArraySerializator = (object o) => {
	            try
	            {
	                var str = JsonConvert.SerializeObject(o);
	                return System.Text.Encoding.UTF8.GetBytes(str);
	            }
	            catch (Exception ex)
	            {
	                throw;
	            }
	        };
	        CustomSerializator.ByteArrayDeSerializator = (byte[] bt, Type t)
	            =>
	        {
	            try
	            {
	                var str = System.Text.Encoding.UTF8.GetString(bt);
	                return JsonConvert.DeserializeObject(str, t);
	            }
	            catch (Exception ex)
	            {
	                throw;
	            }

	        };
             _tableName = tableName ?? typeof(TModel).FullName;
        } 

	    public OperationResult<IList<TModel>> All(Expression<Func<TModel, bool>> filter = null, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null, bool recursive = false)
		{

			try
			{
				//todo this likely could be more efficient
			    using (var engine = Engine)
			    {
			        using (var transaction = engine.GetTransaction())
			        {
                       
                        var itemsStrs = transaction.SelectForward<string, string>(_tableName)
			                .Select(x => x.Value).ToList();
			            var items = itemsStrs.Select(x => JsonConvert.DeserializeObject<TModel>(x));
			            if (filter != null)
			            {
			                items = items.AsQueryable().Where(filter);
			            }

			            if (orderBy != null)
			            {
			                items = orderBy(items.AsQueryable()).ToList();
			            }

			            return OperationResult<IList<TModel>>.CreateSuccessResult(items.ToList());
			        }
			    }
			}
			catch (Exception ex)
			{
				return OperationResult<IList<TModel>>.CreateFailure(ex);
			}
            finally
		    {
		        _provider.Release();
		    }
		}

		public OperationResult Delete(TIdentifier id)
		{
			throw new NotImplementedException();
		}

		public OperationResult DeleteAll(Expression<Func<TModel, bool>> filter = null, bool recursive = false)
		{
			throw new NotImplementedException();
		}

		public OperationResult DeleteAllInTransaction(ICollection<TModel> items, bool recursive = false)
		{
			throw new NotImplementedException();
		}

		public OperationResult<TModel> Get(TIdentifier id, bool withChildren = false, bool recursive = false)
		{
			try
			{
                //todo this likely could be more efficient
			    using (var engine = Engine)
			    {
			        using (var transaction = engine.GetTransaction())
			        {
			            var row = transaction.Select<TIdentifier, TModel>(_tableName, id);
			            return OperationResult<TModel>.CreateSuccessResult(row.Value);
			        }
			    }
			}
			catch (Exception ex)
			{
				return OperationResult<TModel>.CreateFailure(ex);
			}
            finally
            {
                _provider.Release();
            }
        }

		public OperationResult<IList<TModel>> Get(List<TIdentifier> ids, bool withChildren = false, bool recursive = false)
		{
		    try
		    {
		        //todo this likely could be more efficient
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

                        return OperationResult<IList<TModel>>.CreateSuccessResult(items.ToList());
                    }
		    }
		}
			catch (Exception ex)
			{
				return OperationResult<IList<TModel>>.CreateFailure(ex);
			}
            finally
            {
                _provider.Release();
            }
        }

		public void Initialize()
		{
			throw new NotImplementedException();
		}

		public OperationResult Upsert(object item)
		{
			if (!(item is TModel))
			{
				throw new InvalidOperationException($"Item is not of type {typeof(TModel).FullName}");
			}
			var model = (TModel)item;
			return Upsert(model);
		}

		public OperationResult Upsert(ICollection<TModel> items)
		{
			try
			{
				foreach (var model in items)
				{
					model.LastTimeSynced = DateTime.UtcNow;
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
				entity.LastTimeSynced = DateTime.UtcNow;
			    using (var engine = Engine)
			    {
			        using (var transaction = engine.GetTransaction())
			        {

			            var exists = transaction.Select<string, string>(_tableName, entity.Id.ToString()) != null;
			           /* if (exists)
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

                        };*/
                        transaction.Insert(_tableName,entity.Id.ToString(), JsonConvert.SerializeObject(entity));
			           // transaction.ObjectInsert(_tableName, wrapper);

                        transaction.Commit();
			        }
			        return OperationResult.CreateSuccessResult("Success");
                }
			}
			catch (Exception ex)
			{
				return OperationResult.CreateFailure(ex);
			}
            finally
            {
                _provider.Release();
            }
        }
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using LiteDB;
using Newtonsoft.Json;
using XOFF.Core;
using XOFF.Core.Logging;
using XOFF.Core.Repositories;

namespace XOFF.LiteDB
{
    public interface ILiteDBRepository<TModel, TIdentifier> : IRepository<TModel, TIdentifier> where TModel : class, IModel<TIdentifier>, new()

    {

    }

    public class LiteDBRepository<TModel, TIdentifier> : ILiteDBRepository<TModel, TIdentifier>
        where TModel : class, IModel<TIdentifier>, new()

    {
        protected LiteDatabase Connection
        {
            get
            {
                _connectionProvider.WaitOne();
                return _connectionProvider.Database;
            }
        }

        protected readonly ILiteDbConnectionProvider _connectionProvider;

        public LiteDBRepository(ILiteDbConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;

        }

		public static string GetClassTableName(Type type = null)
		{
		    if (type == null)
		    {
		        type = typeof(TModel);
		    }

			var fullName = type.FullName;
			Regex rgx = new Regex("[^a-zA-Z0-9]");
			fullName =  rgx.Replace(fullName, "");
			fullName = fullName.Substring(Math.Max(0, fullName.Length - 29));
			return fullName;
		}

		protected LiteCollection<TModel> GetCollection(LiteDatabase connection)
		{
            try
            {
                var fullName = typeof(TModel).FullName;
				return connection.GetCollection<TModel>(GetClassTableName(typeof(TModel)));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public virtual XOFFOperationResult<IList<TModel>> All(Expression<Func<TModel, bool>> filter = null, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null, bool recursive = false)
        {
            try
            {
                using (var conn = Connection)
                {
                    List<TModel> list = GetAll(GetCollection(conn),filter, orderBy);
                    return XOFFOperationResult<IList<TModel>>.CreateSuccessResult(list);
                }
            }
            catch (Exception ex)
            {
                return XOFFOperationResult<IList<TModel>>.CreateFailure(ex);
            }
            finally
            {
                _connectionProvider.Release();
            }
        }

        protected virtual List<TModel> GetAll(LiteCollection<TModel> collection, Expression<Func<TModel, bool>> filter = null, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null)
        {
            IEnumerable<TModel> items;
            if (filter != null)
            {
                items = collection.Find(filter);
            }
            else
            {
                items = collection.FindAll();
            }
            if (orderBy != null)
            {
                items = orderBy(items.AsQueryable()).ToList();
            }
            var list = items.ToList();
            return list;
        }

        public virtual XOFFOperationResult Delete<T>(T id)
        {
            if (typeof(T) != typeof(TIdentifier))
            {
                throw new ArgumentException($"LocalId is not of type {typeof(TIdentifier)}");
            }
			return Delete((TIdentifier)(object)id).ToOperationResult();
        }
		public virtual XOFFOperationResult<TModel> Delete(TIdentifier id)
        {
            try
            {
                using (var conn = Connection)
                {
					var collection = GetCollection(conn);
					var item = collection.FindById(new BsonValue(id));
					var deleteSuccessful = collection.Delete(new BsonValue(id));
					if (deleteSuccessful)
					{
						return XOFFOperationResult<TModel>.CreateSuccessResult(item);
					}
					else 
					{
						return XOFFOperationResult<TModel>.CreateFailure("Failed to delete locally");
					}
                }
            }
            catch (Exception ex)
            {
				return XOFFOperationResult<TModel>.CreateFailure(ex);
            }
            finally
            {
                _connectionProvider.Release();
            }
        }

        public virtual XOFFOperationResult DeleteAll(Expression<Func<TModel, bool>> filter = null, bool recursive = false)
        {

            try
            {
                using (var conn = Connection)
                {
                    if (filter == null)
                    {

                        try
                        {
                            var collection = GetCollection(conn);
                            if (collection != null)
                            {
                                conn.DropCollection(GetClassTableName());
                            }
                        }
                        catch (Exception ex)
                        {
                            //this is here because if the collection doesn't already exist litedb throws an exception
                        }

                    }
                    else
                    {
                        var toDelete = GetCollection(conn).Delete(filter);
                        XOFFLoggerSingleton.Instance.LogMessage($"LiteDbRepository<{typeof(TModel).FullName}>", $"{toDelete} Items Deleted locally");
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
                _connectionProvider.Release();
            }

        }

        public virtual XOFFOperationResult DeleteAllInTransaction(ICollection<TModel> items, bool recursive = false)
        {

            try
            {
                using (var conn = Connection)
                {
                    using (var transaction = Connection.BeginTrans())
                    {
                        try
                        {
                            foreach (var model in items)
                            {
                                GetCollection(conn).Delete(x => x.LocalId.Equals(model.LocalId));
                            }
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
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
                _connectionProvider.Release();
            }

        }

        public virtual XOFFOperationResult<TModel> Get(TIdentifier id, bool withChildren = false, bool recursive = false)
        {
			try
			{
				using (var conn = Connection)
				{
					var item = GetCollection(conn).FindById(new BsonValue(id));
					if (item == null) 
					{
						return XOFFOperationResult<TModel>.CreateFailure("Item not found");
					}
					return XOFFOperationResult<TModel>.CreateSuccessResult(item);
				}
			}
			catch (Exception ex)
			{
				return XOFFOperationResult<TModel>.CreateFailure(ex);

			}
			finally 
			{
				_connectionProvider.Release();
			}
        }

        public virtual XOFFOperationResult<IList<TModel>> Get(List<TIdentifier> ids, bool withChildren = false, bool recursive = false)
        {
            try
            {
                using (var conn = Connection)
                {
                    var items = GetCollection(conn).Find(x => ids.Contains(x.LocalId)).ToList();
                    return XOFFOperationResult<IList<TModel>>.CreateSuccessResult(items);
                }
            }
            catch (Exception ex)
            {
                return XOFFOperationResult<IList<TModel>>.CreateFailure(ex);
                throw;
            }
            finally
            {
                _connectionProvider.Release();
            }
        }

        public virtual void Initialize()
        {
            //nothing needed here yet
        }

        public virtual XOFFOperationResult UpsertCollection(ICollection<TModel> items)
        {
            try
            {
                
                foreach (var model in items)
                {
                    Upsert(model);
                }
                return XOFFOperationResult.CreateSuccessResult("Success");
            }
            catch (Exception ex)
            {
                return XOFFOperationResult.CreateFailure(ex);
            }
        }

		public virtual XOFFOperationResult<TModel> Upsert(TModel entity) 
        {
            try
            {
				//todo(XOFF) change this so the upserts only change the date when done by the framework not local cals
                entity.LastTimeSynced = DateTime.UtcNow;
                using (var conn = Connection)
                {
                    var collection = GetCollection(conn);
                    var exists = collection.Exists(x => x.LocalId.Equals(entity.LocalId));
                    if (exists)
                    {
						collection.Update(entity);
                    }
                    else
                    {
                        collection.Insert(entity);
                    }
                }
                return XOFFOperationResult<TModel>.CreateSuccessResult(entity);
            }
            catch (Exception ex)
            {
                return XOFFOperationResult<TModel>.CreateFailure(ex);
            }
            finally
            {
                _connectionProvider.Release();
            }
        }

        public virtual XOFFOperationResult<string> InsertCallBack(string itemJson, string itemLocalId)
        {
            var item = (TModel)JsonConvert.DeserializeObject(itemJson, typeof(TModel));
            var result = Upsert(item);

            if (!result.Success)
            {
                return XOFFOperationResult<string>.CreateFailure("Failed to update");
            }
            else
            {
                return XOFFOperationResult<string>.CreateSuccessResult(item.RemoteId);
            }
        }

        public virtual XOFFOperationResult ReplaceAll(ICollection<TModel> items)
        {
            //NOTE(JACKSON) No this cannot be changed to string.isnullorwhitespace(); litedb doesn't process that correctly
            var deleteResult = DeleteAll(x=>x.RemoteId != null && x.RemoteId != string.Empty);//don't delete things that haven't been synced yet 
            if (!deleteResult.Success)
            {
                return deleteResult;
            }
            var upsertResult = UpsertCollection(items);
            return upsertResult;
        }
		public virtual XOFFOperationResult UpsertObject(object item)
		{
			if (item.GetType() != typeof(TModel))
			{
				return XOFFOperationResult.CreateFailure("");
			}
			else 
			{
				return Upsert((TModel)item).ToOperationResult();
			}
		}

		public XOFFOperationResult<int> Count()
		{
            try
            {
                using (var conn = Connection)
                {
                    var collection = GetCollection(conn);
					var count = collection.Count();
					return XOFFOperationResult<int>.CreateSuccessResult(count);
                }
            }
            catch (Exception ex)
            {
                return XOFFOperationResult<int>.CreateFailure(ex);
            }
            finally
            {
                _connectionProvider.Release();
            }
		}
	}
}

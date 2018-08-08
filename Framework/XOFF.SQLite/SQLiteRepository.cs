using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SQLite.Net;
using SQLiteNetExtensions.Extensions;
using XOFF.Core;

namespace XOFF.SQLite
{
    public class SQLiteRepository<TModel, TIdentifier> : ISQLiteRepository<TModel, TIdentifier> where TModel : class, IModel<TIdentifier>, new()
    {
        private const int EXPIRATIONMINUTES = 10080;
        protected SQLiteConnection Connection { get; }

        public virtual int ExpirationMinutes
        {
            get { return EXPIRATIONMINUTES; }
        }

        protected ISQLiteConnectionProvider _connectionProvider;

        public SQLiteRepository(ISQLiteConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
            Connection = connectionProvider.Connection;

            // perform initial setup
            Connection.CreateTable<TModel>();
            Initialize();
        }

        public virtual XOFFOperationResult<IList<TModel>> Get(List<TIdentifier> ids, bool withChildren = false, bool recursive = false)
        {
            /*
             * Note(Jackson) this method does not use getallwithchildren because we would have to write something 
             * to the effect of Connection.GetAllWithChildren(x=>ids.Contains(x.Id)); which can throw a SQLite exception if 
             * you have to many ids (the query generated has too many variables)
             *             
            
             */
            if (ids == null || ids.Count == 0)
            {
                return XOFFOperationResult<IList<TModel>>.CreateSuccessResult(new List<TModel>());
            }
            try
            {
                var items = new List<TModel>();
                foreach (var id in ids)
                {
                    var result = Get(id, withChildren, recursive:recursive);

                    if (result.Success)
                    {
                        if (result.Result != null)
                        {
                            items.Add(result.Result);
                        }
                    }
                }

                return XOFFOperationResult<IList<TModel>>.CreateSuccessResult(items);
            }
            catch (InvalidOperationException ex)
            {
                return XOFFOperationResult<IList<TModel>>.CreateFailure(ex);
            }
        }

        /// <summary>
        /// This method does not do anything with deferred execution. 
        /// Calling this will bring EVERYTHING into memory. Be careful.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public virtual XOFFOperationResult<IList<TModel>> All(Expression<Func<TModel, bool>> filter = null,
                Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null, bool recursive = false)
        {
            List<TModel> items = new List<TModel>();

            if (filter != null)
            {
                items = Connection.GetAllWithChildren<TModel>(filter,
                    recursive:recursive);
            }
            else
            {
                items = Connection.GetAllWithChildren<TModel>(recursive: recursive);
            }
            
            if (orderBy != null)
            {
                items = orderBy(items.AsQueryable()).ToList();
            }
            return XOFFOperationResult<IList<TModel>>.CreateSuccessResult(items);
           
        }

        public XOFFOperationResult<IList<TModel>> Get()
        {
            return All(null, null, true);
        }

        public XOFFOperationResult<TModel> Get(TIdentifier id)
        {
            return Get(id, true, true);
        }

        public virtual XOFFOperationResult<TModel> Get(TIdentifier id, bool withChildren = false, bool recursive = false)
        {
            XOFFOperationResult<TModel> result;
            try
            {
                if (!withChildren)
                {
                    var item = Connection.Get<TModel>(id);
                    return XOFFOperationResult<TModel>.CreateSuccessResult(item);
                }
                else
                {
                    var item = Connection.GetWithChildren<TModel>(id, recursive);
                    return XOFFOperationResult<TModel>.CreateSuccessResult(item);
                }
            }
            catch (InvalidOperationException ex) // If SQLite finds nothing it throws this exception
            {
                result = XOFFOperationResult<TModel>.CreateSuccessResult(null);
            }
            catch (Exception ex)
            {
                result = XOFFOperationResult<TModel>.CreateFailure(ex);
            }
            return result;
        }

        public virtual XOFFOperationResult Upsert(TModel item)
        {
            return Upsert(new List<TModel>() {item});
        }

        public virtual XOFFOperationResult Upsert(ICollection<TModel> items)
        {
			
            _connectionProvider.WaitOne();
            XOFFOperationResult result;
            try
            {
				foreach (var item in items)
				{
					item.LastTimeSynced = DateTime.UtcNow;
				}

				var objIds =
					items.Select(i => (object)i.Id)
						.ToList();
                Connection.RunInTransaction(() =>
                {
                    Connection.DeleteAll(items, recursive: true);
                    Connection.InsertAllWithChildren(items, recursive: true);
                });
                OnReplaceComplete(items);
                result  = XOFFOperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                result = XOFFOperationResult.CreateFailure(ex);
            }
            finally
            {
                _connectionProvider.Release();
            }
            return result;
        }
        public XOFFOperationResult Delete<T>(T id)
        {
            if (typeof(T) != typeof(TIdentifier))
            {
                throw new ArgumentException($"Id is not of type {typeof(TIdentifier)}");
            }
            return Delete((TIdentifier)(object)id);
        }

        public virtual XOFFOperationResult Delete(TIdentifier id)
        {
            XOFFOperationResult result;
            try
            {
                Connection.Delete<TModel>(id);
                Connection.Commit();
                result = XOFFOperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                result = XOFFOperationResult.CreateFailure(ex);
            }
            return result;
        }

        public virtual XOFFOperationResult DeleteAll(Expression<Func<TModel, bool>> filter = null, bool recursive = false)
        {
            XOFFOperationResult result;
            try
            {
                var modelsToDelete = All(filter, recursive: true);

                if (modelsToDelete.Success)
                {
                    Connection.DeleteAll(modelsToDelete.Result, recursive);
                }
                result = XOFFOperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                result = XOFFOperationResult.CreateFailure(ex);
            }
            return result;
        }

        /// <summary>
        /// This method will run the delete in a transaction. 
        /// If there is a failure none of the objects will be deleted.
        /// Each Item is deleted individually 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="recursive"></param>
        public virtual XOFFOperationResult DeleteAllInTransaction(ICollection<TModel> items, bool recursive = false)
        {
            XOFFOperationResult result;
            _connectionProvider.WaitOne();
            try
            {
                Connection.RunInTransaction(() =>
                {
                    foreach (var item in items)
                    {
                        Connection.Delete(item,recursive);
                    }
                });
                result = XOFFOperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                result = XOFFOperationResult.CreateFailure(ex);
            }
            finally
            {
                _connectionProvider.Release();
            }
            return result;
        }

     
        public virtual void Initialize()
        {
            // by default this method will do nothing, use if you need to expand on the typical initialization process
        }

        public virtual void OnReplaceComplete(ICollection<TModel> items)
        {
            // by default this method will do nothing, but use it if you need to take action after a replace
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
        public XOFFOperationResult Upsert(object item)
		{
			if (!(item is TModel))
			{
				throw new InvalidOperationException($"Item is not of type {typeof(TModel).FullName}");
			}
			var model = (TModel)item;
			return Upsert(model);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SQLite.Net;
using SQLiteNetExtensions.Extensions;
using XOFF.Core;

namespace XOFF.SQLite
{
    public class SQLiteRepository<TModel, TIdentifier> : ISQLiteRepository<TModel, TIdentifier> where TModel : class, IModel<TIdentifier>
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

        public virtual OperationResult<IList<TModel>> Get(List<TIdentifier> ids, bool withChildren = false, bool recursive = false)
        {
            /*
             * Note(Jackson) this method does not use getallwithchildren because we would have to write something 
             * to the effect of Connection.GetAllWithChildren(x=>ids.Contains(x.Id)); which can throw a SQLite exception if 
             * you have to many ids (the query generated has too many variables)
             *             
            
             */
            if (ids == null || ids.Count == 0)
            {
                return OperationResult<IList<TModel>>.CreateSuccessResult(new List<TModel>());
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

                return OperationResult<IList<TModel>>.CreateSuccessResult(items);
            }
            catch (InvalidOperationException ex)
            {
                return OperationResult<IList<TModel>>.CreateFailure(ex);
            }
        }

        /// <summary>
        /// This method does not do anything with deferred execution. 
        /// Calling this will bring EVERYTHING into memory. Be careful.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public virtual OperationResult<IList<TModel>> All(Expression<Func<TModel, bool>> filter = null,
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
            return OperationResult<IList<TModel>>.CreateSuccessResult(items);
           
        }

        public OperationResult<IList<TModel>> Get()
        {
            return All(null, null, true);
        }

        public OperationResult<TModel> Get(TIdentifier id)
        {
            return Get(id, true, true);
        }

        public virtual OperationResult<TModel> Get(TIdentifier id, bool withChildren = false, bool recursive = false)
        {
            OperationResult<TModel> result;
            try
            {
                if (!withChildren)
                {
                    var item = Connection.Get<TModel>(id);
                    return OperationResult<TModel>.CreateSuccessResult(item);
                }
                else
                {
                    var item = Connection.GetWithChildren<TModel>(id, recursive);
                    return OperationResult<TModel>.CreateSuccessResult(item);
                }
            }
            catch (InvalidOperationException ex) // If SQLite finds nothing it throws this exception
            {
                result = OperationResult<TModel>.CreateSuccessResult(null);
            }
            catch (Exception ex)
            {
                result = OperationResult<TModel>.CreateFailure(ex);
            }
            return result;
        }

        public virtual OperationResult Upsert(TModel item)
        {
            return Upsert(new List<TModel>() {item});
        }

        public virtual OperationResult Upsert(ICollection<TModel> items)
        {
            var objIds =
                items.Select(i => (object)i.Id)
                    .ToList();
            _connectionProvider.WaitOne();
            OperationResult result;
            try
            {
                Connection.RunInTransaction(() =>
                {
                    Connection.DeleteAll(items, recursive: true);
                    Connection.InsertAllWithChildren(items, recursive: true);
                });
                OnReplaceComplete(items);
                result  = OperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                result = OperationResult.CreateFailure(ex);
            }
            finally
            {
                _connectionProvider.Release();
            }
            return result;
        }

        public virtual OperationResult Delete(TIdentifier id)
        {
            OperationResult result;
            try
            {
                Connection.Delete<TModel>(id);
                Connection.Commit();
                result = OperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                result = OperationResult.CreateFailure(ex);
            }
            return result;
        }

        public virtual OperationResult DeleteAll(Expression<Func<TModel, bool>> filter = null, bool recursive = false)
        {
            OperationResult result;
            try
            {
                var modelsToDelete = All(filter, recursive: true);

                if (modelsToDelete.Success)
                {
                    Connection.DeleteAll(modelsToDelete.Result, recursive);
                }
                result = OperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                result = OperationResult.CreateFailure(ex);
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
        public virtual OperationResult DeleteAllInTransaction(ICollection<TModel> items, bool recursive = false)
        {
            OperationResult result;
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
                result = OperationResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                result = OperationResult.CreateFailure(ex);
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

        
    }
}

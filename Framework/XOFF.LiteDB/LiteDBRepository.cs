using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LiteDB;
using XOFF.Core;
using XOFF.Core.Repositories;

namespace XOFF.LiteDB
{
	public interface ILiteDBRepository<TModel, TIdentifier> : IRepository<TModel,TIdentifier> where TModel : class, IModel<TIdentifier>, new()

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

        readonly ILiteDbConnectionProvider _connectionProvider;

        public LiteDBRepository(ILiteDbConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;

        }

        private LiteCollection<TModel> GetCollection(LiteDatabase connection)

        {
            var fullName = typeof(TModel).FullName;
            return connection.GetCollection<TModel>(fullName.Substring(Math.Max(0, fullName.Length - 4)));
            
        }

        public OperationResult<IList<TModel>> All(Expression<Func<TModel, bool>> filter = null, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null, bool recursive = false)
		{
		    try
		    {
		        using (var conn = Connection)
		        {
		            IEnumerable<TModel> items = new List<TModel>();
		            var collection = GetCollection(conn);
		            if (filter != null)
		            {
		                items = collection.Find(filter);
		            }
		            else
		            {
		                items = collection.FindAll();
		            }

		            //recursive is by default because this stuff is being stored as json documents 

		            if (orderBy != null)
		            {
		                items = orderBy(items.AsQueryable()).ToList();
		            }

		            return OperationResult<IList<TModel>>.CreateSuccessResult(items.ToList());
		        }
		    }
		    catch (Exception ex)
		    {
		        return OperationResult<IList<TModel>>.CreateFailure(ex);
		    }
		    finally
		    {
                _connectionProvider.Release();
            }
		}
        public OperationResult Delete<T>(T id)
        {
            if (typeof(T) != typeof(TIdentifier))
            {
                throw new ArgumentException($"Id is not of type {typeof(TIdentifier)}");
            }
            return Delete((TIdentifier)(object)id);
        }
        public OperationResult Delete(TIdentifier id)
		{
		    try
		    {
		        using (var conn = Connection)
		        {
		            GetCollection(conn).Delete(x => x.Id.Equals(id));
		            return OperationResult.CreateSuccessResult("Success");
		        }
		    }
		    catch (Exception ex)
		    {
		        return OperationResult.CreateFailure(ex);
		    }
		    finally
		    {
                _connectionProvider.Release();
            }
		}

		public OperationResult DeleteAll(Expression<Func<TModel, bool>> filter = null, bool recursive = false)
		{

            try
            {
				using (var conn = Connection)
				{
					if (filter == null)
					{
                        GetCollection(conn).Delete(x=>x.Id != null);//kindof hacky

                    }
					else
					{
						GetCollection(conn).Delete(filter);
					}
				}
                return OperationResult.CreateSuccessResult("Success");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(ex);
            }
            finally
            {
                _connectionProvider.Release();
            }

        }

		public OperationResult DeleteAllInTransaction(ICollection<TModel> items, bool recursive = false)
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
								GetCollection(conn).Delete(x => x.Id.Equals(model.Id));
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
                return OperationResult.CreateSuccessResult("Success");
		    }
		    catch (Exception ex)
		    {
		        return OperationResult.CreateFailure(ex);
		    }
            finally
            {
                _connectionProvider.Release();
            }

        }

		public OperationResult<TModel> Get(TIdentifier id, bool withChildren = false, bool recursive = false)
		{
		    try
		    {
				using (var conn = Connection)
				{
					var item = GetCollection(conn).FindById(new BsonValue(id));
					return OperationResult<TModel>.CreateSuccessResult(item);
				}
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
				using (var conn = Connection)
				{
					var items = GetCollection(conn).Find(x => ids.Contains(x.Id)).ToList();
					return OperationResult<IList<TModel>>.CreateSuccessResult(items);
				}
            }
            catch (Exception ex)
            {
                return OperationResult<IList<TModel>>.CreateFailure(ex);
                throw;
            }
            finally
            {
                _connectionProvider.Release();
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
				entity.LastTimeSynced = DateTime.UtcNow;
				using (var conn = Connection)
				{
					var collection = GetCollection(conn);
					var exists = collection.Exists(x => x.Id.Equals(entity.Id));
					if (exists)
					{
						collection.Update(entity);
					}
					else
					{
						collection.Insert(entity);
					}
				}
                return OperationResult.CreateSuccessResult("Success");
            }
		    catch (Exception ex)
		    {
                return OperationResult.CreateFailure(ex);
            }
            finally
            {
                _connectionProvider.Release();
            }
        }
        public OperationResult ReplaceAll(ICollection<TModel> items)
        {
            var deleteResult = DeleteAll();
            if (!deleteResult.Success)
            {
                return deleteResult;
            }
            var upsertResult = Upsert(items);
            return upsertResult;
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
	}
}

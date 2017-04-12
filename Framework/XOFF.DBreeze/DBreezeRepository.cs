using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using XOFF.Core;
using XOFF.Core.Repositories;
using DBreeze.Utils;
using DBreeze;
using System.IO;

namespace XOFF.DBreeze
{



	public class DBreezeRepository<TModel, TIdentifier> : IDBreezeRepository<TModel, TIdentifier> where TModel : class, IModel<TIdentifier>
	{
		readonly DBreezeEngine _engine;
		readonly string _tableName;

		public DBreezeRepository(IDBreezeConnectionProvider provider, string tableName = null)
		{
			_engine = provider.Engine;
			//CustomSerializator.ByteArraySerializator = (object o) => { return NetJSON.NetJSON.Serialize(o).To_UTF8Bytes(); };
			//CustomSerializator.ByteArrayDeSerializator = (byte[] bt, Type t) => { return NetJSON.NetJSON.Deserialize(t, bt.UTF8_GetString()); };

			_tableName = tableName ?? typeof(TModel).FullName;
		}

		public OperationResult<IList<TModel>> All(Expression<Func<TModel, bool>> filter = null, Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null, bool recursive = false)
		{

			try
			{
				//todo this likely could be more efficient
				using (var transaction = _engine.GetTransaction())
				{
					var items = transaction.SelectForward<TIdentifier, TModel>(_tableName)
						.Select(x => x.Value);

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
			catch (Exception ex)
			{
				return OperationResult<IList<TModel>>.CreateFailure(ex);
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
				using (var transaction = _engine.GetTransaction())
				{
					var row = transaction.Select<TIdentifier, TModel>(_tableName, id);
					return OperationResult<TModel>.CreateSuccessResult(row.Value);
				}
			}
			catch (Exception ex)
			{
				return OperationResult<TModel>.CreateFailure(ex);
			}
		}

		public OperationResult<IList<TModel>> Get(List<TIdentifier> ids, bool withChildren = false, bool recursive = false)
		{
			try
			{
				//todo this likely could be more efficient
				using (var transaction = _engine.GetTransaction())
				{
					List<TModel> items = new List<TModel>();
					foreach (var id in ids)
					{
						var row = transaction.Select<TIdentifier, TModel>(_tableName, id);
						items.Add(row.Value);
					}

					return OperationResult<IList<TModel>>.CreateSuccessResult(items.ToList());
				}
			}
			catch (Exception ex)
			{
				return OperationResult<IList<TModel>>.CreateFailure(ex);
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
				using (var trans = _engine.GetTransaction())
				{

					var exists = trans.Select<TIdentifier, TModel>(_tableName, entity.Id) != null;
					if (exists)
					{
						trans.RemoveKey(_tableName, entity.Id);
					}
					trans.Insert(_tableName, entity.Id, entity);

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

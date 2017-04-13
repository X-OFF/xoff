using System;
using XOFF.Core;
using XOFF.Core.Repositories;

namespace XOFF.DBreeze
{
	public interface IDBreezeRepository<TModel, TIdentifier> : IRepository<TModel,TIdentifier> where TModel: class,IModel<TIdentifier>
	{
	}

	

}

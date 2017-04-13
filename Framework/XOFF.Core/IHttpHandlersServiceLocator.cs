using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using XOFF.Core.ChangeQueue;
using XOFF.Core.Remote;
using XOFF.Core.Repositories;

namespace XOFF.Core
{

	public interface IHttpHandlersServiceLocator
	{
		IHttpDeleteHandler ResolveDeleteHandler(Type changedItemType, Type changedItemIdentifierType);
		IHttpCreateHandler ResolveCreateHandler(Type changedItemType, Type changedItemIdentifierType);
		IHttpUpdateHandler ResolveUpdateHandler(Type changedItemType, Type changedItemIdentifierType);
	}

	public class AutoFacHttpHandlersServiceLocator : IHttpHandlersServiceLocator
	{
		readonly IComponentContext _context;

		public AutoFacHttpHandlersServiceLocator(IComponentContext context) 
		{
			_context = context;
		}

		public IHttpCreateHandler ResolveCreateHandler(Type changedItemType, Type changedItemIdentifierType)
		{
			var handler = _context.Resolve(typeof(IHttpEntityCreateHandler<,>).MakeGenericType(changedItemType,changedItemIdentifierType));
			return (IHttpCreateHandler)handler;
		}

		public IHttpDeleteHandler ResolveDeleteHandler(Type changedItemType, Type changedItemIdentifierType)
		{
			var handler = _context.Resolve(typeof(IHttpEntityDeleteHandler<,>).MakeGenericType(changedItemType, changedItemIdentifierType));
			return (IHttpDeleteHandler)handler;
		}

		public IHttpUpdateHandler ResolveUpdateHandler(Type changedItemType, Type changedItemIdentifierType)
		{
			var handler = _context.Resolve(typeof(IHttpEntityUpdateHandler<,>).MakeGenericType(changedItemType, changedItemIdentifierType));
			return (IHttpUpdateHandler)handler;
		}
	}

	public interface IRepositoryServiceLocator
	{
		IObjectRepository ResolveRepository(Type changedItemType, Type changedItemIdentifierType);
	}

	public class AutofacRepositoryServiceLocator : IRepositoryServiceLocator
	{
		readonly IComponentContext _context;

		public AutofacRepositoryServiceLocator(IComponentContext context)
		{

			_context = context;
		}

		public IObjectRepository ResolveRepository(Type changedItemType, Type changedItemIdentifierType)
		{
			var repository = _context.Resolve(typeof(IRepository<,>).MakeGenericType(changedItemType, changedItemIdentifierType));
			return (IObjectRepository)repository;
		}
	}
}

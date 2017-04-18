using System;
using Autofac;
using XOFF.Core.Repositories;

namespace XOFF.Core
{
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
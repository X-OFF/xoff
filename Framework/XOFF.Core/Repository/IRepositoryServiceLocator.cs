using System;
using XOFF.Core.Repositories;

namespace XOFF.Core.Repositories
{
    public interface IRepositoryServiceLocator
    {
        IObjectRepository ResolveRepository(Type changedItemType, Type changedItemIdentifierType);
    }
}
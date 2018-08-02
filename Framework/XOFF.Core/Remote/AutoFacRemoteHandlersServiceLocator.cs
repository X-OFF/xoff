using System;
using Autofac;

namespace XOFF.Core.Remote
{
    public class AutofacRemoteHandlersServiceLocator : IRemoteHandlersServiceLocator
    {
        readonly IComponentContext _context;

        public AutofacRemoteHandlersServiceLocator(IComponentContext context) 
        {
            _context = context;
        }

        public IRemoteCreateHandler ResolveCreateHandler(Type changedItemType, Type changedItemIdentifierType)
        {
            var handler = _context.Resolve(typeof(IRemoteEntityCreateHandler<,>).MakeGenericType(changedItemType,changedItemIdentifierType));
            return (IRemoteCreateHandler)handler;
        }

        public IRemoteDeleteHandler ResolveDeleteHandler(Type changedItemType, Type changedItemIdentifierType)
        {
            var handler = _context.Resolve(typeof(IRemoteEntityDeleteHandler<,>).MakeGenericType(changedItemType, changedItemIdentifierType));
            return (IRemoteDeleteHandler)handler;
        }

        public IRemoteUpdateHandler ResolveUpdateHandler(Type changedItemType, Type changedItemIdentifierType)
        {
            var handler = _context.Resolve(typeof(IRemoteEntityUpdateHandler<,>).MakeGenericType(changedItemType, changedItemIdentifierType));
            return (IRemoteUpdateHandler)handler;
        }

        public IRemoteGetHandler ResolveGetHandler(Type modelType, Type identifierType)
        {
            try
            {
                var handler =
                    _context.Resolve(typeof(IRemoteEntityGetHandler<,>).MakeGenericType(modelType, identifierType));
                return (IRemoteGetHandler) handler;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
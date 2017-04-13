using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using XOFF.Core.ChangeQueue;
using XOFF.Core.Remote;

namespace XOFF.Core
{

	public interface IRemoteHandlersServiceLocator
	{
		IRemoteDeleteHandler ResolveDeleteHandler(Type changedItemType, Type changedItemIdentifierType);
		IRemoteCreateHandler ResolveCreateHandler(Type changedItemType, Type changedItemIdentifierType);
		IRemoteUpdateHandler ResolveUpdateHandler(Type changedItemType, Type changedItemIdentifierType);
	    IRemoteGetHandler ResolveGetHandler(Type modelType, Type identifierType);
	}
}

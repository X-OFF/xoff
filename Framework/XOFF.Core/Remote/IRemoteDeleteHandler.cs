using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XOFF.Core.Remote
{

	public interface IRemoteDeleteHandler
	{
		Task<OperationResult> Delete(object model);
		Task<OperationResult> DeleteById(object id);
	}
	
}
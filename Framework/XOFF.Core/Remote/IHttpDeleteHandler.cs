using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XOFF.Core.Remote
{

	public interface IHttpDeleteHandler
	{
		Task<OperationResult> Delete(object model);
		Task<OperationResult> DeleteById(object id);
	}
	
}
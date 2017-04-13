using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XOFF.Core.Remote
{

	public interface IRemoteUpdateHandler 
	{
		Task<OperationResult> Update(object model);
	}
}
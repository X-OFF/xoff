using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XOFF.Core.Remote
{

	public interface IHttpUpdateHandler 
	{
		Task<OperationResult> Update(object model);
	}
	
}
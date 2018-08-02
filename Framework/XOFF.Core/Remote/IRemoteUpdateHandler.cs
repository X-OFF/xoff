using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XOFF.Core.ChangeQueue;

namespace XOFF.Core.Remote
{

	public interface IRemoteUpdateHandler 
	{
		Task<XOFFOperationResult> Update(ChangeQueueItem model);
	}
}
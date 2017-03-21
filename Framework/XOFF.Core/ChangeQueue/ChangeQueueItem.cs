using System;
using XOFF.Core.Repositories;

namespace XOFF.Core.ChangeQueue
{

	public class ChangeQueueItem : IModel<Guid>
	{
		public Guid Id { get; set; }

		public DateTime LastTimeSynced { get; set; }

		public string ChangedItemJson { get; set; }

		public string ChangedItemId { get; set; }

		public string ChangeType { get; set; }

		public Type ChangedItemType { get; set;}

		public Type ChangedItemIdentifierType { get; set;}
	}
	
}

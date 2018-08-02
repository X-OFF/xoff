using System;
using LiteDB;
using XOFF.Core.Repositories;

namespace XOFF.Core.ChangeQueue
{
    [Serializable]
	public class ChangeQueueItem : IModel<Guid>
	{
		[BsonId]
		public Guid LocalId { get; set; }

		public DateTime LastTimeSynced { get; set; }

        public DateTime CreateDateTime { get; set; }

		public string ChangedItemJson { get; set; }

		public string ChangedItemId { get; set; }

		public string ChangedItemLocalId { get; set; }

		public string ChangeType { get; set; }
       
		public string ChangeItemTypeString { get; set; }
		public string ChangedItemIdentifierTypeString { get; set; }

        
		[BsonField]
	    public int FailedAttempts { get; set; }
		[BsonField]
		public bool SuccessfullyProcessed { get; set; }

		[BsonIgnore]
		public string RemoteId
		{
			get
			{
				return LocalId.ToString();
			}

			set
			{
				LocalId = Guid.Parse(value);
			}
		}

		public int ApiSortOrder { get; set; }
	}
	
}

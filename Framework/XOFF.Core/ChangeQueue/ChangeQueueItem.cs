using System;
using LiteDB;
using XOFF.Core.Repositories;

namespace XOFF.Core.ChangeQueue
{
    [Serializable]
	public class ChangeQueueItem : IModel<Guid>
	{
		public Guid Id { get; set; }

		public DateTime LastTimeSynced { get; set; }

		public string ChangedItemJson { get; set; }

		public string ChangedItemId { get; set; }

		public string ChangeType { get; set; }
        [BsonIgnore]
        private Type _changedItemType;
		public string ChangeItemTypeString { get; set; }
        [BsonIgnore]
        public Type ChangedItemType
		{
			get
			{
				return _changedItemType ?? Type.GetType(ChangeItemTypeString);
			}

			set
			{
				_changedItemType = value;
				ChangeItemTypeString = value.FullName;
			}
		}
        [BsonIgnore]
        private Type _changedItemIdentifierType;
		public string ChangedItemIdentifierTypeString { get; set; }
        [BsonIgnore]
        public Type ChangedItemIdentifierType
		{
			get
			{
				return _changedItemIdentifierType ?? Type.GetType(ChangedItemIdentifierTypeString);;
			}

			set
			{
				_changedItemIdentifierType = value;
				ChangedItemIdentifierTypeString = value.FullName;
			}
		}

	    public int FailedAttempts { get; set; }
	}
	
}

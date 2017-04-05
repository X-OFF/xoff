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

		private Type _changedItemType;
		public string ChangeItemTypeString { get; set; }
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

		private Type _changedItemIdentifierType;
		public string ChangedItemIdentifierTypeString { get; set; }
		public Type ChangedItemIdentifierType
		{
			get
			{
				return _changedItemIdentifierType ?? Type.GetType(ChangeItemTypeString);;
			}

			set
			{
				_changedItemIdentifierType = value;
				ChangedItemIdentifierTypeString = value.FullName;
			}
		}




	}
	
}

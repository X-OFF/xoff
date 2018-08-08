using System;
using XOFF.Core;
namespace OfflineFirstReferenceArch.Models
{
    [Serializable]
	public class Widget : IModel<Guid>
	{
		public string Name { get; set; }
		public DateTime LastTimeSynced { get; set; }
        public Guid LocalId { get; set; }
        public string RemoteId { get; set; }
        public int ApiSortOrder { get; set; }
    }
}

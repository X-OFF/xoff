using System;
using XOFF.Core;
namespace OfflineFirstReferenceArch.Models
{
    [Serializable]
	public class Widget : IModel<Guid>

	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public DateTime LastTimeSynced { get; set; }
	}
}

using System;
using XOFF.Core;
namespace OfflineFirstReferenceArch.Models
{
	public class Widget : IModel<Guid>

	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public DateTime LastTimeSynced { get; set; }
	}
}

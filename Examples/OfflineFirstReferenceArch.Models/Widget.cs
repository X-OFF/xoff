using System;
using System.Collections.Generic;
using System.Text;
using SQLite.Net.Attributes;
using XOFF.Core;

namespace OfflineFirstReferenceArch.Models
{
    public class Widget : IModel<Guid>
    {
		[PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime LastTimeSynced { get; set; }
    }
}

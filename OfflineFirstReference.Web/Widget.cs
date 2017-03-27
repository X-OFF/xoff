using System;
using System.ComponentModel.DataAnnotations;

namespace OfflineFirstReference.Web
{
    public class Widget 

    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime LastTimeSynced { get; set; }
    }
}

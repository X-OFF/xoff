using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XOFF.Core;

namespace XOFF.Tests.Framework
{
    public class Person : IModel<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Salutation { get; set; }
        public string Address { get; set; }
        public string HairColor { get; set; }
        public Guid Id { get; set; }
        public DateTime LastTimeSynced { get; set; }
    }
}

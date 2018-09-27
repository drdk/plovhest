using System;
using System.Collections.Generic;
using System.Text;

namespace Plovhest.Shared
{
    public class OrderIdentifier
    {
        public int Id { get; set; }
        public IList<string> HangfireIds { get; set; }
    }
}

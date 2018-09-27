using System;
using System.Collections.Generic;
using System.Text;

namespace Plovhest.Shared
{
    public struct OrderRequest
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public OrderType OrderType { get; set; }
        public Uri Callback { get; set; }
    }
}

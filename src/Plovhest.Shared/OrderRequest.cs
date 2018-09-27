namespace Plovhest.Shared
{
    using System;

    public struct OrderRequest
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public OrderType OrderType { get; set; }
        public Uri Callback { get; set; }
    }
}
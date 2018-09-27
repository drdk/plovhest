namespace Plovhest.Shared
{
    using System.Collections.Generic;

    public class OrderIdentifier
    {
        public int Id { get; set; }
        public IList<string> HangfireIds { get; set; }
    }
}

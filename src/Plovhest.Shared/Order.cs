using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Plovhest.Shared
{
    public class Order
    {
        public int Id { get; set; }
        public string Executable { get; set; }
        public string Arguments { get; set; }
        public State State { get; set; }
        public JObject Result { get; set; }
    }

    public enum State
    {
        Invalid = 0,
        Queued = 1,
        Running = 2,
        Done = 3,
        Failed = 4,
        Canceled = 5
    }
}

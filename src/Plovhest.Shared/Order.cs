using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Plovhest.Shared
{
    public class Order
    {
        public int Id { get; set; }
        public State State { get; set; }
        public JObject Data { get; set; } = new JObject();
        public Uri Callback { get; set; }

        [NotMapped]
        public IEnumerable<Task> Tasks
        {
            get => Data["Tasks"]?.ToObject< IEnumerable< Task>>();
            set => Data["Tasks"] = JArray.FromObject(value);
        }
    }

    public class Task
    {
        public string HangfireId { get; set; }
        public string Executable { get; set; }
        public string Arguments { get; set; }
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

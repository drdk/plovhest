using System;
using System.Collections.Generic;
using System.Text;
using Plovhest.Shared;
using Xunit;

namespace Plovhest.Test
{
    public class JsonTest
    {
        [Fact]
        public void Test1()
        {
            var order = new Order
            {
                Callback = new Uri("rabbitlol://foobar"),
                State = State.Queued,
                Tasks = new [] { new Task
                {
                    HangfireId = Guid.NewGuid().ToString(),
                    Arguments = "args",
                    Executable = "dir.exe"
                } }
            };
        }
    }
}

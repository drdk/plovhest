namespace Plovhest.Test
{
    using System;
    using Shared;
    using Xunit;

    public class JsonTest
    {
        [Fact]
        public void OrderDataTest()
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
            Assert.NotEqual("{}",order.Data.ToString());

            order = new Order();
            Assert.Equal("{}",order.Data.ToString());
        }
    }
}

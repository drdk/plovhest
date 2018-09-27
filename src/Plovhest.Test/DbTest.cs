namespace Plovhest.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Shared;
    using Xunit;

    public class DbTest : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        public DbTest()
        {
            var connectionString = "Server=.;Database=PlovhestTest;Trusted_Connection=True;Integrated security=True;MultipleActiveResultSets=True;";
            _serviceProvider = new ServiceCollection()
                .AddDbContext<PlovhestDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                    options.EnableSensitiveDataLogging();
                })
                .BuildServiceProvider();
        
            PlovhestDbContext.Initialize(_serviceProvider, delete:true);
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }

        [Fact]
        public void EFModelTest()
        {
            using (var serviceScope = _serviceProvider.CreateScope())
            using (var context = serviceScope.ServiceProvider.GetService<PlovhestDbContext>())
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

                context.Orders.Attach(order);
                Assert.Equal(1, context.SaveChanges());

                var dbOrder = context.Orders.FirstOrDefault();
                Assert.NotNull(dbOrder);
                Assert.Single(dbOrder.Tasks);
                order.Tasks = new List<Task> { new Task { Arguments = "dsfdsf", Executable = "explorer.exe" }};
                context.SaveChanges();

                //dbOrder = context.Orders.First();
                var tasks = order.Tasks.ToList();
                tasks[0].HangfireId = Guid.NewGuid().ToString();
                order.Tasks = tasks;
                context.SaveChanges();
                dbOrder = context.Orders.First();
                Assert.Single(dbOrder.Tasks);
            }

        }
    }
}
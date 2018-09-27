using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Plovhest.Shared;
using Xunit;

namespace Plovhest.Test
{
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
        public void Test1()
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

                context.Orders.Add(order);
                Assert.Equal(1, context.SaveChanges());

                var dbOrder = context.Orders.FirstOrDefault();
                Assert.NotNull(dbOrder);

            }

        }
    }
}

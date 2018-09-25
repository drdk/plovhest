using System;
using System.Diagnostics;
using System.IO;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plovhest.Shared;

namespace Plovhest.Executor
{
    class Program
    {
        private static readonly ISettings Settings = new Settings();
        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            
            configuration.Bind(Settings);

            var serviceProvider = new ServiceCollection()
                .AddLogging(loggingBuilder => loggingBuilder.AddConfiguration(configuration.GetSection("Logging")) )
                .AddSingleton(Settings)
                .AddTransient<ProcessWrapper>()
                .AddDbContext<PlovhestDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("PlovhestDatabase")))
                .BuildServiceProvider();

            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug);

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            
            GlobalConfiguration.Configuration.UseActivator(new ServiceProviderJobActivator(serviceProvider));

            GlobalConfiguration.Configuration
                .UseSqlServerStorage(configuration.GetConnectionString("PlovhestDatabase"));

            using (var server = new BackgroundJobServer(
                new BackgroundJobServerOptions
                {
                    WorkerCount = 1
                }))
            {
                logger.LogDebug("Hangfire Server started.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Plovhest.Shared
{
    public class ProcessWrapper : IDisposable
    {
        private readonly PlovhestDbContext _context;
        private readonly ISettings _settings;
        private readonly ILogger _logger;

        public ProcessWrapper(PlovhestDbContext context, ISettings settings, ILoggerFactory loggerFactory)
        {
            _context = context;
            _settings = settings;
            _logger = loggerFactory.CreateLogger<ProcessWrapper>();

        }
        public void Run(int orderId, string executable, string arguments, IJobCancellationToken cancellationToken)
        {
            var order = _context.Orders.First(o => o.Id == orderId);

            if (! new[] {State.Queued, State.Running}.Contains(order.State))
            {
                _logger.LogWarning($"Invalid order state : {order.State}");
                return;
            }

            order.State = State.Running;
            _context.SaveChanges();


            if (executable == "{FFmpegPath}")
                executable = _settings.FFmpegPath;
            else
            {
                _logger.LogError($"Invalid executable : {executable}");
                order.State = State.Failed;
                _context.SaveChanges();
                return;
            }

          
            _logger.LogInformation($"Order {order.Id} started {executable} {arguments}");
            
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        FileName = executable,
                        Arguments = arguments
                    }
                };

                process.OutputDataReceived += DataReceived;
                process.ErrorDataReceived += DataReceived;
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                process.WaitForExit();
                _logger.LogInformation($"Order {order.Id} {executable} {arguments} done");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to start process for order {order.Id}");
                order.State = State.Failed;
                _context.SaveChanges();
            }

        }

        private static void DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            Console.WriteLine(e.Data);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

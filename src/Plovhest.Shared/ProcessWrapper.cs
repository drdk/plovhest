using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Plovhest.Shared
{
    public class ProcessWrapper
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
        public void Run(int orderId)
        {
            if (_settings == null)
            {
                throw new Exception("Settings not initialized");
            }

            var order = _context.Orders.First(o => o.Id == orderId);

            if (order.Executable == "{FFmpegPath}")
                order.Executable = _settings.FFmpegPath;

            order.State = State.Running;
            _context.SaveChanges();
            _logger.LogInformation($"Order {order.Id} started.");
            
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        FileName = order.Executable,
                        Arguments = order.Arguments
                    }
                };

                process.OutputDataReceived += DataReceived;
                process.ErrorDataReceived += DataReceived;
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                process.WaitForExit();
                order.State = State.Done;
                _context.SaveChanges();
                _logger.LogInformation($"Order {order.Id} done.");
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
    }
}

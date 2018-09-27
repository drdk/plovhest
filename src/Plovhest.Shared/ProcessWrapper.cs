namespace Plovhest.Shared
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Hangfire;
    using Microsoft.Extensions.Logging;

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

            if (! new[] { State.Queued, State.Running }.Contains(order.State))
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
                process.ErrorDataReceived += ErrorReceived;
                process.Start();
                cancellationToken.ShutdownToken.Register(process.Kill);
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                process.WaitForExit();
                if (cancellationToken.ShutdownToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"Order {order.Id} canceled via hangfire.");
                    order.State = State.Canceled;
                    _context.SaveChanges();
                }
                _logger.LogInformation($"Order {order.Id} {executable} {arguments} done");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to start process for order {order.Id}");
                order.State = State.Failed;
                _context.SaveChanges();
            }

        }

        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            _logger.LogDebug(e.Data);
        }
        private void ErrorReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            _logger.LogWarning(e.Data);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
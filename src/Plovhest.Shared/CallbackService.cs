

namespace Plovhest.Shared
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class CallbackService : IDisposable
    {
        private readonly PlovhestDbContext _context;
        private readonly ISettings _settings;
        private readonly ILogger _logger;

        public CallbackService(PlovhestDbContext context, ISettings settings, ILoggerFactory loggerFactory)
        {
            _context = context;
            _settings = settings;
            _logger = loggerFactory.CreateLogger<CallbackService>();
        }

        public async Task<int> Run(int orderId, Uri callbackUri)
        {
            var order = await _context.Orders.FirstAsync(o => o.Id == orderId);
            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                await wc.UploadStringTaskAsync(
                    callbackUri,
                    JsonConvert.SerializeObject(order, JsonHelper.JsonSerializerSettings));
            }

            order.State = State.Done;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Order id {order.Id} Done. Callback to {callbackUri} send.");
            return 1;
        } 

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

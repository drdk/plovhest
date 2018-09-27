using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Plovhest.Shared
{
    public class CallbackService : IDisposable
    {
        private readonly PlovhestDbContext _context;
        private readonly ISettings _settings;
        private readonly ILogger _logger;

        public CallbackService(PlovhestDbContext context, ISettings settings, ILoggerFactory loggerFactory)
        {
            _context = context;
            _settings = settings;
            _logger = loggerFactory.CreateLogger<ProcessWrapper>();
        }

        public async Task<int> Run(int orderId, Uri callbackUri)
        {
            var order = await _context.Orders.FirstAsync(o => o.Id == orderId);
            using (var wc = new WebClient())
            {
                 await wc.UploadStringTaskAsync(callbackUri,JsonConvert.SerializeObject(order,JsonHelper.JsonSerializerSettings));
            }

            order.State = State.Done;
            await _context.SaveChangesAsync();
            return 1;

        } 

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

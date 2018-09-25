using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Plovhest.Shared;

namespace Plovhest.Web.Controllers
{
    /// <summary>
    /// Sample Job controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly PlovhestDbContext _context;
        private static ProcessWrapper _pw;

        public OrderController(PlovhestDbContext context, ProcessWrapper pw)
        {
            _context = context;
            _pw = pw;
        }
        /// <summary>
        /// Create FFmpeg transcoding job. 
        /// </summary>
        /// <returns>Order Id</returns>
        [HttpPost]
        public ActionResult<int> Transcode(string source, string destination, Format format)
        {
            var order = new Order
            {
                Executable = "{FFmpegPath}",
                Arguments = $"-i {source} -c:v libvpx-vp9 -b:v 2M -c:a libopus {destination}",
                State = State.Queued
            };
            _context.Orders.Attach(order);
            _context.SaveChanges();
            BackgroundJob.Enqueue(() => _pw.Run(order.Id));
            return order.Id;
        }

        [HttpGet]
        public ActionResult<Order> GetStatus(int id)
        {
            return _context.Orders.FirstOrDefault(o => o.Id == id);
        }
    }
}
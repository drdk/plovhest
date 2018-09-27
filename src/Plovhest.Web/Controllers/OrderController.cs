using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Plovhest.Shared;
using Task = Plovhest.Shared.Task;

namespace Plovhest.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly PlovhestDbContext _context;

        public OrderController(PlovhestDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create FFmpeg transcoding job. 
        /// </summary>
        /// <returns>Order Id</returns>
        [HttpPost]
        public ActionResult<OrderIdentifier> Enqueue(OrderRequest request)
        {
            if (request.OrderType != OrderType.VideoEncodeVP9)
                throw new Exception($"Invalid order {request.OrderType.ToString()}");
            var order = new Order
            {
                State = State.Queued,
                Callback = request.Callback
            };
            _context.Orders.Attach(order);
            _context.SaveChanges();
            var tasks = new List<Task>()
            {
                new Task
                {
                    Executable = "{FFmpegPath}",
                    Arguments =
                        $"-y -i \"{request.Source}\" -vf yadif=1:-1:0 -vf scale=640:-1 -c:v libvpx-vp9 -pass 1 -b:v 2000K -keyint_min 60 -g 60 -threads 8 -speed 4 -tile-columns 4 -f webm NUL"
                },
                new Task
                {
                    Executable = "{FFmpegPath}",
                    Arguments =
                        $"-y -i \"{request.Source}\" -vf yadif=1:-1:0 -vf scale=640:-1 -c:v libvpx-vp9 -pass 2 -b:v 2000K -keyint_min 60 -g 60 -threads 8 -speed 2 -tile-columns 4 -c:a libopus -b:a 128k -f webm \"{request.Destination}\""
                },
            };
            tasks[0].HangfireId = BackgroundJob.Enqueue<ProcessWrapper>(x => x.Run(order.Id,tasks[0].Executable, tasks[0].Arguments, JobCancellationToken.Null));
            tasks[1].HangfireId = BackgroundJob.ContinueWith<ProcessWrapper>(tasks[0].HangfireId, x => x.Run(order.Id,tasks[1].Executable, tasks[1].Arguments, JobCancellationToken.Null));
            var callbackHangfireId = BackgroundJob.ContinueWith<CallbackService>(tasks[1].HangfireId, x => x.Run(order.Id, request.Callback));
            order.Tasks = tasks;
            _context.SaveChanges();

          return new OrderIdentifier
          {
              Id = order.Id,
              HangfireIds = new [] {tasks[0].HangfireId, tasks[1].HangfireId, callbackHangfireId}
          };
        }

        [HttpGet]
        public ActionResult<Order> GetStatus(int id)
        {
            return _context.Orders.FirstOrDefault(o => o.Id == id);
        }

        [HttpPost("EchoTest")]
        public ActionResult<string> EchoTest(Order order)
        {
            return "ok";
        }
    }
}
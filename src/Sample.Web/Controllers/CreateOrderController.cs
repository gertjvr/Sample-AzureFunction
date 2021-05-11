using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.Contracts;

namespace Sample.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CreateOrderController : ControllerBase
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public CreateOrderController(ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider = sendEndpointProvider;
        }

        [HttpGet, HttpPost]
        public async Task<string> CreateOrder()
        {
            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:submit-order-state"));
            
            var orderId = Guid.NewGuid();
            var orderNumber = "123";
            
            await endpoint.Send<SubmitOrder>(new
            {
                OrderId = orderId,
                OrderNumber = orderNumber
            });

            return "Order Submitted";
        }
    }
}
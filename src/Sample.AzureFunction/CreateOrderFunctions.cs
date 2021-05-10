using System;
using System.Net;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Sample.AzureFunction
{
    public class CreateOrderFunctions
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public CreateOrderFunctions(ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider =
                sendEndpointProvider ?? throw new ArgumentNullException(nameof(sendEndpointProvider));
        }

        [Function("CreateOrder")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("CreateOrder");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:submit-order"));

            await endpoint.Send<SubmitOrder>(new
            {
                OrderId = Guid.NewGuid(),
                OrderNumber = "123"
            });

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            await response.WriteStringAsync("Welcome to Azure Functions!");

            return response;
        }
    }
}
using System.Threading.Tasks;
using MassTransit.Transports;
using Microsoft.Azure.Functions.Worker;
using Sample.AzureFunction.Sagas;

namespace Sample.AzureFunction
{
    public class SubmitOrderFunctions
    {
        private const string SubmitOrderStateQueueName = "submit-order-state";
        private readonly IReceiveEndpointDispatcher<SubmitOrderState> _dispatcher;

        public SubmitOrderFunctions(IReceiveEndpointDispatcher<SubmitOrderState> dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [Function("SubmitOrderState")]
        public Task SubmitOrderStateAsync([ServiceBusTrigger(SubmitOrderStateQueueName, Connection = "ServiceBusConnection", IsSessionsEnabled = true)]
            byte[] body, FunctionContext context)
        {
            return _dispatcher.Dispatch(body, context);
        }
    }
}
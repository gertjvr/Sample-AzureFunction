using System.Threading.Tasks;
using MassTransit.Transports;
using Microsoft.Azure.Functions.Worker;
using Sample.AzureFunction.Sagas;

namespace Sample.AzureFunction
{
    public class SubmitOrderFunctions
    {
        const string SubmitOrderQueueName = "submit-order";
        readonly IReceiveEndpointDispatcher<SubmitOrderStateMachine> _dispatcher;

        public SubmitOrderFunctions(IReceiveEndpointDispatcher<SubmitOrderStateMachine> dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [Function("SubmitOrder")]
        public Task SubmitOrderAsync([ServiceBusTrigger(SubmitOrderQueueName, Connection = "ServiceBusConnection")]
            byte[] body, FunctionContext context)
        {
            return _dispatcher.Dispatch(context, body);
        }
    }
}
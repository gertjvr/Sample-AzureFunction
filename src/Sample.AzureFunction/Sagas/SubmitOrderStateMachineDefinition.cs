using GreenPipes;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.Definition;

namespace Sample.AzureFunction.Sagas
{
    public class SubmitOrderStateMachineDefinition : SagaDefinition<SumbitOrderState>
    {
        public SubmitOrderStateMachineDefinition()
        {
            EndpointName = "submit-order";
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<SumbitOrderState> sagaConfigurator)
        {
            // var ep = (IServiceBusReceiveEndpointConfigurator) endpointConfigurator;
            // ep.RequiresSession = true;
            
            //endpointConfigurator.UseMessageRetry(r => r.Interval(5, 1000));
        }
    }
}
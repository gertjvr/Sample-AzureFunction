using GreenPipes;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.Definition;

namespace Sample.Components.Sagas
{
    public class SubmitOrderStateMachineDefinition : SagaDefinition<SubmitOrderState>
    {
        public SubmitOrderStateMachineDefinition()
        {
            EndpointName = "submit-order-state";
        }
    
        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<SubmitOrderState> sagaConfigurator)
        {
            var ep = (IServiceBusReceiveEndpointConfigurator) endpointConfigurator;
            ep.RequiresSession = true;
            
            endpointConfigurator.UseMessageRetry(r => r.Interval(5, 1000));
        }
    }
}
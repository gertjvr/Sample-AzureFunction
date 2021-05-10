using System;
using Automatonymous;
using MassTransit;
using MassTransit.EventHubIntegration;

namespace Sample.AzureFunction.Sagas
{
    public class SubmitOrderStateMachine : MassTransitStateMachine<SumbitOrderState>
    {
        public SubmitOrderStateMachine(IEventHubProducerProvider producerProvider)
        {
            Event(() => SubmitOrder, configurator =>
            {
                configurator.CorrelateById(context => context.Message.OrderId);
            });
            
            InstanceState(x => x.CurrentState, Submitted, Accepted, Faulted);
            
            Initially(When(SubmitOrder)
                .ThenAsync(async context =>
                {
                    var producer = await producerProvider.GetProducer(AuditOrderFunctions.AuditOrderEventHubName);

                    await producer.Produce<OrderReceived>(new
                    {
                        context.Data.OrderNumber,
                        Timestamp = DateTime.UtcNow
                    });
                })
                .PublishAsync(context => context.Init<OrderReceived>(new
                {
                    context.Data.OrderNumber,
                    Timestamp = DateTime.UtcNow
                }))
                .RespondAsync(context => context.Init<OrderAccepted>(new
                {
                    context.Data.OrderNumber,
                }))
                .TransitionTo(Accepted)
            );
        }
        
        public Event<SubmitOrder> SubmitOrder { get; }
        
        public State Submitted { get; }
        public State Accepted { get; }
        public State Faulted { get; }
    }
}
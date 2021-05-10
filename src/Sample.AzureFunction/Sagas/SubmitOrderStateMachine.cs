using System;
using Automatonymous;
using MassTransit;
using MassTransit.EventHubIntegration;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.AzureFunction.Sagas
{
    public class SubmitOrderStateMachine : MassTransitStateMachine<SumbitOrderState>
    {
        public SubmitOrderStateMachine()
        {
            Event(() => SubmitOrder, configurator =>
            {
                configurator.CorrelateById(context => context.Message.OrderId);
            });
            
            InstanceState(x => x.CurrentState, Submitted, Accepted, Faulted);
            
            Initially(When(SubmitOrder)
                .ThenAsync(async context =>
                {
                    context.TryGetPayload<IServiceProvider>(out var provider);
                    var producerProvider = provider.GetService<IEventHubProducerProvider>();
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
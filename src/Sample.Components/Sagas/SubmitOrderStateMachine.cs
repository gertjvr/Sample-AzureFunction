using System;
using Automatonymous;
using MassTransit;
using Sample.Contracts;

namespace Sample.Components.Sagas
{
    public class SubmitOrderStateMachine : MassTransitStateMachine<SubmitOrderState>
    {
        public SubmitOrderStateMachine()
        {
            Event(() => SubmitOrder, configurator =>
            {
                configurator.CorrelateById(context => context.Message.OrderId);
            });
            
            InstanceState(x => x.CurrentState, Submitted, Accepted, Faulted);
            
            Initially(When(SubmitOrder)
                .Then(x =>
                {
                    x.Instance.Timestamp = DateTime.UtcNow;
                })
                .Produce(context => "audit-order", context => context.Init<OrderReceived>(new
                {
                    context.Data.OrderNumber,
                    Timestamp = DateTime.UtcNow
                }))
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
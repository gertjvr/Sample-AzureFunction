 using System;
using Automatonymous;

namespace Sample.AzureFunction.Sagas
{
    public class SubmitOrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public int CurrentState { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
 using System;
using Automatonymous;

namespace Sample.AzureFunction.Sagas
{
    public class SumbitOrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public int CurrentState { get; set; }
    }
}
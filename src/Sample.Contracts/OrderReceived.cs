using System;

namespace Sample.Contracts
{
    public interface OrderReceived
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }

        string OrderNumber { get; }
    }
}

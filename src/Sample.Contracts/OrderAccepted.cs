using System;

namespace Sample.Contracts
{
    public interface OrderAccepted
    {
        Guid OrderId { get; }
        string OrderNumber { get; }
    }
}

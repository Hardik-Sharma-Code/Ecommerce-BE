using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Services.Implementations;

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways;
    }

    public IPaymentGateway GetGateway(PaymentGatewayType type) =>
        _gateways.FirstOrDefault(g => g.GatewayType == type)
            ?? throw new InvalidOperationException($"No gateway registered for {type}.");
}

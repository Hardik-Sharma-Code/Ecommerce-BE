using Ecommerce_BE.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class StubEmailService : IEmailService
{
    private readonly ILogger<StubEmailService> _logger;

    public StubEmailService(ILogger<StubEmailService> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string body, bool isHtml = true)
    {
        _logger.LogInformation("[EMAIL] To: {To} | Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task SendOrderConfirmationAsync(string to, string orderNumber, decimal totalAmount)
    {
        _logger.LogInformation("[EMAIL] Order confirmation to {To}: Order {OrderNumber}, Total {Total}", to, orderNumber, totalAmount);
        return Task.CompletedTask;
    }

    public Task SendOrderShippedAsync(string to, string orderNumber, string trackingNumber, string? trackingUrl)
    {
        _logger.LogInformation("[EMAIL] Order shipped to {To}: Order {OrderNumber}, Tracking {Tracking}", to, orderNumber, trackingNumber);
        return Task.CompletedTask;
    }
}

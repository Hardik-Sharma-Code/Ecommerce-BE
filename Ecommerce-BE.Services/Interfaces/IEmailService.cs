namespace Ecommerce_BE.Services.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, bool isHtml = true);
    Task SendOrderConfirmationAsync(string to, string orderNumber, decimal totalAmount);
    Task SendOrderShippedAsync(string to, string orderNumber, string trackingNumber, string? trackingUrl);
}

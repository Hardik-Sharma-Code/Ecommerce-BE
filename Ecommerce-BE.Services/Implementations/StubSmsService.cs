using Ecommerce_BE.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class StubSmsService : ISmsService
{
    private readonly ILogger<StubSmsService> _logger;

    public StubSmsService(ILogger<StubSmsService> logger) => _logger = logger;

    public Task SendAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("[SMS] To: {Phone} | Message: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }

    public Task SendOtpAsync(string phoneNumber, string otp)
    {
        _logger.LogInformation("[SMS] OTP to {Phone}: {Otp}", phoneNumber, otp);
        return Task.CompletedTask;
    }
}

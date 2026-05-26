namespace Ecommerce_BE.Services.Interfaces;

public interface ISmsService
{
    Task SendAsync(string phoneNumber, string message);
    Task SendOtpAsync(string phoneNumber, string otp);
}

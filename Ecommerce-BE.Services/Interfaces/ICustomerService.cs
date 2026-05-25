using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Customer;

namespace Ecommerce_BE.Services.Interfaces;

public interface ICustomerService
{
    Task<ApiResponse<CustomerProfileDto>> RegisterAsync(CustomerRegistrationDto dto);
    Task<ApiResponse<CustomerProfileDto>> GetProfileAsync(string userId);
    Task<ApiResponse<CustomerProfileDto>> UpdateProfileAsync(string userId, UpdateCustomerProfileDto dto);
}

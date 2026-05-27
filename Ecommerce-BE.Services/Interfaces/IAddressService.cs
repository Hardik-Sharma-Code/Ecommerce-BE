using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Address;

namespace Ecommerce_BE.Services.Interfaces;

public interface IAddressService
{
    Task<ApiResponse<IEnumerable<AddressDto>>> GetAllAsync(string userId);
    Task<ApiResponse<AddressDto>> GetByIdAsync(int id, string userId);
    Task<ApiResponse<AddressDto>> CreateAsync(string userId, CreateAddressDto dto);
    Task<ApiResponse<AddressDto>> UpdateAsync(int id, string userId, UpdateAddressDto dto);
    Task<ApiResponse> DeleteAsync(int id, string userId);
    Task<ApiResponse<AddressDto>> SetDefaultAsync(int id, string userId);
}

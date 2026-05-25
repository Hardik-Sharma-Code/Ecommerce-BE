using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Vendor;

namespace Ecommerce_BE.Services.Interfaces;

public interface IVendorService
{
    Task<ApiResponse<VendorProfileDto>> RegisterAsync(VendorRegistrationDto dto);
    Task<ApiResponse<VendorProfileDto>> GetProfileAsync(string userId);
    Task<ApiResponse<VendorProfileDto>> UpdateProfileAsync(string userId, UpdateVendorProfileDto dto);
    Task<ApiResponse> SubmitKycAsync(string userId, KycSubmissionDto dto);
    Task<ApiResponse<KycStatusDto>> GetKycStatusAsync(string userId);
}

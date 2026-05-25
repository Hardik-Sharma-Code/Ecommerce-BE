using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Auth;

namespace Ecommerce_BE.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto);
    Task<ApiResponse> LogoutAsync(string userId);
    Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto);
    Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto dto);
    Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequestDto dto);
    Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordRequestDto dto);
}

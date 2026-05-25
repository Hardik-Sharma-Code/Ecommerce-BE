using System.Security.Claims;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Auth;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        Microsoft.Extensions.Options.IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return ApiResponse<LoginResponseDto>.Fail("Invalid email or password.");

        if (!user.IsActive)
            return ApiResponse<LoginResponseDto>.Fail("Your account has been disabled. Please contact support.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        await _refreshTokenRepository.CreateAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        });

        _logger.LogInformation("User {Email} logged in successfully.", user.Email);

        return ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtService.GetAccessTokenExpiry(),
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles
        });
    }

    public async Task<ApiResponse> LogoutAsync(string userId)
    {
        await _refreshTokenRepository.RevokeAllForUserAsync(userId);
        _logger.LogInformation("User {UserId} logged out.", userId);
        return ApiResponse.Ok("Logged out successfully.");
    }

    public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(dto.AccessToken);
        if (principal is null)
            return ApiResponse<LoginResponseDto>.Fail("Invalid access token.");

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return ApiResponse<LoginResponseDto>.Fail("Invalid token claims.");

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken);
        if (storedToken is null || storedToken.IsRevoked || storedToken.ExpiresAt <= DateTime.UtcNow)
            return ApiResponse<LoginResponseDto>.Fail("Invalid or expired refresh token.");

        if (storedToken.UserId != userId)
            return ApiResponse<LoginResponseDto>.Fail("Token mismatch.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null || !user.IsActive)
            return ApiResponse<LoginResponseDto>.Fail("User not found or account disabled.");

        await _refreshTokenRepository.RevokeAsync(storedToken);

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        await _refreshTokenRepository.CreateAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        });

        return ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = _jwtService.GetAccessTokenExpiry(),
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles
        });
    }

    public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        // Always return success to prevent email enumeration
        if (user is null)
            return ApiResponse<string>.Ok("If that email exists, a reset link has been sent.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        _logger.LogInformation("Password reset token generated for {Email}.", dto.Email);

        // In production, send the token via email. Returning for dev/testing.
        return ApiResponse<string>.Ok(token, "Password reset token generated. Send this token to the user via email.");
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequestDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user is null)
            return ApiResponse.Fail("Invalid request.");

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            return ApiResponse.Fail("Password reset failed.", result.Errors.Select(e => e.Description));

        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id);
        _logger.LogInformation("Password reset for user {Email}.", dto.Email);
        return ApiResponse.Ok("Password has been reset successfully.");
    }

    public async Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordRequestDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return ApiResponse.Fail("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return ApiResponse.Fail("Password change failed.", result.Errors.Select(e => e.Description));

        await _refreshTokenRepository.RevokeAllForUserAsync(userId);
        _logger.LogInformation("Password changed for user {UserId}.", userId);
        return ApiResponse.Ok("Password changed successfully.");
    }
}

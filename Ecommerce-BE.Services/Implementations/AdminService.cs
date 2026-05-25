using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Admin;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        UserManager<ApplicationUser> userManager,
        IUserRepository userRepository,
        IVendorRepository vendorRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<AdminService> logger)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _vendorRepository = vendorRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<UserListDto>>> GetUsersAsync(int page, int pageSize, string? role = null)
    {
        IEnumerable<ApplicationUser> users;
        int totalCount;

        if (!string.IsNullOrEmpty(role))
        {
            (users, totalCount) = await _userRepository.GetByRoleAsync(role, page, pageSize);
        }
        else
        {
            (users, totalCount) = await _userRepository.GetAllAsync(page, pageSize);
        }

        var userDtos = new List<UserListDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(MapToDto(user, roles));
        }

        return ApiResponse<PagedResult<UserListDto>>.Ok(new PagedResult<UserListDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse> EnableDisableUserAsync(string userId, UserStatusUpdateDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return ApiResponse.Fail("User not found.");

        user.IsActive = dto.IsActive;
        await _userRepository.UpdateAsync(user);

        if (!dto.IsActive)
            await _refreshTokenRepository.RevokeAllForUserAsync(userId);

        _logger.LogInformation("User {UserId} {Status}.", userId, dto.IsActive ? "enabled" : "disabled");
        return ApiResponse.Ok($"User has been {(dto.IsActive ? "enabled" : "disabled")} successfully.");
    }

    public async Task<ApiResponse> DeleteUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return ApiResponse.Fail("User not found.");

        await _userRepository.DeleteAsync(user);
        _logger.LogInformation("User {UserId} deleted.", userId);
        return ApiResponse.Ok("User deleted successfully.");
    }

    public async Task<ApiResponse<PagedResult<UserListDto>>> GetCustomersAsync(int page, int pageSize) =>
        await GetUsersAsync(page, pageSize, Roles.Customer);

    public async Task<ApiResponse<PagedResult<UserListDto>>> GetVendorsAsync(int page, int pageSize) =>
        await GetUsersAsync(page, pageSize, Roles.Vendor);

    public async Task<ApiResponse> UpdateVendorKycStatusAsync(string vendorId, UpdateVendorKycStatusDto dto)
    {
        var profile = await _vendorRepository.GetByUserIdAsync(vendorId);
        if (profile is null)
            return ApiResponse.Fail("Vendor profile not found.");

        if (profile.KycStatus != KycStatus.Submitted)
            return ApiResponse.Fail("KYC can only be reviewed when status is 'Submitted'.");

        if (dto.Status == KycStatus.Rejected && string.IsNullOrWhiteSpace(dto.RejectionReason))
            return ApiResponse.Fail("Rejection reason is required when rejecting KYC.");

        profile.KycStatus = dto.Status;
        profile.KycRejectionReason = dto.Status == KycStatus.Rejected ? dto.RejectionReason : null;
        profile.KycReviewedAt = DateTime.UtcNow;
        await _vendorRepository.UpdateAsync(profile);

        _logger.LogInformation("KYC status updated to {Status} for vendor {VendorId}.", dto.Status, vendorId);
        return ApiResponse.Ok($"Vendor KYC status updated to {dto.Status}.");
    }

    private static UserListDto MapToDto(ApplicationUser user, IList<string> roles) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email!,
        IsActive = user.IsActive,
        Roles = roles,
        CreatedAt = user.CreatedAt
    };
}

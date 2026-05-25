using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Vendor;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class VendorService : IVendorService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly ILogger<VendorService> _logger;

    public VendorService(
        UserManager<ApplicationUser> userManager,
        IUserRepository userRepository,
        IVendorRepository vendorRepository,
        ILogger<VendorService> logger)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _vendorRepository = vendorRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<VendorProfileDto>> RegisterAsync(VendorRegistrationDto dto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser is not null)
            return ApiResponse<VendorProfileDto>.Fail("A user with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return ApiResponse<VendorProfileDto>.Fail("Registration failed.", createResult.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, Roles.Vendor);

        var profile = await _vendorRepository.CreateAsync(new VendorProfile
        {
            UserId = user.Id,
            BusinessName = dto.BusinessName,
            BusinessDescription = dto.BusinessDescription,
            BusinessPhone = dto.BusinessPhone,
            BusinessAddress = dto.BusinessAddress
        });

        _logger.LogInformation("Vendor registered: {Email}", dto.Email);
        return ApiResponse<VendorProfileDto>.Ok(MapToDto(user, profile), "Registration successful.");
    }

    public async Task<ApiResponse<VendorProfileDto>> GetProfileAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return ApiResponse<VendorProfileDto>.Fail("User not found.");

        var profile = await _vendorRepository.GetByUserIdAsync(userId);
        if (profile is null)
            return ApiResponse<VendorProfileDto>.Fail("Vendor profile not found.");

        return ApiResponse<VendorProfileDto>.Ok(MapToDto(user, profile));
    }

    public async Task<ApiResponse<VendorProfileDto>> UpdateProfileAsync(string userId, UpdateVendorProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return ApiResponse<VendorProfileDto>.Fail("User not found.");

        var profile = await _vendorRepository.GetByUserIdAsync(userId);
        if (profile is null)
            return ApiResponse<VendorProfileDto>.Fail("Vendor profile not found.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        await _userRepository.UpdateAsync(user);

        profile.BusinessName = dto.BusinessName;
        profile.BusinessDescription = dto.BusinessDescription;
        profile.BusinessPhone = dto.BusinessPhone;
        profile.BusinessAddress = dto.BusinessAddress;
        await _vendorRepository.UpdateAsync(profile);

        _logger.LogInformation("Vendor profile updated: {UserId}", userId);
        return ApiResponse<VendorProfileDto>.Ok(MapToDto(user, profile));
    }

    public async Task<ApiResponse> SubmitKycAsync(string userId, KycSubmissionDto dto)
    {
        var profile = await _vendorRepository.GetByUserIdAsync(userId);
        if (profile is null)
            return ApiResponse.Fail("Vendor profile not found.");

        if (profile.KycStatus == KycStatus.Approved)
            return ApiResponse.Fail("KYC has already been approved.");

        if (profile.KycStatus == KycStatus.Submitted)
            return ApiResponse.Fail("KYC is already under review.");

        profile.KycDocumentType = dto.DocumentType;
        profile.KycDocumentNumber = dto.DocumentNumber;
        profile.KycStatus = KycStatus.Submitted;
        profile.KycSubmittedAt = DateTime.UtcNow;
        profile.KycRejectionReason = null;
        await _vendorRepository.UpdateAsync(profile);

        _logger.LogInformation("KYC submitted for vendor: {UserId}", userId);
        return ApiResponse.Ok("KYC documents submitted for review.");
    }

    public async Task<ApiResponse<KycStatusDto>> GetKycStatusAsync(string userId)
    {
        var profile = await _vendorRepository.GetByUserIdAsync(userId);
        if (profile is null)
            return ApiResponse<KycStatusDto>.Fail("Vendor profile not found.");

        return ApiResponse<KycStatusDto>.Ok(new KycStatusDto
        {
            Status = profile.KycStatus,
            RejectionReason = profile.KycRejectionReason,
            SubmittedAt = profile.KycSubmittedAt,
            ReviewedAt = profile.KycReviewedAt
        });
    }

    private static VendorProfileDto MapToDto(ApplicationUser user, VendorProfile profile) => new()
    {
        UserId = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email!,
        BusinessName = profile.BusinessName,
        BusinessDescription = profile.BusinessDescription,
        BusinessAddress = profile.BusinessAddress,
        BusinessPhone = profile.BusinessPhone,
        KycStatus = profile.KycStatus
    };
}

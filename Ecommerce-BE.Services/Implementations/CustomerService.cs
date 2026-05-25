using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Customer;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        UserManager<ApplicationUser> userManager,
        IUserRepository userRepository,
        ICustomerRepository customerRepository,
        ILogger<CustomerService> logger)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<CustomerProfileDto>> RegisterAsync(CustomerRegistrationDto dto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser is not null)
            return ApiResponse<CustomerProfileDto>.Fail("A user with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return ApiResponse<CustomerProfileDto>.Fail("Registration failed.", createResult.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, Roles.Customer);

        var profile = await _customerRepository.CreateAsync(new CustomerProfile
        {
            UserId = user.Id,
            PhoneNumber = dto.PhoneNumber
        });

        _logger.LogInformation("Customer registered: {Email}", dto.Email);
        return ApiResponse<CustomerProfileDto>.Ok(MapToDto(user, profile), "Registration successful.");
    }

    public async Task<ApiResponse<CustomerProfileDto>> GetProfileAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return ApiResponse<CustomerProfileDto>.Fail("User not found.");

        var profile = await _customerRepository.GetByUserIdAsync(userId);
        return ApiResponse<CustomerProfileDto>.Ok(MapToDto(user, profile));
    }

    public async Task<ApiResponse<CustomerProfileDto>> UpdateProfileAsync(string userId, UpdateCustomerProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return ApiResponse<CustomerProfileDto>.Fail("User not found.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        await _userRepository.UpdateAsync(user);

        var profile = await _customerRepository.GetByUserIdAsync(userId);
        if (profile is null)
        {
            profile = await _customerRepository.CreateAsync(new CustomerProfile { UserId = userId });
        }

        profile.PhoneNumber = dto.PhoneNumber;
        profile.Address = dto.Address;
        profile.City = dto.City;
        profile.State = dto.State;
        profile.Country = dto.Country;
        profile.PostalCode = dto.PostalCode;
        await _customerRepository.UpdateAsync(profile);

        _logger.LogInformation("Customer profile updated: {UserId}", userId);
        return ApiResponse<CustomerProfileDto>.Ok(MapToDto(user, profile));
    }

    private static CustomerProfileDto MapToDto(ApplicationUser user, CustomerProfile? profile) => new()
    {
        UserId = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email!,
        PhoneNumber = profile?.PhoneNumber ?? user.PhoneNumber,
        Address = profile?.Address,
        City = profile?.City,
        State = profile?.State,
        Country = profile?.Country,
        PostalCode = profile?.PostalCode
    };
}

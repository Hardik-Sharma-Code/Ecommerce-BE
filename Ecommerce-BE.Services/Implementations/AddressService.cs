using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Address;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;
    private readonly ILogger<AddressService> _logger;

    public AddressService(IAddressRepository addressRepository, ILogger<AddressService> logger)
    {
        _addressRepository = addressRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<AddressDto>>> GetAllAsync(string userId)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(userId);
        return ApiResponse<IEnumerable<AddressDto>>.Ok(addresses.Select(MapToDto));
    }

    public async Task<ApiResponse<AddressDto>> GetByIdAsync(int id, string userId)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        if (address is null || address.UserId != userId)
            return ApiResponse<AddressDto>.Fail("Address not found.");

        return ApiResponse<AddressDto>.Ok(MapToDto(address));
    }

    public async Task<ApiResponse<AddressDto>> CreateAsync(string userId, CreateAddressDto dto)
    {
        if (dto.SetAsDefault)
            await _addressRepository.ClearDefaultAsync(userId);

        var hasDefault = await _addressRepository.HasDefaultAsync(userId);

        var address = new CustomerAddress
        {
            UserId = userId,
            Label = dto.Label,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            IsDefault = dto.SetAsDefault || !hasDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _addressRepository.AddAsync(address);
        await _addressRepository.SaveAsync();
        return ApiResponse<AddressDto>.Ok(MapToDto(address), "Address created.");
    }

    public async Task<ApiResponse<AddressDto>> UpdateAsync(int id, string userId, UpdateAddressDto dto)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        if (address is null || address.UserId != userId)
            return ApiResponse<AddressDto>.Fail("Address not found.");

        if (dto.SetAsDefault == true)
            await _addressRepository.ClearDefaultAsync(userId);

        if (dto.Label is not null) address.Label = dto.Label;
        if (dto.FirstName is not null) address.FirstName = dto.FirstName;
        if (dto.LastName is not null) address.LastName = dto.LastName;
        if (dto.PhoneNumber is not null) address.PhoneNumber = dto.PhoneNumber;
        if (dto.AddressLine1 is not null) address.AddressLine1 = dto.AddressLine1;
        if (dto.AddressLine2 is not null) address.AddressLine2 = dto.AddressLine2;
        if (dto.City is not null) address.City = dto.City;
        if (dto.State is not null) address.State = dto.State;
        if (dto.PostalCode is not null) address.PostalCode = dto.PostalCode;
        if (dto.Country is not null) address.Country = dto.Country;
        if (dto.SetAsDefault.HasValue) address.IsDefault = dto.SetAsDefault.Value;
        address.UpdatedAt = DateTime.UtcNow;

        await _addressRepository.UpdateAsync(address);
        await _addressRepository.SaveAsync();
        return ApiResponse<AddressDto>.Ok(MapToDto(address), "Address updated.");
    }

    public async Task<ApiResponse> DeleteAsync(int id, string userId)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        if (address is null || address.UserId != userId)
            return ApiResponse.Fail("Address not found.");

        await _addressRepository.DeleteAsync(address);
        await _addressRepository.SaveAsync();

        if (address.IsDefault)
        {
            var remaining = (await _addressRepository.GetByUserIdAsync(userId)).FirstOrDefault();
            if (remaining is not null)
            {
                remaining.IsDefault = true;
                await _addressRepository.UpdateAsync(remaining);
                await _addressRepository.SaveAsync();
            }
        }

        return ApiResponse.Ok("Address deleted.");
    }

    public async Task<ApiResponse<AddressDto>> SetDefaultAsync(int id, string userId)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        if (address is null || address.UserId != userId)
            return ApiResponse<AddressDto>.Fail("Address not found.");

        await _addressRepository.ClearDefaultAsync(userId);
        address.IsDefault = true;
        address.UpdatedAt = DateTime.UtcNow;
        await _addressRepository.UpdateAsync(address);
        await _addressRepository.SaveAsync();

        return ApiResponse<AddressDto>.Ok(MapToDto(address), "Default address updated.");
    }

    private static AddressDto MapToDto(CustomerAddress a) => new()
    {
        Id = a.Id,
        Label = a.Label,
        FirstName = a.FirstName,
        LastName = a.LastName,
        PhoneNumber = a.PhoneNumber,
        AddressLine1 = a.AddressLine1,
        AddressLine2 = a.AddressLine2,
        City = a.City,
        State = a.State,
        PostalCode = a.PostalCode,
        Country = a.Country,
        IsDefault = a.IsDefault
    };
}

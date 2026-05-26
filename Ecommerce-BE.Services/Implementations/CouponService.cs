using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Coupon;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class CouponService : ICouponService
{
    private readonly ICouponRepository _couponRepository;
    private readonly ILogger<CouponService> _logger;

    public CouponService(ICouponRepository couponRepository, ILogger<CouponService> logger)
    {
        _couponRepository = couponRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<CouponValidationResultDto>> ValidateAsync(ApplyCouponDto dto)
    {
        var coupon = await _couponRepository.GetByCodeAsync(dto.Code);
        if (coupon is null)
            return ApiResponse<CouponValidationResultDto>.Ok(Invalid("Coupon code not found."));

        if (!coupon.IsActive)
            return ApiResponse<CouponValidationResultDto>.Ok(Invalid("This coupon is inactive."));

        var now = DateTime.UtcNow;
        if (now < coupon.ValidFrom)
            return ApiResponse<CouponValidationResultDto>.Ok(Invalid("This coupon is not yet valid."));

        if (now > coupon.ValidTo)
            return ApiResponse<CouponValidationResultDto>.Ok(Invalid("This coupon has expired."));

        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
            return ApiResponse<CouponValidationResultDto>.Ok(Invalid("This coupon has reached its usage limit."));

        if (coupon.MinOrderAmount.HasValue && dto.OrderAmount < coupon.MinOrderAmount.Value)
            return ApiResponse<CouponValidationResultDto>.Ok(
                Invalid($"Minimum order amount of {coupon.MinOrderAmount:C} required."));

        var discount = coupon.DiscountType == DiscountType.Percentage
            ? dto.OrderAmount * (coupon.DiscountValue / 100m)
            : coupon.DiscountValue;

        if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
            discount = coupon.MaxDiscountAmount.Value;

        discount = Math.Min(discount, dto.OrderAmount);

        _logger.LogInformation("Coupon {Code} validated: discount={Discount}", coupon.Code, discount);

        return ApiResponse<CouponValidationResultDto>.Ok(new CouponValidationResultDto
        {
            IsValid = true,
            Message = "Coupon applied successfully.",
            DiscountAmount = Math.Round(discount, 2),
            FinalAmount = Math.Round(dto.OrderAmount - discount, 2)
        });
    }

    public async Task<ApiResponse<CouponDto>> GetByCodeAsync(string code)
    {
        var coupon = await _couponRepository.GetByCodeAsync(code);
        if (coupon is null)
            return ApiResponse<CouponDto>.Fail("Coupon not found.");

        return ApiResponse<CouponDto>.Ok(MapToDto(coupon));
    }

    public async Task<ApiResponse<PagedResult<CouponDto>>> GetAllAsync(int page, int pageSize)
    {
        var (coupons, total) = await _couponRepository.GetAllAsync(page, pageSize);
        return ApiResponse<PagedResult<CouponDto>>.Ok(new PagedResult<CouponDto>
        {
            Items = coupons.Select(MapToDto),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<CouponDto>> CreateAsync(CreateCouponDto dto)
    {
        if (dto.ValidTo <= dto.ValidFrom)
            return ApiResponse<CouponDto>.Fail("ValidTo must be after ValidFrom.");

        var code = dto.Code.ToUpper();
        if (await _couponRepository.CodeExistsAsync(code))
            return ApiResponse<CouponDto>.Fail($"Coupon code '{code}' already exists.");

        var coupon = new Coupon
        {
            Code = code,
            Description = dto.Description,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            MinOrderAmount = dto.MinOrderAmount,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            UsageLimit = dto.UsageLimit,
            UsageCount = 0,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _couponRepository.AddAsync(coupon);
        await _couponRepository.SaveAsync();
        return ApiResponse<CouponDto>.Ok(MapToDto(coupon), "Coupon created.");
    }

    public async Task<ApiResponse<CouponDto>> UpdateAsync(int id, UpdateCouponDto dto)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);
        if (coupon is null)
            return ApiResponse<CouponDto>.Fail("Coupon not found.");

        if (dto.Description is not null) coupon.Description = dto.Description;
        if (dto.DiscountType.HasValue) coupon.DiscountType = dto.DiscountType.Value;
        if (dto.DiscountValue.HasValue) coupon.DiscountValue = dto.DiscountValue.Value;
        if (dto.MinOrderAmount.HasValue) coupon.MinOrderAmount = dto.MinOrderAmount;
        if (dto.MaxDiscountAmount.HasValue) coupon.MaxDiscountAmount = dto.MaxDiscountAmount;
        if (dto.UsageLimit.HasValue) coupon.UsageLimit = dto.UsageLimit;
        if (dto.ValidFrom.HasValue) coupon.ValidFrom = dto.ValidFrom.Value;
        if (dto.ValidTo.HasValue) coupon.ValidTo = dto.ValidTo.Value;
        if (dto.IsActive.HasValue) coupon.IsActive = dto.IsActive.Value;
        coupon.UpdatedAt = DateTime.UtcNow;

        if (coupon.ValidTo <= coupon.ValidFrom)
            return ApiResponse<CouponDto>.Fail("ValidTo must be after ValidFrom.");

        await _couponRepository.UpdateAsync(coupon);
        await _couponRepository.SaveAsync();
        return ApiResponse<CouponDto>.Ok(MapToDto(coupon), "Coupon updated.");
    }

    public async Task<ApiResponse> DeleteAsync(int id)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);
        if (coupon is null)
            return ApiResponse.Fail("Coupon not found.");

        await _couponRepository.DeleteAsync(coupon);
        await _couponRepository.SaveAsync();
        return ApiResponse.Ok("Coupon deleted.");
    }

    private static CouponValidationResultDto Invalid(string message) => new()
    {
        IsValid = false,
        Message = message,
        DiscountAmount = 0,
        FinalAmount = 0
    };

    private static CouponDto MapToDto(Coupon c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        Description = c.Description,
        DiscountType = c.DiscountType,
        DiscountValue = c.DiscountValue,
        MinOrderAmount = c.MinOrderAmount,
        MaxDiscountAmount = c.MaxDiscountAmount,
        UsageLimit = c.UsageLimit,
        UsageCount = c.UsageCount,
        ValidFrom = c.ValidFrom,
        ValidTo = c.ValidTo,
        IsActive = c.IsActive
    };
}

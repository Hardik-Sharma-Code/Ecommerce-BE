using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Cart;
using Ecommerce_BE.Shared.Kernel.DTOs.Wishlist;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICartService _cartService;
    private readonly ILogger<WishlistService> _logger;

    public WishlistService(
        IWishlistRepository wishlistRepository,
        IProductRepository productRepository,
        ICartService cartService,
        ILogger<WishlistService> logger)
    {
        _wishlistRepository = wishlistRepository;
        _productRepository = productRepository;
        _cartService = cartService;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<WishlistItemDto>>> GetWishlistAsync(string userId)
    {
        var items = await _wishlistRepository.GetByUserIdAsync(userId);
        return ApiResponse<IEnumerable<WishlistItemDto>>.Ok(items.Select(MapToDto));
    }

    public async Task<ApiResponse<WishlistItemDto>> AddToWishlistAsync(string userId, AddToWishlistDto dto)
    {
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product is null)
            return ApiResponse<WishlistItemDto>.Fail("Product not found.");

        var already = await _wishlistRepository.IsInWishlistAsync(userId, dto.ProductId);
        if (already)
            return ApiResponse<WishlistItemDto>.Fail("Product is already in your wishlist.");

        var item = new WishlistItem
        {
            UserId = userId,
            ProductId = dto.ProductId,
            AddedAt = DateTime.UtcNow
        };

        await _wishlistRepository.AddAsync(item);
        await _wishlistRepository.SaveAsync();

        var saved = await _wishlistRepository.GetItemAsync(userId, dto.ProductId);
        return ApiResponse<WishlistItemDto>.Ok(MapToDto(saved!), "Added to wishlist.");
    }

    public async Task<ApiResponse> RemoveFromWishlistAsync(string userId, int itemId)
    {
        var item = await _wishlistRepository.GetItemByIdAsync(itemId);
        if (item is null || item.UserId != userId)
            return ApiResponse.Fail("Wishlist item not found.");

        await _wishlistRepository.RemoveAsync(item);
        await _wishlistRepository.SaveAsync();
        return ApiResponse.Ok("Removed from wishlist.");
    }

    public async Task<ApiResponse> ClearWishlistAsync(string userId)
    {
        await _wishlistRepository.ClearAsync(userId);
        await _wishlistRepository.SaveAsync();
        return ApiResponse.Ok("Wishlist cleared.");
    }

    public async Task<ApiResponse<bool>> IsInWishlistAsync(string userId, int productId)
    {
        var result = await _wishlistRepository.IsInWishlistAsync(userId, productId);
        return ApiResponse<bool>.Ok(result);
    }

    public async Task<ApiResponse<CartDto>> MoveToCartAsync(string userId, int itemId)
    {
        var item = await _wishlistRepository.GetItemByIdAsync(itemId);
        if (item is null || item.UserId != userId)
            return ApiResponse<CartDto>.Fail("Wishlist item not found.");

        var addResult = await _cartService.AddItemAsync(userId, new Shared.Kernel.DTOs.Cart.AddToCartDto
        {
            ProductId = item.ProductId,
            Quantity = 1
        });

        if (!addResult.Success)
            return ApiResponse<CartDto>.Fail(addResult.Message);

        await _wishlistRepository.RemoveAsync(item);
        await _wishlistRepository.SaveAsync();

        return ApiResponse<CartDto>.Ok(addResult.Data!, "Moved to cart.");
    }

    private static WishlistItemDto MapToDto(WishlistItem w) => new()
    {
        ItemId = w.Id,
        ProductId = w.ProductId,
        ProductName = w.Product?.Name ?? string.Empty,
        ProductSlug = w.Product?.Slug ?? string.Empty,
        PrimaryImageUrl = w.Product?.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                       ?? w.Product?.Images.FirstOrDefault()?.ImageUrl,
        Price = w.Product?.Price ?? 0,
        CompareAtPrice = w.Product?.CompareAtPrice,
        IsInStock = (w.Product?.StockQuantity ?? 0) > 0,
        AddedAt = w.AddedAt
    };
}

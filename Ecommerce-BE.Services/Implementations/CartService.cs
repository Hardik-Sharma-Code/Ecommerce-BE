using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Cart;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CartService> _logger;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        ILogger<CartService> logger)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<CartDto>> GetCartAsync(string userId)
    {
        var cart = await _cartRepository.GetOrCreateAsync(userId);
        return ApiResponse<CartDto>.Ok(MapToDto(cart));
    }

    public async Task<ApiResponse<CartDto>> AddItemAsync(string userId, AddToCartDto dto)
    {
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product is null)
            return ApiResponse<CartDto>.Fail("Product not found.");

        if (!product.IsActive)
            return ApiResponse<CartDto>.Fail("Product is not available.");

        if (product.StockQuantity < dto.Quantity)
            return ApiResponse<CartDto>.Fail($"Only {product.StockQuantity} units in stock.");

        var cart = await _cartRepository.GetOrCreateAsync(userId);
        var existing = await _cartRepository.GetItemAsync(cart.Id, dto.ProductId);

        if (existing is not null)
        {
            var newQty = existing.Quantity + dto.Quantity;
            if (product.StockQuantity < newQty)
                return ApiResponse<CartDto>.Fail($"Only {product.StockQuantity} units available. Already {existing.Quantity} in cart.");

            existing.Quantity = newQty;
            await _cartRepository.UpdateItemAsync(existing);
        }
        else
        {
            var item = new CartItem
            {
                CartId = cart.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                AddedAt = DateTime.UtcNow
            };
            await _cartRepository.AddItemAsync(item);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.SaveAsync();

        var updated = await _cartRepository.GetOrCreateAsync(userId);
        return ApiResponse<CartDto>.Ok(MapToDto(updated), "Item added to cart.");
    }

    public async Task<ApiResponse<CartDto>> UpdateItemAsync(string userId, int itemId, UpdateCartItemDto dto)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
            return ApiResponse<CartDto>.Fail("Cart not found.");

        var item = await _cartRepository.GetItemByIdAsync(itemId);
        if (item is null || item.CartId != cart.Id)
            return ApiResponse<CartDto>.Fail("Cart item not found.");

        if (dto.Quantity == 0)
        {
            await _cartRepository.RemoveItemAsync(item);
        }
        else
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product is not null && product.StockQuantity < dto.Quantity)
                return ApiResponse<CartDto>.Fail($"Only {product.StockQuantity} units in stock.");

            item.Quantity = dto.Quantity;
            await _cartRepository.UpdateItemAsync(item);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.SaveAsync();

        var updated = await _cartRepository.GetOrCreateAsync(userId);
        return ApiResponse<CartDto>.Ok(MapToDto(updated), "Cart updated.");
    }

    public async Task<ApiResponse<CartDto>> RemoveItemAsync(string userId, int itemId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
            return ApiResponse<CartDto>.Fail("Cart not found.");

        var item = await _cartRepository.GetItemByIdAsync(itemId);
        if (item is null || item.CartId != cart.Id)
            return ApiResponse<CartDto>.Fail("Cart item not found.");

        await _cartRepository.RemoveItemAsync(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.SaveAsync();

        var updated = await _cartRepository.GetOrCreateAsync(userId);
        return ApiResponse<CartDto>.Ok(MapToDto(updated), "Item removed from cart.");
    }

    public async Task<ApiResponse> ClearCartAsync(string userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
            return ApiResponse.Ok("Cart is already empty.");

        await _cartRepository.ClearItemsAsync(cart.Id);
        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.SaveAsync();
        return ApiResponse.Ok("Cart cleared.");
    }

    public async Task<ApiResponse<int>> GetItemCountAsync(string userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        var count = cart?.Items.Sum(i => i.Quantity) ?? 0;
        return ApiResponse<int>.Ok(count);
    }

    private static CartDto MapToDto(Cart cart)
    {
        var items = cart.Items.Select(i => new CartItemDto
        {
            ItemId = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name ?? string.Empty,
            ProductSlug = i.Product?.Slug ?? string.Empty,
            PrimaryImageUrl = i.Product?.Images.FirstOrDefault(img => img.IsPrimary)?.ImageUrl
                           ?? i.Product?.Images.FirstOrDefault()?.ImageUrl,
            Price = i.Product?.Price ?? 0,
            CompareAtPrice = i.Product?.CompareAtPrice,
            Quantity = i.Quantity,
            Subtotal = (i.Product?.Price ?? 0) * i.Quantity,
            IsInStock = (i.Product?.StockQuantity ?? 0) > 0,
            StockQuantity = i.Product?.StockQuantity ?? 0
        }).ToList();

        return new CartDto
        {
            Items = items,
            ItemCount = items.Sum(i => i.Quantity),
            Subtotal = items.Sum(i => i.Subtotal)
        };
    }
}

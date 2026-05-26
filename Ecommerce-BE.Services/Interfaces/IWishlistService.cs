using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Cart;
using Ecommerce_BE.Shared.Kernel.DTOs.Wishlist;

namespace Ecommerce_BE.Services.Interfaces;

public interface IWishlistService
{
    Task<ApiResponse<IEnumerable<WishlistItemDto>>> GetWishlistAsync(string userId);
    Task<ApiResponse<WishlistItemDto>> AddToWishlistAsync(string userId, AddToWishlistDto dto);
    Task<ApiResponse> RemoveFromWishlistAsync(string userId, int itemId);
    Task<ApiResponse> ClearWishlistAsync(string userId);
    Task<ApiResponse<bool>> IsInWishlistAsync(string userId, int productId);
    Task<ApiResponse<CartDto>> MoveToCartAsync(string userId, int itemId);
}

using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Cart;

namespace Ecommerce_BE.Services.Interfaces;

public interface ICartService
{
    Task<ApiResponse<CartDto>> GetCartAsync(string userId);
    Task<ApiResponse<CartDto>> AddItemAsync(string userId, AddToCartDto dto);
    Task<ApiResponse<CartDto>> UpdateItemAsync(string userId, int itemId, UpdateCartItemDto dto);
    Task<ApiResponse<CartDto>> RemoveItemAsync(string userId, int itemId);
    Task<ApiResponse> ClearCartAsync(string userId);
    Task<ApiResponse<int>> GetItemCountAsync(string userId);
}

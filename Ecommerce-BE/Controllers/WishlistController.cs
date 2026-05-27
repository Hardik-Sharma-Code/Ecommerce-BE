using System.Security.Claims;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Cart;
using Ecommerce_BE.Shared.Kernel.DTOs.Wishlist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    /// <summary>Get all items in the user's wishlist</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WishlistItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWishlist()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _wishlistService.GetWishlistAsync(userId);
        return Ok(result);
    }

    /// <summary>Add a product to the wishlist</summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<WishlistItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _wishlistService.AddToWishlistAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Remove a specific item from the wishlist</summary>
    [HttpDelete("items/{itemId:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveFromWishlist(int itemId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _wishlistService.RemoveFromWishlistAsync(userId, itemId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Clear the entire wishlist</summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearWishlist()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _wishlistService.ClearWishlistAsync(userId);
        return Ok(result);
    }

    /// <summary>Check whether a product is in the wishlist</summary>
    [HttpGet("check/{productId:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsInWishlist(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _wishlistService.IsInWishlistAsync(userId, productId);
        return Ok(result);
    }

    /// <summary>Move a wishlist item directly into the cart</summary>
    [HttpPost("items/{itemId:int}/move-to-cart")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MoveToCart(int itemId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _wishlistService.MoveToCartAsync(userId, itemId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

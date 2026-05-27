using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Wishlist;

public class AddToWishlistDto
{
    [Required]
    public int ProductId { get; set; }
}

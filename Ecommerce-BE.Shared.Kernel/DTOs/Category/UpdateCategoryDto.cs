using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Category;

public class UpdateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public int? ParentCategoryId { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0;
}

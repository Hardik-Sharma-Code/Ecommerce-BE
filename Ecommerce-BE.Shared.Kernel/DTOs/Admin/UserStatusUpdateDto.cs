using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Admin;

public class UserStatusUpdateDto
{
    [Required]
    public bool IsActive { get; set; }
}

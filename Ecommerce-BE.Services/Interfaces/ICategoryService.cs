using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Category;

namespace Ecommerce_BE.Services.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllAsync(bool activeOnly = true);
    Task<ApiResponse<IEnumerable<CategoryTreeDto>>> GetTreeAsync();
    Task<ApiResponse<CategoryDto>> GetByIdAsync(int id);
    Task<ApiResponse<CategoryDto>> GetBySlugAsync(string slug);
    Task<ApiResponse<IEnumerable<CategoryDto>>> GetSubCategoriesAsync(int parentId);
    Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryDto dto);
    Task<ApiResponse<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<ApiResponse> DeleteAsync(int id);
}

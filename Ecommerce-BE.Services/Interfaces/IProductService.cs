using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Product;

namespace Ecommerce_BE.Services.Interfaces;

public interface IProductService
{
    Task<ApiResponse<PagedResult<ProductListDto>>> GetAllAsync(int page, int pageSize);
    Task<ApiResponse<ProductDto>> GetByIdAsync(int id);
    Task<ApiResponse<ProductDto>> GetBySlugAsync(string slug);
    Task<ApiResponse<PagedResult<ProductListDto>>> GetByCategoryAsync(int categoryId, int page, int pageSize);
    Task<ApiResponse<PagedResult<ProductListDto>>> GetByVendorAsync(string vendorId, int page, int pageSize);
    Task<ApiResponse<ProductDto>> CreateAsync(string vendorId, CreateProductDto dto);
    Task<ApiResponse<ProductDto>> UpdateAsync(int id, string userId, string userRole, UpdateProductDto dto);
    Task<ApiResponse> DeleteAsync(int id, string userId, string userRole);
    Task<ApiResponse<StockInfoDto>> GetStockAsync(int id);
    Task<ApiResponse<StockInfoDto>> UpdateStockAsync(int id, string userId, string userRole, UpdateStockDto dto);
}

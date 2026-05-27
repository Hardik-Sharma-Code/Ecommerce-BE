using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Product;
using Ecommerce_BE.Shared.Kernel.DTOs.Search;

namespace Ecommerce_BE.Services.Interfaces;

public interface ISearchService
{
    Task<ApiResponse<PagedResult<ProductListDto>>> SearchAsync(ProductSearchRequestDto request);
    Task<ApiResponse<IEnumerable<string>>> GetSuggestionsAsync(string query, int limit = 10);
}

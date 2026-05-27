using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Product;
using Ecommerce_BE.Shared.Kernel.DTOs.Search;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Search and filter products.
    /// Supports: text query, category, vendor, price range, in-stock, featured.
    /// Sort options: newest | price-asc | price-desc | name-asc | name-desc | featured
    /// </summary>
    [HttpGet("products")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchRequestDto request)
    {
        var result = await _searchService.SearchAsync(request);
        return Ok(result);
    }

    /// <summary>Get product name suggestions for autocomplete (min 2 characters)</summary>
    [HttpGet("suggestions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuggestions(
        [FromQuery] string query,
        [FromQuery] int limit = 10)
    {
        var result = await _searchService.GetSuggestionsAsync(query, limit);
        return Ok(result);
    }
}

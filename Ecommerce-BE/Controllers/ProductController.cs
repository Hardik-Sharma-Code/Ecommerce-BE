using System.Security.Claims;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Get all active products (paginated)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _productService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Get a product by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get a product by slug</summary>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await _productService.GetBySlugAsync(slug);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get products by category</summary>
    [HttpGet("category/{categoryId:int}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(
        int categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _productService.GetByCategoryAsync(categoryId, page, pageSize);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get products by vendor</summary>
    [HttpGet("vendor/{vendorId}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByVendor(
        string vendorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _productService.GetByVendorAsync(vendorId, page, pageSize);
        return Ok(result);
    }

    /// <summary>Create a new product [Vendor / Admin]</summary>
    [HttpPost]
    [Authorize(Roles = $"{Roles.Vendor},{Roles.Admin}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var vendorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _productService.CreateAsync(vendorId, dto);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    /// <summary>Update a product [Vendor (own) / Admin (any)]</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Vendor},{Roles.Admin}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var userRole = User.IsInRole(Roles.Admin) ? Roles.Admin : Roles.Vendor;
        var result = await _productService.UpdateAsync(id, userId, userRole, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete a product [Vendor (own) / Admin (any)]</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{Roles.Vendor},{Roles.Admin}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var userRole = User.IsInRole(Roles.Admin) ? Roles.Admin : Roles.Vendor;
        var result = await _productService.DeleteAsync(id, userId, userRole);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get stock information for a product [Vendor / Admin]</summary>
    [HttpGet("{id:int}/stock")]
    [Authorize(Roles = $"{Roles.Vendor},{Roles.Admin}")]
    [ProducesResponseType(typeof(ApiResponse<StockInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStock(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var userRole = User.IsInRole(Roles.Admin) ? Roles.Admin : Roles.Vendor;
        var result = await _productService.GetStockAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Update stock quantity [Vendor (own) / Admin (any)]</summary>
    [HttpPatch("{id:int}/stock")]
    [Authorize(Roles = $"{Roles.Vendor},{Roles.Admin}")]
    [ProducesResponseType(typeof(ApiResponse<StockInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var userRole = User.IsInRole(Roles.Admin) ? Roles.Admin : Roles.Vendor;
        var result = await _productService.UpdateStockAsync(id, userId, userRole, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

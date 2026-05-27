using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Category;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllAsync(bool activeOnly = true)
    {
        var categories = await _categoryRepository.GetAllAsync(activeOnly);
        var dtos = new List<CategoryDto>();

        foreach (var category in categories)
        {
            var count = await _productRepository.GetCountByCategoryAsync(category.Id);
            dtos.Add(MapToDto(category, count));
        }

        return ApiResponse<IEnumerable<CategoryDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<CategoryTreeDto>>> GetTreeAsync()
    {
        var roots = await _categoryRepository.GetRootCategoriesAsync();
        var tree = roots.Select(MapToTree);
        return ApiResponse<IEnumerable<CategoryTreeDto>>.Ok(tree);
    }

    public async Task<ApiResponse<CategoryDto>> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id, includeSubCategories: true);
        if (category is null)
            return ApiResponse<CategoryDto>.Fail("Category not found.");

        var count = await _productRepository.GetCountByCategoryAsync(id);
        return ApiResponse<CategoryDto>.Ok(MapToDto(category, count));
    }

    public async Task<ApiResponse<CategoryDto>> GetBySlugAsync(string slug)
    {
        var category = await _categoryRepository.GetBySlugAsync(slug);
        if (category is null)
            return ApiResponse<CategoryDto>.Fail("Category not found.");

        var count = await _productRepository.GetCountByCategoryAsync(category.Id);
        return ApiResponse<CategoryDto>.Ok(MapToDto(category, count));
    }

    public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetSubCategoriesAsync(int parentId)
    {
        var parent = await _categoryRepository.GetByIdAsync(parentId);
        if (parent is null)
            return ApiResponse<IEnumerable<CategoryDto>>.Fail("Parent category not found.");

        var subCategories = await _categoryRepository.GetSubCategoriesAsync(parentId);
        var dtos = new List<CategoryDto>();

        foreach (var sub in subCategories)
        {
            var count = await _productRepository.GetCountByCategoryAsync(sub.Id);
            dtos.Add(MapToDto(sub, count));
        }

        return ApiResponse<IEnumerable<CategoryDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryDto dto)
    {
        if (dto.ParentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(dto.ParentCategoryId.Value);
            if (parent is null)
                return ApiResponse<CategoryDto>.Fail("Parent category not found.");
        }

        var slug = await GenerateUniqueSlugAsync(dto.Name);

        var category = new Category
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            ParentCategoryId = dto.ParentCategoryId,
            SortOrder = dto.SortOrder,
            IsActive = true
        };

        var created = await _categoryRepository.CreateAsync(category);
        _logger.LogInformation("Category created: {Name} (Id: {Id})", created.Name, created.Id);
        return ApiResponse<CategoryDto>.Ok(MapToDto(created, 0), "Category created successfully.");
    }

    public async Task<ApiResponse<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
            return ApiResponse<CategoryDto>.Fail("Category not found.");

        if (dto.ParentCategoryId.HasValue)
        {
            if (dto.ParentCategoryId.Value == id)
                return ApiResponse<CategoryDto>.Fail("A category cannot be its own parent.");

            var parent = await _categoryRepository.GetByIdAsync(dto.ParentCategoryId.Value);
            if (parent is null)
                return ApiResponse<CategoryDto>.Fail("Parent category not found.");
        }

        // Regenerate slug only if name changed
        if (!category.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase))
            category.Slug = await GenerateUniqueSlugAsync(dto.Name, excludeId: id);

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.ImageUrl = dto.ImageUrl;
        category.ParentCategoryId = dto.ParentCategoryId;
        category.IsActive = dto.IsActive;
        category.SortOrder = dto.SortOrder;

        var updated = await _categoryRepository.UpdateAsync(category);
        var count = await _productRepository.GetCountByCategoryAsync(id);
        _logger.LogInformation("Category updated: {Name} (Id: {Id})", updated.Name, updated.Id);
        return ApiResponse<CategoryDto>.Ok(MapToDto(updated, count));
    }

    public async Task<ApiResponse> DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id, includeSubCategories: true);
        if (category is null)
            return ApiResponse.Fail("Category not found.");

        if (category.SubCategories.Any())
            return ApiResponse.Fail("Cannot delete a category that has sub-categories. Remove sub-categories first.");

        if (await _categoryRepository.HasProductsAsync(id))
            return ApiResponse.Fail("Cannot delete a category that has products assigned to it.");

        await _categoryRepository.DeleteAsync(category);
        _logger.LogInformation("Category deleted: Id {Id}", id);
        return ApiResponse.Ok("Category deleted successfully.");
    }

    // -------------------------------------------------------------------------
    private async Task<string> GenerateUniqueSlugAsync(string name, int? excludeId = null)
    {
        var baseSlug = SlugHelper.Generate(name);
        var slug = baseSlug;
        var counter = 1;

        while (await _categoryRepository.SlugExistsAsync(slug, excludeId))
            slug = $"{baseSlug}-{counter++}";

        return slug;
    }

    private static CategoryDto MapToDto(Category c, int productCount) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        Description = c.Description,
        ImageUrl = c.ImageUrl,
        ParentCategoryId = c.ParentCategoryId,
        ParentCategoryName = c.ParentCategory?.Name,
        IsActive = c.IsActive,
        SortOrder = c.SortOrder,
        ProductCount = productCount
    };

    private static CategoryTreeDto MapToTree(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        ImageUrl = c.ImageUrl,
        SortOrder = c.SortOrder,
        SubCategories = c.SubCategories
            .OrderBy(s => s.SortOrder).ThenBy(s => s.Name)
            .Select(MapToTree)
            .ToList()
    };
}

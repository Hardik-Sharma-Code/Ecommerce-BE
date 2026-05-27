using Ecommerce_BE.Services.Implementations;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce_BE.Services.Extensions;

public static class ServiceLayerExtensions
{
    public static IServiceCollection AddServiceLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISearchService, SearchService>();

        return services;
    }
}

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
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<ICouponService, CouponService>();

        services.Configure<RazorpaySettings>(configuration.GetSection("PaymentGateways:Razorpay"));
        services.Configure<StripeSettings>(configuration.GetSection("PaymentGateways:Stripe"));
        services.Configure<ShippingSettings>(configuration.GetSection("Shipping"));

        services.AddHttpClient("Razorpay", client =>
            client.BaseAddress = new Uri("https://api.razorpay.com/v1/"));
        services.AddHttpClient("Stripe", client =>
            client.BaseAddress = new Uri("https://api.stripe.com/v1/"));

        // Register gateways as IPaymentGateway (IEnumerable<IPaymentGateway> enables factory resolution)
        services.AddScoped<IPaymentGateway, RazorpayGateway>();
        services.AddScoped<IPaymentGateway, StripeGateway>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IRefundService, RefundService>();
        services.AddScoped<IShippingService, ShippingService>();

        return services;
    }
}

using Ecommerce_BE.Services.Implementations;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

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

        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IEmailService, StubEmailService>();
        services.AddScoped<ISmsService, StubSmsService>();

        services.Configure<FileUploadSettings>(configuration.GetSection("FileUpload"));
        services.AddHttpContextAccessor();

        return services;
    }
}

using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Dashboard;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ApiResponse<AdminDashboardDto>> GetAdminDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalOrders = await _context.Orders.CountAsync();
        var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
        var totalProducts = await _context.Products.CountAsync();
        var lowStockProducts = await _context.Products.CountAsync(p => p.IsActive && p.StockQuantity > 0 && p.StockQuantity <= p.LowStockThreshold);
        var totalReviews = await _context.Reviews.CountAsync();
        var pendingReviews = await _context.Reviews.CountAsync(r => !r.IsApproved);

        var customers = await _userManager.GetUsersInRoleAsync(Roles.Customer);
        var vendors = await _userManager.GetUsersInRoleAsync(Roles.Vendor);

        var totalRevenue = await _context.Orders
            .Where(o => o.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var revenueToday = await _context.Orders
            .Where(o => o.PaymentStatus == PaymentStatus.Paid && o.CreatedAt >= todayStart)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var revenueThisMonth = await _context.Orders
            .Where(o => o.PaymentStatus == PaymentStatus.Paid && o.CreatedAt >= monthStart)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        // Materialize before calling .ToString() on enum — EF Core cannot translate it to SQL
        var recentOrdersRaw = await _context.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new { o.Id, o.OrderNumber, CustomerEmail = o.User.Email ?? string.Empty, o.TotalAmount, o.Status, o.CreatedAt })
            .ToListAsync();

        var recentOrders = recentOrdersRaw.Select(o => new RecentOrderDto
        {
            OrderId = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerEmail = o.CustomerEmail,
            TotalAmount = o.TotalAmount,
            Status = o.Status.ToString(),
            CreatedAt = o.CreatedAt
        }).ToList();

        var topProducts = await _context.OrderItems
            .Where(oi => oi.ProductId != null)
            .GroupBy(oi => new { oi.ProductId, oi.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId!.Value,
                ProductName = g.Key.ProductName,
                TotalSold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.Subtotal)
            })
            .OrderByDescending(p => p.TotalSold)
            .Take(5)
            .ToListAsync();

        var dto = new AdminDashboardDto
        {
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            TotalCustomers = customers.Count,
            TotalVendors = vendors.Count,
            TotalProducts = totalProducts,
            LowStockProducts = lowStockProducts,
            TotalRevenue = totalRevenue,
            RevenueToday = revenueToday,
            RevenueThisMonth = revenueThisMonth,
            TotalReviews = totalReviews,
            PendingReviews = pendingReviews,
            RecentOrders = recentOrders,
            TopProducts = topProducts
        };

        return ApiResponse<AdminDashboardDto>.Ok(dto);
    }

    public async Task<ApiResponse<VendorDashboardDto>> GetVendorDashboardAsync(int vendorProfileId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalProducts = await _context.Products.CountAsync(p => p.VendorId == vendorProfileId);
        var activeProducts = await _context.Products.CountAsync(p => p.VendorId == vendorProfileId && p.IsActive);
        var lowStockProducts = await _context.Products.CountAsync(p => p.VendorId == vendorProfileId && p.IsActive && p.StockQuantity > 0 && p.StockQuantity <= p.LowStockThreshold);

        var vendorProductIds = await _context.Products
            .Where(p => p.VendorId == vendorProfileId)
            .Select(p => p.Id)
            .ToListAsync();

        var vendorOrders = _context.Orders
            .Where(o => o.Items.Any(i => i.ProductId != null && vendorProductIds.Contains(i.ProductId!.Value)));

        var totalOrders = await vendorOrders.CountAsync();
        var pendingOrders = await vendorOrders.CountAsync(o => o.Status == OrderStatus.Pending);

        // Revenue counts only the items that belong to this vendor, not the full order amount
        var vendorRevenue = await _context.OrderItems
            .Where(oi => oi.ProductId != null
                      && vendorProductIds.Contains(oi.ProductId!.Value)
                      && oi.Order.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(oi => (decimal?)oi.Subtotal) ?? 0;

        var vendorRevenueThisMonth = await _context.OrderItems
            .Where(oi => oi.ProductId != null
                      && vendorProductIds.Contains(oi.ProductId!.Value)
                      && oi.Order.PaymentStatus == PaymentStatus.Paid
                      && oi.Order.CreatedAt >= monthStart)
            .SumAsync(oi => (decimal?)oi.Subtotal) ?? 0;

        var ratings = await _context.Reviews
            .Where(r => vendorProductIds.Contains(r.ProductId) && r.IsApproved)
            .Select(r => r.Rating)
            .ToListAsync();

        var averageRating = ratings.Count > 0 ? ratings.Average() : 0;

        // Materialize before calling .ToString() on enum
        var recentOrdersRaw = await vendorOrders
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new { o.Id, o.OrderNumber, CustomerEmail = o.User.Email ?? string.Empty, o.TotalAmount, o.Status, o.CreatedAt })
            .ToListAsync();

        var recentOrders = recentOrdersRaw.Select(o => new RecentOrderDto
        {
            OrderId = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerEmail = o.CustomerEmail,
            TotalAmount = o.TotalAmount,
            Status = o.Status.ToString(),
            CreatedAt = o.CreatedAt
        }).ToList();

        var topProducts = await _context.OrderItems
            .Where(oi => oi.ProductId != null && vendorProductIds.Contains(oi.ProductId!.Value))
            .GroupBy(oi => new { oi.ProductId, oi.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId!.Value,
                ProductName = g.Key.ProductName,
                TotalSold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.Subtotal)
            })
            .OrderByDescending(p => p.TotalSold)
            .Take(5)
            .ToListAsync();

        var dto = new VendorDashboardDto
        {
            TotalProducts = totalProducts,
            ActiveProducts = activeProducts,
            LowStockProducts = lowStockProducts,
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            TotalRevenue = vendorRevenue,
            RevenueThisMonth = vendorRevenueThisMonth,
            AverageRating = Math.Round(averageRating, 1),
            TotalReviews = ratings.Count,
            RecentOrders = recentOrders,
            TopProducts = topProducts
        };

        return ApiResponse<VendorDashboardDto>.Ok(dto);
    }
}

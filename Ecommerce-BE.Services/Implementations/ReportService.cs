using System.Text;
using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Report;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Services.Implementations;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context) => _context = context;

    public async Task<ApiResponse<SalesReportDto>> GetSalesReportAsync(ReportRequestDto request)
    {
        var from = request.FromDate.Date;
        var to = request.ToDate.Date.AddDays(1);

        if (request.VendorId.HasValue)
        {
            // Vendor report: sum only items belonging to this vendor, not the full order amount
            var vendorId = request.VendorId.Value;

            var vendorProductIds = await _context.Products
                .Where(p => p.VendorId == vendorId)
                .Select(p => p.Id)
                .ToListAsync();

            var vendorItems = await _context.OrderItems
                .Where(oi => oi.ProductId != null
                          && vendorProductIds.Contains(oi.ProductId!.Value)
                          && oi.Order.CreatedAt >= from
                          && oi.Order.CreatedAt < to
                          && oi.Order.PaymentStatus == PaymentStatus.Paid)
                .Select(oi => new { oi.Order.CreatedAt, oi.Subtotal })
                .ToListAsync();

            var dailyBreakdown = vendorItems
                .GroupBy(i => i.CreatedAt.Date)
                .Select(g => new DailySalesDto
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(i => i.Subtotal)
                })
                .OrderBy(d => d.Date)
                .ToList();

            var dto = new SalesReportDto
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalOrders = dailyBreakdown.Sum(d => d.OrderCount),
                TotalRevenue = vendorItems.Sum(i => i.Subtotal),
                TotalDiscount = 0,
                TotalTax = 0,
                NetRevenue = vendorItems.Sum(i => i.Subtotal),
                DailyBreakdown = dailyBreakdown
            };

            return ApiResponse<SalesReportDto>.Ok(dto);
        }
        else
        {
            // Admin report: full order view
            var orders = await _context.Orders
                .Where(o => o.CreatedAt >= from && o.CreatedAt < to && o.PaymentStatus == PaymentStatus.Paid)
                .ToListAsync();

            var dailyBreakdown = orders
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new DailySalesDto
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(d => d.Date)
                .ToList();

            var dto = new SalesReportDto
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                TotalDiscount = orders.Sum(o => o.DiscountAmount),
                TotalTax = orders.Sum(o => o.TaxAmount),
                NetRevenue = orders.Sum(o => o.TotalAmount - o.TaxAmount),
                DailyBreakdown = dailyBreakdown
            };

            return ApiResponse<SalesReportDto>.Ok(dto);
        }
    }

    public async Task<ApiResponse<InventoryReportDto>> GetInventoryReportAsync(int? vendorId = null)
    {
        var query = _context.Products.AsQueryable();
        if (vendorId.HasValue)
            query = query.Where(p => p.VendorId == vendorId.Value);

        var products = await query.AsNoTracking().ToListAsync();

        var items = products.Select(p => new InventoryItemDto
        {
            ProductId = p.Id,
            ProductName = p.Name,
            SKU = p.SKU,
            StockQuantity = p.StockQuantity,
            Price = p.Price,
            Status = p.StockQuantity == 0 ? "OutOfStock"
                   : p.StockQuantity <= p.LowStockThreshold ? "LowStock"
                   : "InStock"
        }).ToList();

        var dto = new InventoryReportDto
        {
            TotalProducts = items.Count,
            InStockProducts = items.Count(i => i.Status == "InStock"),
            OutOfStockProducts = items.Count(i => i.Status == "OutOfStock"),
            LowStockProducts = items.Count(i => i.Status == "LowStock"),
            Items = items
        };

        return ApiResponse<InventoryReportDto>.Ok(dto);
    }

    public async Task<ApiResponse<byte[]>> ExportSalesReportCsvAsync(ReportRequestDto request)
    {
        var report = (await GetSalesReportAsync(request)).Data!;

        var sb = new StringBuilder();
        sb.AppendLine("Date,Order Count,Revenue");
        foreach (var row in report.DailyBreakdown)
            sb.AppendLine($"{row.Date:yyyy-MM-dd},{row.OrderCount},{row.Revenue:F2}");

        sb.AppendLine();
        sb.AppendLine($"Total Orders,{report.TotalOrders}");
        sb.AppendLine($"Total Revenue,{report.TotalRevenue:F2}");
        sb.AppendLine($"Total Discount,{report.TotalDiscount:F2}");
        sb.AppendLine($"Total Tax,{report.TotalTax:F2}");
        sb.AppendLine($"Net Revenue,{report.NetRevenue:F2}");

        return ApiResponse<byte[]>.Ok(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    public async Task<ApiResponse<byte[]>> ExportInventoryReportCsvAsync(int? vendorId = null)
    {
        var report = (await GetInventoryReportAsync(vendorId)).Data!;

        var sb = new StringBuilder();
        sb.AppendLine("Product ID,Product Name,SKU,Stock Quantity,Price,Status");
        foreach (var item in report.Items)
            sb.AppendLine($"{item.ProductId},\"{item.ProductName}\",{item.SKU},{item.StockQuantity},{item.Price:F2},{item.Status}");

        return ApiResponse<byte[]>.Ok(Encoding.UTF8.GetBytes(sb.ToString()));
    }
}

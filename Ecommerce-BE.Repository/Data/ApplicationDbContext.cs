using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<VendorProfile> VendorProfiles => Set<VendorProfile>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        });

        builder.Entity<CustomerProfile>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasOne(c => c.User)
                  .WithOne(u => u.CustomerProfile)
                  .HasForeignKey<CustomerProfile>(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(c => c.PhoneNumber).HasMaxLength(20);
            entity.Property(c => c.Address).HasMaxLength(500);
            entity.Property(c => c.City).HasMaxLength(100);
            entity.Property(c => c.State).HasMaxLength(100);
            entity.Property(c => c.Country).HasMaxLength(100);
            entity.Property(c => c.PostalCode).HasMaxLength(20);
        });

        builder.Entity<VendorProfile>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.HasOne(v => v.User)
                  .WithOne(u => u.VendorProfile)
                  .HasForeignKey<VendorProfile>(v => v.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(v => v.BusinessName).HasMaxLength(200).IsRequired();
            entity.Property(v => v.BusinessDescription).HasMaxLength(1000);
            entity.Property(v => v.BusinessAddress).HasMaxLength(500);
            entity.Property(v => v.BusinessPhone).HasMaxLength(20);
            entity.Property(v => v.KycDocumentType).HasMaxLength(50);
            entity.Property(v => v.KycDocumentNumber).HasMaxLength(100);
            entity.Property(v => v.KycRejectionReason).HasMaxLength(500);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasOne(r => r.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(r => r.Token).HasMaxLength(500).IsRequired();
            entity.HasIndex(r => r.Token).IsUnique();
        });

        builder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Slug).HasMaxLength(120).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.Property(c => c.ImageUrl).HasMaxLength(500);
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasOne(c => c.ParentCategory)
                  .WithMany(c => c.SubCategories)
                  .HasForeignKey(c => c.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Slug).HasMaxLength(250).IsRequired();
            entity.Property(p => p.ShortDescription).HasMaxLength(500);
            entity.Property(p => p.SKU).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            entity.Property(p => p.CompareAtPrice).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Tags).HasMaxLength(500);
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.HasIndex(p => p.SKU).IsUnique();
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>()
                  .WithMany()
                  .HasForeignKey(p => p.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(pi => pi.Id);
            entity.Property(pi => pi.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(pi => pi.AltText).HasMaxLength(200);
            entity.HasOne(pi => pi.Product)
                  .WithMany(p => p.Images)
                  .HasForeignKey(pi => pi.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Cart>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(c => c.UserId).IsUnique();
        });

        builder.Entity<CartItem>(entity =>
        {
            entity.HasKey(ci => ci.Id);
            entity.HasOne(ci => ci.Cart)
                  .WithMany(c => c.Items)
                  .HasForeignKey(ci => ci.CartId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ci => ci.Product)
                  .WithMany()
                  .HasForeignKey(ci => ci.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
        });

        builder.Entity<WishlistItem>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.HasOne(w => w.User)
                  .WithMany()
                  .HasForeignKey(w => w.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(w => w.Product)
                  .WithMany()
                  .HasForeignKey(w => w.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
        });

        builder.Entity<CustomerAddress>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(a => a.Label).HasMaxLength(50).IsRequired();
            entity.Property(a => a.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(a => a.LastName).HasMaxLength(100).IsRequired();
            entity.Property(a => a.PhoneNumber).HasMaxLength(20);
            entity.Property(a => a.AddressLine1).HasMaxLength(250).IsRequired();
            entity.Property(a => a.AddressLine2).HasMaxLength(250);
            entity.Property(a => a.City).HasMaxLength(100).IsRequired();
            entity.Property(a => a.State).HasMaxLength(100).IsRequired();
            entity.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
            entity.Property(a => a.Country).HasMaxLength(100).IsRequired();
        });

        builder.Entity<Coupon>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Code).HasMaxLength(50).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.Property(c => c.DiscountValue).HasColumnType("decimal(18,2)");
            entity.Property(c => c.MinOrderAmount).HasColumnType("decimal(18,2)");
            entity.Property(c => c.MaxDiscountAmount).HasColumnType("decimal(18,2)");
            entity.HasIndex(c => c.Code).IsUnique();
        });

        builder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.OrderNumber).HasMaxLength(30).IsRequired();
            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.Property(o => o.ShippingFirstName).HasMaxLength(100).IsRequired();
            entity.Property(o => o.ShippingLastName).HasMaxLength(100).IsRequired();
            entity.Property(o => o.ShippingPhone).HasMaxLength(20);
            entity.Property(o => o.ShippingAddressLine1).HasMaxLength(250).IsRequired();
            entity.Property(o => o.ShippingAddressLine2).HasMaxLength(250);
            entity.Property(o => o.ShippingCity).HasMaxLength(100).IsRequired();
            entity.Property(o => o.ShippingState).HasMaxLength(100).IsRequired();
            entity.Property(o => o.ShippingPostalCode).HasMaxLength(20).IsRequired();
            entity.Property(o => o.ShippingCountry).HasMaxLength(100).IsRequired();
            entity.Property(o => o.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(o => o.ShippingAmount).HasColumnType("decimal(18,2)");
            entity.Property(o => o.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(o => o.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(o => o.CouponCode).HasMaxLength(50);
            entity.Property(o => o.CancellationReason).HasMaxLength(500);
            entity.Property(o => o.Notes).HasMaxLength(500);
            entity.HasOne(o => o.User)
                  .WithMany()
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(oi => oi.SKU).HasMaxLength(100).IsRequired();
            entity.Property(oi => oi.ImageUrl).HasMaxLength(500);
            entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(oi => oi.Subtotal).HasColumnType("decimal(18,2)");
            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.Items)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            // SetNull keeps snapshot data even when product is deleted
            entity.HasOne(oi => oi.Product)
                  .WithMany()
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);
        });

        builder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.GatewayOrderId).HasMaxLength(200);
            entity.Property(p => p.GatewayPaymentId).HasMaxLength(200);
            entity.Property(p => p.GatewaySignature).HasMaxLength(500);
            entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Currency).HasMaxLength(10).IsRequired();
            entity.Property(p => p.FailureReason).HasMaxLength(500);
            entity.HasOne(p => p.Order)
                  .WithOne(o => o.Payment)
                  .HasForeignKey<Payment>(p => p.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Refund>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Amount).HasColumnType("decimal(18,2)");
            entity.Property(r => r.Reason).HasMaxLength(500).IsRequired();
            entity.Property(r => r.GatewayRefundId).HasMaxLength(200);
            entity.Property(r => r.Notes).HasMaxLength(500);
            entity.HasOne(r => r.Order)
                  .WithMany(o => o.Refunds)
                  .HasForeignKey(r => r.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Payment)
                  .WithMany()
                  .HasForeignKey(r => r.PaymentId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);
        });

        builder.Entity<Shipment>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Carrier).HasMaxLength(100).IsRequired();
            entity.Property(s => s.TrackingNumber).HasMaxLength(100);
            entity.Property(s => s.TrackingUrl).HasMaxLength(500);
            entity.Property(s => s.Notes).HasMaxLength(500);
            entity.HasOne(s => s.Order)
                  .WithOne(o => o.Shipment)
                  .HasForeignKey<Shipment>(s => s.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Rename Identity tables for cleaner naming
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
    }
}

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

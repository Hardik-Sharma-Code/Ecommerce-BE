using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ecommerce_BE.Repository.Data;

// Used only for design-time migration generation
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=EcommerceDB;Trusted_Connection=True;TrustServerCertificate=True",
            b => b.MigrationsAssembly("Ecommerce-BE.Repository"));
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

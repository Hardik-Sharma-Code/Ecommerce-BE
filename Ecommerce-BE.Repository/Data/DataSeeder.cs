using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Repository.Data;

public static class DataSeeder
{
    private const string DefaultPassword = "Password@123";

    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (result.Succeeded)
                    logger.LogInformation("Role '{Role}' created successfully.", role);
                else
                    logger.LogError("Failed to create role '{Role}': {Errors}", role,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        await SeedAdminsAsync(userManager, logger);
        await SeedCustomersAsync(userManager, context, logger);
        await SeedVendorsAsync(userManager, context, logger);
    }

    // -------------------------------------------------------------------------
    // Admins
    // -------------------------------------------------------------------------
    private static async Task SeedAdminsAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        var admins = new[]
        {
            new { FirstName = "Super",   LastName = "Admin",   Email = "superadmin@ecommerce.com" },
            new { FirstName = "John",    LastName = "Manager", Email = "john.manager@ecommerce.com" },
            new { FirstName = "Sarah",   LastName = "Ops",     Email = "sarah.ops@ecommerce.com" },
            new { FirstName = "Michael", LastName = "Support", Email = "michael.support@ecommerce.com" },
            new { FirstName = "Diana",   LastName = "Finance", Email = "diana.finance@ecommerce.com" },
        };

        foreach (var a in admins)
        {
            if (await userManager.FindByEmailAsync(a.Email) is not null)
                continue;

            var user = new ApplicationUser
            {
                UserName    = a.Email,
                Email       = a.Email,
                FirstName   = a.FirstName,
                LastName    = a.LastName,
                IsActive    = true,
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(user, DefaultPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, Roles.Admin);
                logger.LogInformation("Admin seeded: {Email}", a.Email);
            }
            else
            {
                logger.LogError("Failed to seed admin {Email}: {Errors}", a.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    // -------------------------------------------------------------------------
    // Customers
    // -------------------------------------------------------------------------
    private static async Task SeedCustomersAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger logger)
    {
        var customers = new[]
        {
            new
            {
                FirstName = "Alice",   LastName = "Johnson",
                Email     = "alice.johnson@example.com",
                Phone     = "+1-555-0101",
                Address   = "123 Maple Street",  City = "New York",
                State     = "NY",  Country = "USA", PostalCode = "10001"
            },
            new
            {
                FirstName = "Bob",     LastName = "Williams",
                Email     = "bob.williams@example.com",
                Phone     = "+1-555-0102",
                Address   = "456 Oak Avenue",     City = "Los Angeles",
                State     = "CA",  Country = "USA", PostalCode = "90001"
            },
            new
            {
                FirstName = "Carol",   LastName = "Brown",
                Email     = "carol.brown@example.com",
                Phone     = "+1-555-0103",
                Address   = "789 Pine Road",      City = "Chicago",
                State     = "IL",  Country = "USA", PostalCode = "60601"
            },
            new
            {
                FirstName = "David",   LastName = "Jones",
                Email     = "david.jones@example.com",
                Phone     = "+1-555-0104",
                Address   = "321 Elm Boulevard",  City = "Houston",
                State     = "TX",  Country = "USA", PostalCode = "77001"
            },
            new
            {
                FirstName = "Emma",    LastName = "Davis",
                Email     = "emma.davis@example.com",
                Phone     = "+1-555-0105",
                Address   = "654 Birch Lane",     City = "Phoenix",
                State     = "AZ",  Country = "USA", PostalCode = "85001"
            },
        };

        foreach (var c in customers)
        {
            if (await userManager.FindByEmailAsync(c.Email) is not null)
                continue;

            var user = new ApplicationUser
            {
                UserName       = c.Email,
                Email          = c.Email,
                FirstName      = c.FirstName,
                LastName       = c.LastName,
                PhoneNumber    = c.Phone,
                IsActive       = true,
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(user, DefaultPassword);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to seed customer {Email}: {Errors}", c.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                continue;
            }

            await userManager.AddToRoleAsync(user, Roles.Customer);

            context.CustomerProfiles.Add(new CustomerProfile
            {
                UserId     = user.Id,
                PhoneNumber = c.Phone,
                Address    = c.Address,
                City       = c.City,
                State      = c.State,
                Country    = c.Country,
                PostalCode = c.PostalCode,
            });

            logger.LogInformation("Customer seeded: {Email}", c.Email);
        }

        await context.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // Vendors
    // -------------------------------------------------------------------------
    private static async Task SeedVendorsAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger logger)
    {
        var vendors = new[]
        {
            new
            {
                FirstName = "James",  LastName = "Carter",
                Email     = "james.carter@techstore.com",
                BusinessName = "TechStore Pro",
                BusinessDescription = "Latest gadgets, laptops, and accessories.",
                BusinessPhone = "+1-800-0201",
                BusinessAddress = "10 Silicon Valley Blvd, San Jose, CA 95110",
                KycStatus = KycStatus.Approved,
                DocType = "GST", DocNumber = "GST-001-2024",
                SubmittedAt = DateTime.UtcNow.AddDays(-30), ReviewedAt = DateTime.UtcNow.AddDays(-25)
            },
            new
            {
                FirstName = "Sophia", LastName = "Lee",
                Email     = "sophia.lee@fashionhub.com",
                BusinessName = "Fashion Hub",
                BusinessDescription = "Trendy clothing and accessories for all seasons.",
                BusinessPhone = "+1-800-0202",
                BusinessAddress = "25 Fashion Ave, New York, NY 10018",
                KycStatus = KycStatus.Submitted,
                DocType = "PAN", DocNumber = "PAN-002-2024",
                SubmittedAt = DateTime.UtcNow.AddDays(-5), ReviewedAt = (DateTime?)null
            },
            new
            {
                FirstName = "Ryan",   LastName = "Patel",
                Email     = "ryan.patel@electronicsworld.com",
                BusinessName = "Electronics World",
                BusinessDescription = "Consumer electronics and home appliances at best prices.",
                BusinessPhone = "+1-800-0203",
                BusinessAddress = "50 Tech Park, Austin, TX 78701",
                KycStatus = KycStatus.Approved,
                DocType = "GST", DocNumber = "GST-003-2024",
                SubmittedAt = DateTime.UtcNow.AddDays(-60), ReviewedAt = DateTime.UtcNow.AddDays(-55)
            },
            new
            {
                FirstName = "Olivia", LastName = "Martin",
                Email     = "olivia.martin@homedecor.com",
                BusinessName = "Home Décor",
                BusinessDescription = "Premium home furnishings and interior decor.",
                BusinessPhone = "+1-800-0204",
                BusinessAddress = "99 Design Street, Portland, OR 97201",
                KycStatus = KycStatus.Rejected,
                DocType = "PAN", DocNumber = "PAN-004-2024",
                SubmittedAt = DateTime.UtcNow.AddDays(-20), ReviewedAt = DateTime.UtcNow.AddDays(-15)
            },
            new
            {
                FirstName = "Liam",   LastName = "Nguyen",
                Email     = "liam.nguyen@sportszone.com",
                BusinessName = "Sports Zone",
                BusinessDescription = "Sports equipment and activewear for every athlete.",
                BusinessPhone = "+1-800-0205",
                BusinessAddress = "77 Athletic Drive, Denver, CO 80201",
                KycStatus = KycStatus.Pending,
                DocType   = (string?)null, DocNumber = (string?)null,
                SubmittedAt = (DateTime?)null, ReviewedAt = (DateTime?)null
            },
        };

        foreach (var v in vendors)
        {
            if (await userManager.FindByEmailAsync(v.Email) is not null)
                continue;

            var user = new ApplicationUser
            {
                UserName       = v.Email,
                Email          = v.Email,
                FirstName      = v.FirstName,
                LastName       = v.LastName,
                IsActive       = true,
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(user, DefaultPassword);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to seed vendor {Email}: {Errors}", v.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                continue;
            }

            await userManager.AddToRoleAsync(user, Roles.Vendor);

            context.VendorProfiles.Add(new VendorProfile
            {
                UserId              = user.Id,
                BusinessName        = v.BusinessName,
                BusinessDescription = v.BusinessDescription,
                BusinessPhone       = v.BusinessPhone,
                BusinessAddress     = v.BusinessAddress,
                KycStatus           = v.KycStatus,
                KycDocumentType     = v.DocType,
                KycDocumentNumber   = v.DocNumber,
                KycRejectionReason  = v.KycStatus == KycStatus.Rejected
                                        ? "Document number could not be verified. Please resubmit."
                                        : null,
                KycSubmittedAt      = v.SubmittedAt,
                KycReviewedAt       = v.ReviewedAt,
            });

            logger.LogInformation("Vendor seeded: {Email} (KYC: {Status})", v.Email, v.KycStatus);
        }

        await context.SaveChangesAsync();
    }
}

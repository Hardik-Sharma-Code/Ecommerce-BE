using Ecommerce_BE.Extensions;
using Ecommerce_BE.Middleware;
using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Extensions;
using Ecommerce_BE.Services.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Repository layer (DbContext + Identity + Repositories)
builder.Services.AddRepositoryServices(builder.Configuration);

// Service layer (JWT settings + Application services)
builder.Services.AddServiceLayer(builder.Configuration);

// JWT Authentication + Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);

// Controllers
builder.Services.AddControllers();

// Swagger with JWT support
builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

// Auto-migrate and seed roles on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedRolesAsync(scope.ServiceProvider);
}

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce API v1");
    c.RoutePrefix = string.Empty; // Swagger at root
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

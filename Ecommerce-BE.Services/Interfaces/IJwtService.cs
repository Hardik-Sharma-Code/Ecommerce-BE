using System.Security.Claims;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Services.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    DateTime GetAccessTokenExpiry();
}

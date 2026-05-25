namespace Ecommerce_BE.Shared.Kernel.Common;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Vendor = "Vendor";
    public const string Customer = "Customer";

    public static readonly IReadOnlyList<string> All = [Admin, Vendor, Customer];
}

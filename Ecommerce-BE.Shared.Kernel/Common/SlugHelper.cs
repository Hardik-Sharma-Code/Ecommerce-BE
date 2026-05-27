using System.Text.RegularExpressions;

namespace Ecommerce_BE.Shared.Kernel.Common;

public static class SlugHelper
{
    public static string Generate(string text)
    {
        text = text.ToLowerInvariant().Trim();
        text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
        text = Regex.Replace(text, @"\s+", "-");
        text = Regex.Replace(text, @"-+", "-");
        return text.Trim('-');
    }
}

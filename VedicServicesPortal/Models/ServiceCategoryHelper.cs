using System;

namespace VedicServicesPortal.Models
{
    public static class ServiceCategoryHelper
    {
        public static string GetCategoryKey(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) return "puja";

            if (serviceName.IndexOf("Havan", StringComparison.OrdinalIgnoreCase) >= 0 ||
                serviceName.IndexOf("Yagna", StringComparison.OrdinalIgnoreCase) >= 0 ||
                serviceName.IndexOf("Homa", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "havan";
            }

            if (serviceName.IndexOf("Jyotish", StringComparison.OrdinalIgnoreCase) >= 0 ||
                serviceName.IndexOf("Kundali", StringComparison.OrdinalIgnoreCase) >= 0 ||
                serviceName.IndexOf("Dosha", StringComparison.OrdinalIgnoreCase) >= 0 ||
                serviceName.IndexOf("Navgraha", StringComparison.OrdinalIgnoreCase) >= 0 ||
                serviceName.IndexOf("Kaal Sarp", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "jyotish";
            }

            if (serviceName.IndexOf("Griha", StringComparison.OrdinalIgnoreCase) >= 0 ||
                serviceName.IndexOf("Sanskar", StringComparison.OrdinalIgnoreCase) >= 0 ||
                serviceName.IndexOf("Vastu", StringComparison.OrdinalIgnoreCase) >= 0 ||
                serviceName.IndexOf("Vivah", StringComparison.OrdinalIgnoreCase) >= 0 ||
                serviceName.IndexOf("Namkaran", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "griha";
            }

            return "puja";
        }

        public static string GetCategoryDisplayName(string categoryKey)
        {
            if (string.IsNullOrWhiteSpace(categoryKey)) return "Sacred Puja";

            return categoryKey.ToLowerInvariant() switch
            {
                "havan" => "Sacred Havan & Homa",
                "jyotish" => "Astrology & Dosha",
                "griha" => "Griha & Sanskar",
                _ => "Sacred Puja"
            };
        }

        public static string GetCategoryIcon(string categoryKey)
        {
            if (string.IsNullOrWhiteSpace(categoryKey)) return "🪔";

            return categoryKey.ToLowerInvariant() switch
            {
                "havan" => "🔥",
                "jyotish" => "🔮",
                "griha" => "🏠",
                _ => "🪔"
            };
        }
    }
}

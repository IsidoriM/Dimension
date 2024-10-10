namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        public static string BasePath(this IConfiguration configuration)
        {
            var path = configuration["VirtualPath"] ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(path) && !path.Contains('#'))
            {
                return "/" + path.Trim('/');
            }
            return string.Empty;
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Passi.Core.Extensions
{
    static class HttpContextExtensions
    {
        public static string GetString(this HttpRequest request, string key)
        {
            var value = request.Query[key];

            if (!string.IsNullOrWhiteSpace(value))
                return value.ToString().Trim();

            value = request.Headers[key];
            if (!string.IsNullOrWhiteSpace(value))
                return value.ToString().Trim();

            try
            {
                value = request.Form[key];
                if (!string.IsNullOrWhiteSpace(value))
                    return value.ToString().Trim();
            }
            catch
            {
                //Do nothing
            }


            value = request.Cookies[key];
            if (!string.IsNullOrWhiteSpace(value))
                return value.ToString().Trim();

            try
            {
                value = request.HttpContext.RequestServices.GetService<IConfiguration>()?[key];
                if (!string.IsNullOrWhiteSpace(value))
                    return value.ToString().Trim();
            }
            catch
            {
                // Do nothing
            }

            return string.Empty;
        }
    }
}

using Passi.Authentication.Cookie.Const;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using System.Reflection;

namespace Microsoft.AspNetCore.Http
{
    internal static class CookieExtensions
    {
        public static void RemoveCookie(this HttpContext httpContext, string name)
        {
            httpContext.Response.Cookies.Delete(name);
        }

        public static void AddCypheredCookie(this HttpContext httpContext, string name, string value, IDataCypherService dataCypher)
        {
            try
            {
                value = dataCypher.Crypt(value, Crypto.KCA);
                httpContext.Response.Cookies.Append(name, value, new CookieOptions()
                {
                    Path = "/",
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    Domain = ".inps.it",
                });
            }
            catch (TypeLoadException)
            {
                // Do nothing
            }
        }
    }
}

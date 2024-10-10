using Microsoft.AspNetCore.Http;
using Passi.Authentication.Cookie.Const;
using Passi.Core.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Passi.Authentication.Cookie.Repository
{
    class CookieHostingAppManager : IHostingAppManager
    {
        private readonly IHttpContextAccessor accessor;

        public CookieHostingAppManager(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public Task ClearAllInfoAsync()
        {
            var httpContext = accessor.HttpContext;

            if (httpContext != null)
            {
                foreach (var requestCookie in httpContext.Request.Cookies.Select(x => x.Key))
                {
                    httpContext.RemoveCookie(requestCookie!);
                }
            }
            return Task.CompletedTask;
        }

        public Task ClearExternalInfoAsync()
        {
            var httpContext = accessor.HttpContext;

            if (httpContext != null)
            {
                foreach (var requestCookie in httpContext.Request.Cookies.Select(x => x.Key))
                {
                    var passiCookieKeys = new List<string>()
                    {
                        Cookies.ContactCenterSCC.ToUpper(),
                        Cookies.ContactCenterVSU.ToUpper(),
                        Cookies.Convention.ToUpper(),
                        Cookies.Profile.ToUpper(),
                        Cookies.Session.ToUpper()
                    };

                    var key = requestCookie?.ToUpper() ?? string.Empty;

                    /// Eliminiamo tutti i cookie che NON sono di PASSI
                    if (!string.IsNullOrWhiteSpace(requestCookie) && !passiCookieKeys.Contains(key))
                    {
                        httpContext.RemoveCookie(requestCookie!);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}

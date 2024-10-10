using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Store.Fake.Options;

namespace Passi.Core.Store.Fake.Services
{

    /// <summary>
    /// Rimuovere il public (ma capire perchè non funziona il visible to)
    /// </summary>
    class FakeAuthenticationService : IPassiAuthenticationService
    {
        private readonly ErrorOptions errorOptions;
        private readonly IPassiAuthenticationService authenticationService;

        public FakeAuthenticationService(
            IOptions<ErrorOptions> errorOptions,
            IPassiAuthenticationService authenticationService)
        {
            this.errorOptions = errorOptions.Value;
            this.authenticationService = authenticationService;
        }

        public async Task<SessionInfo> IsAuthorizedAsync(int serviceId, string returnUrl = "")
        {
            if (!string.IsNullOrWhiteSpace(errorOptions.Url))
            {
                var url = new Uri(errorOptions.Url);
                throw new PassiUnauthorizedException(url, errorOptions.Reason);
            }

            var sessionInfo = await authenticationService.IsAuthorizedAsync(serviceId);

            return sessionInfo;
        }

        public Uri ReturnUrl(IHttpContextAccessor contextAccessor)
        {
            var _request = contextAccessor.HttpContext?.Request;

            /// Per la fake impostiamo come returnUrl direttamente la home
            var uribuilder = new UriBuilder()
            {
                Scheme = Schema.Https,
                Host = _request?.Host.Host,
                Path = "/"
            };

            return uribuilder.Uri;
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Passi.Core.Application.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using System.Collections.Specialized;

namespace Passi.Core.Services
{

    /// <summary>
    /// Rimuovere il public (ma capire perchè non funziona il visible to)
    /// </summary>
    internal class ApiAuthenticationService : IPassiAuthenticationService
    {
        private readonly IInfoRepository<SessionToken> sessionTokenRepository;
        private readonly IDataCypherService dataCypher;
        private readonly IPassiAuthenticationService authenticationService;
        private readonly IHostingAppManager appManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ConfigurationOptions configurationOptions;
        private readonly UrlOptions urlOptions;

        public ApiAuthenticationService(
            IInfoRepository<SessionToken> sessionTokenRepository,
            IDataCypherService dataCypher,
            IOptions<UrlOptions> urlOptions,
            IOptions<ConfigurationOptions> configurationOptions,
            IPassiAuthenticationService authenticationService,
            IHostingAppManager appManager,
            IHttpContextAccessor httpContextAccessor)
        {
            this.sessionTokenRepository = sessionTokenRepository;
            this.dataCypher = dataCypher;
            this.authenticationService = authenticationService;
            this.appManager = appManager;
            this.httpContextAccessor = httpContextAccessor;
            this.configurationOptions = configurationOptions.Value;
            this.urlOptions = urlOptions.Value;
        }

        public async Task<SessionInfo> IsAuthorizedAsync(int serviceId, string returnUrl = "")
        {
            var token = await sessionTokenRepository.RetrieveAsync();
            if (token.IsValid)
            {
                returnUrl = (await ReturnUrlAsync(httpContextAccessor)).ToString();
            }

            var sessionInfo = await authenticationService.IsAuthorizedAsync(serviceId, returnUrl);
            string path = $"{httpContextAccessor.HttpContext?.Request.PathBase.ToString().ToLower()}/{httpContextAccessor.HttpContext?.Request.Path.ToString().ToLower()}".Trim('/');

            /// Se il path è di tipo /api/{action} e non è l'endpoint del token, allora fai il controllo del token
            bool shouldCheckToken = path.IsApi();
            if (shouldCheckToken)
            {

                if (!token.IsValid)
                {
                    var redirectUrl = token.ServiceUri
                        .AddToQueryString(Keys.ReturnUrl, returnUrl);
                    throw new PassiUnauthorizedException(redirectUrl,
                        ErrorCodes.Three,
                        Reason.SessionTokenNotFound);
                }

                if (sessionInfo.UserId != token.UserId)
                {
                    /// Cancellare tutti i cookie tranne quelli di PASSI che verranno tolti dal SSO
                    /// I cookie vanno eleminati anche quando vieni rediretto al logout

                    await appManager.ClearExternalInfoAsync();

                    var log = dataCypher.Secure(new NameValueCollection
                    {
                        { "idServizio", configurationOptions.ServiceId.ToString() },
                        { "utente", token.UserId },
                        { "loginTime", token.LoggedIn.ToString() }
                    });

                    var url = urlOptions.Logout
                        .AddToQueryString(Keys.SecureTable, log);

                    throw new PassiUnauthorizedException(url,
                        ErrorCodes.Ten, Reason.SessionUserIdDifferentProfileUserId);
                }

                if (!token.LoggedIn.ToString().Equals(sessionInfo.LoggedIn.ToString())
                    || !token.UserTypeId.Equals(sessionInfo.ProfileTypeId)
                    || !token.InstitutionCode.Equals(sessionInfo.InstitutionCode)
                    || !token.OfficeCode.Equals(sessionInfo.OfficeCode))
                {
                    throw new PassiUnauthorizedException(token.ServiceUri,
                        Reason.InvalidSessionToken);
                }
            }

            return sessionInfo;
        }

        private async Task<Uri> ReturnUrlAsync(IHttpContextAccessor contextAccessor)
        {
            if (!string.IsNullOrWhiteSpace(configurationOptions.RedirectUrl))
            {
                return new Uri(configurationOptions.RedirectUrl);
            }

            /// Per le applicazioni API, la returnUrl è la url iniziale del servizio. Viene mantenuta nel SessionToken
            var token = await sessionTokenRepository.RetrieveAsync();
            return token.ServiceUri;
        }
    }
}

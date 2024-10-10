using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Passi.Core.Application.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Extensions;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Passi.Core.Handlers
{
    internal class PassiAuthenticationHandler : AuthenticationHandler<PassiAuthenticationSchemeOption>
    {
        public readonly IPassiAuthenticationService passiAuthenticationService;
        private readonly IHostingAppManager appManager;
        private readonly ConfigurationOptions configurationOptions;
        private const string RedirectLoginParameter = "LoginRedirectUri";
        private const int UnauthorizedStatusCode = 600;
        private const string RedirectUnauthorizedParameter = "UnauthorizedRedirectUri";

        public PassiAuthenticationHandler(IOptionsMonitor<PassiAuthenticationSchemeOption> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IPassiAuthenticationService passiAuthenticationService,
            IHostingAppManager appManager,
            IOptions<ConfigurationOptions> configurationOptions) : base(options, logger, encoder, clock)
        {
            this.passiAuthenticationService = passiAuthenticationService;
            this.appManager = appManager;
            this.configurationOptions = configurationOptions.Value;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var serviceId = configurationOptions.ServiceId;

                SessionInfo authorizedSession = await passiAuthenticationService.IsAuthorizedAsync(serviceId);
                ClaimsPrincipal principal = authorizedSession.CreatePrincipal(Scheme.Name);

                AuthenticationTicket authenticationTicket = new(principal, Scheme.Name);
                return await Task.FromResult(AuthenticateResult.Success(authenticationTicket));
            }
            catch (PassiUnauthorizedException ex)
            {
                var redirectUrl = ex.RedirectUrl.ToString();
                CreateResponseData(redirectUrl);
                if (ex.ClearExternalInfo)
                {
                    await appManager.ClearExternalInfoAsync();
                }
                return await Task.FromResult(AuthenticateResult.Fail("User non authenticated"));
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            string? redirect = Request.HttpContext.Items[RedirectLoginParameter]?.ToString();
            return RedirectMeAsync(redirect);
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            string redirect = Request.HttpContext.Items[RedirectUnauthorizedParameter]?.ToString()!;
            return RedirectMeAsync(redirect);
        }

        protected Task RedirectMeAsync(string? redirect)
        {
            if (!string.IsNullOrEmpty(redirect))
            {
                CreateResponseData(redirect);
                return Task.CompletedTask;
            }
            throw new UnauthorizedAccessException();
        }

        protected void CreateResponseData(string redirect)
        {
            Response.StatusCode = UnauthorizedStatusCode;
            redirect = redirect.Replace("http://", "https://");

            if (!Response.Headers.ContainsKey("X-Location"))
            {
                Response.Headers["X-Location"] = redirect;
            }

            if (!Response.HttpContext.Items.ContainsKey(RedirectLoginParameter))
            {
                Response.HttpContext.Items.Add(RedirectLoginParameter, redirect);
            }

            if (!Request.Path.ToString().IsApi())
            {
                Response.Redirect(redirect);
            }
        }
    }
}

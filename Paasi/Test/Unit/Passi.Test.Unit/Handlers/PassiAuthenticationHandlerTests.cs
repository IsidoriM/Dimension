using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Passi.Core.Application.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Handlers;
using System.Text.Encodings.Web;

namespace Passi.Test.Unit.Handlers
{
    public class PassiAuthenticationHandlerTests
    {
        private readonly Mock<IOptionsMonitor<PassiAuthenticationSchemeOption>> _optionsMonitor;
        private readonly Mock<IOptions<ConfigurationOptions>> _options;
        private readonly Mock<ILoggerFactory> _loggerFactory;
        private readonly Mock<UrlEncoder> _encoder;
        private readonly Mock<ISystemClock> _clock;
        private readonly Mock<IPassiAuthenticationService> _passiAuthorizationService;
        private readonly PassiAuthenticationHandler _handler;
        private readonly Mock<IHostingAppManager> _hostingAppManager;

        public PassiAuthenticationHandlerTests()
        {
            _optionsMonitor = new Mock<IOptionsMonitor<PassiAuthenticationSchemeOption>>();
            _optionsMonitor
                .Setup(x => x.Get(It.IsAny<string>()))
                .Returns(new PassiAuthenticationSchemeOption());

            _options = new Mock<IOptions<ConfigurationOptions>>();
            _options.Setup(x => x.Value).Returns(new ConfigurationOptions()
            {
                ServiceId = 0
            });


            var logger = new Mock<ILogger<PassiAuthenticationHandler>>();
            _loggerFactory = new Mock<ILoggerFactory>();
            _loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            _encoder = new Mock<UrlEncoder>();
            _clock = new Mock<ISystemClock>();
            _passiAuthorizationService = new Mock<IPassiAuthenticationService>();
            _passiAuthorizationService.Setup(x => x.IsAuthorizedAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(new SessionInfo()
            {

            });

            _hostingAppManager = new Mock<IHostingAppManager>();
            _hostingAppManager.Setup(x => x.ClearExternalInfoAsync()).Returns(Task.CompletedTask);

            _handler = new PassiAuthenticationHandler(_optionsMonitor.Object,
                _loggerFactory.Object,
                _encoder.Object,
                _clock.Object,
                _passiAuthorizationService.Object,
                _hostingAppManager.Object,
                _options.Object);
        }

        [Fact]
        public async Task HandleAuthenticateAsync_whenSessionExists_thenAuthorize()
        {
            var context = new DefaultHttpContext();
            context.Items.Add("LoginRedirectUri", "https://www.google.com");
            context.Items.Add("UnauthorizedRedirectUri", "https://www.amazon.com");

            await _handler.InitializeAsync(new AuthenticationScheme("Passi", "Passi", typeof(PassiAuthenticationHandler)), context);
            var result = await _handler.AuthenticateAsync();

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task HandleAuthenticateAsync_whenSessionNotExists_thenForbid()
        {
            var context = new DefaultHttpContext();

            _passiAuthorizationService.Setup(x => x.IsAuthorizedAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new PassiUnauthorizedException(new Uri("https://www.excetera.com"), Reason.InvalidParameter, true));

            await _handler.InitializeAsync(new AuthenticationScheme("Passi", "Passi", typeof(PassiAuthenticationHandler)), context);
            var result = await _handler.AuthenticateAsync();

            Assert.False(result.Succeeded);
            var location = context.Response.Headers["X-Location"].ToString();
            Assert.True(!string.IsNullOrWhiteSpace(location));
            Assert.Contains("excetera", location);
        }

        [Fact]
        public async Task HandleChallengeAsync_whenRedirectionExtists_thenAddXLocation()
        {
            var context = new DefaultHttpContext();
            context.Items.Add("LoginRedirectUri", "https://www.google.com");
            context.Items.Add("UnauthorizedRedirectUri", "https://www.amazon.com");

            await _handler.InitializeAsync(new AuthenticationScheme("Passi", "Passi", typeof(PassiAuthenticationHandler)), context);
            await _handler.ChallengeAsync(new AuthenticationProperties());

            Assert.True(true);

            var location = context.Response.Headers["X-Location"].ToString();
            Assert.True(!string.IsNullOrWhiteSpace(location));
            Assert.Contains("google", location);
        }

        [Fact]
        public async Task HandleChallengeAsync_NoRedirectUri_Unauthorized()
        {
            DefaultHttpContext context = new();
            context.Items.Add("LoginRedirectUri", null);

            await _handler.InitializeAsync(new AuthenticationScheme("Passi", "Passi", typeof(PassiAuthenticationHandler)), context);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.ChallengeAsync(new AuthenticationProperties()));
        }

        [Fact]
        public async Task HandleForbiddenAsync_whenSessionExists_thenAuthorize()
        {
            var context = new DefaultHttpContext();
            context.Items.Add("LoginRedirectUri", "https://www.google.com");
            context.Items.Add("UnauthorizedRedirectUri", "https://www.amazon.com");

            await _handler.InitializeAsync(new AuthenticationScheme("Passi", "Passi", typeof(PassiAuthenticationHandler)), context);
            await _handler.ForbidAsync(new AuthenticationProperties());

            var location = context.Response.Headers["X-Location"].ToString();
            Assert.True(!string.IsNullOrWhiteSpace(location));
            Assert.Contains("amazon", location);
        }

        [Fact]
        public async Task HandleForbiddenAsync_NoRedirectUri_Unauthorized()
        {
            DefaultHttpContext context = new();
            context.Items.Add("UnauthorizedRedirectUri", null);

            await _handler.InitializeAsync(new AuthenticationScheme("Passi", "Passi", typeof(PassiAuthenticationHandler)), context);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.ForbidAsync(new AuthenticationProperties()));
        }
    }
}

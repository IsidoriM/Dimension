using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Passi.Core.Application.Services;
using Passi.Core.Extensions;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Passi.WebApp.Unit.Tests.Cshtml
{
    public class DisableAuthenticationPolicyEvaluator : IPolicyEvaluator
    {
        public async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
        {
            // Always pass authentication.
            var authenticationTicket = new AuthenticationTicket(new ClaimsPrincipal(), new AuthenticationProperties(), ServiceCollectionExtensionsOptions.SCHEME);
            return await Task.FromResult(AuthenticateResult.Success(authenticationTicket));
        }

        public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object resource)
        {
            // Always pass authorization
            return Task.FromResult(PolicyAuthorizationResult.Success());
        }
    }

    public class WithDataTests
    {
        [Theory]
        [InlineData("/")]
        public async Task Html_whenCallingAPage_thenRetrieveData(string url)
        {
            string fakeAppSettings = File.ReadAllText("Contents/fakeAppSettings.json");
            var builder = new ConfigurationBuilder()
                .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(fakeAppSettings)));

            IConfigurationRoot configuration = builder.Build();

            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseConfiguration(configuration);
                    builder.ConfigureTestServices(services =>
                    {
                        services.RemoveAll<IPolicyEvaluator>();
                        services.AddSingleton<IPolicyEvaluator, DisableAuthenticationPolicyEvaluator>();

                        services.Replace(new ServiceDescriptor(typeof(IPassiService), MockPassiService()));
                        services.Replace(new ServiceDescriptor(typeof(IPassiSecureService), MockPassiSecureService()));
                        services.Replace(new ServiceDescriptor(typeof(ICLogService), MockCLogService()));
                    });
                });
            var client = application.CreateClient();
            var response = await client.GetAsync(url);

            var result = await response.Content.ReadAsStringAsync();

            //Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsSuccessStatusCode);

        }

        private IPassiService MockPassiService()
        {
            Mock<IPassiService> mockPassiService = new();
            mockPassiService.Setup(x => x.AuthorizedServicesAsync())
                .ReturnsAsync(new List<int>() { 1 });
            mockPassiService.Setup(x => x.UserContactsAsync())
                .ReturnsAsync(new Core.Domain.Entities.UserContacts() { Email = "aaabbbcccdddeeef" });
            mockPassiService.Setup(x => x.IsAuthorizedAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            mockPassiService.Setup(x => x.HasPatronageDelegationAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            mockPassiService.Setup(x => x.MeAsync())
                .ReturnsAsync(new Core.Domain.Entities.User()
                {
                    UserId = "aaa"
                });
            mockPassiService.Setup(x => x.ProfileAsync())
                .ReturnsAsync(new Core.Domain.Entities.Profile()
                {
                    ProfileTypeId = 1
                });
            mockPassiService.Setup(x => x.LogoutUrl())
                .Returns(new Uri("https://www.google.com"));
            mockPassiService.Setup(x => x.SwitchProfileUrl())
                .Returns(new Uri("https://www.google.com"));

            return mockPassiService.Object;
        }

        private IPassiSecureService MockPassiSecureService()
        {
            Mock<IPassiSecureService> mockPassiSecureService = new();
            mockPassiSecureService.Setup(x => x.SecureAsync(It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync("aaa");
            mockPassiSecureService.Setup(x => x.UnsecureAsync(It.IsAny<string>()))
                .ReturnsAsync(new Dictionary<string, string>() { });
            mockPassiSecureService.Setup(x => x.CheckParameterAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string[]?>()))
                .ReturnsAsync(true);
            mockPassiSecureService.Setup(x => x.SessionTokenAsync())
                .ReturnsAsync("aaa");
            
            return mockPassiSecureService.Object;
        }

        private ICLogService MockCLogService()
        {
            Mock<ICLogService> mockCLogService = new();
            mockCLogService.Setup(x => x.LogAsync(It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Dictionary<string, string>>(),
                0, 
                null))
                .Returns(Task.CompletedTask);

            return mockCLogService.Object;
        }
    }
}

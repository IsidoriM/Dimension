using AutoFixture.Xunit2;
using Bogus;
using Bogus.Extensions.Italy;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Moq;
using Passi.Authentication.Cookie.Const;
using Passi.Authentication.Cookie.Repository;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Test.Unit.Fixtures;
using System.Security.Cryptography;

namespace Passi.Test.Unit.Authentication.Cookie
{
    public class HostingAppManager : IClassFixture<PassiFixture>
    {
        public HostingAppManager(PassiFixture _)
        {
        }

        [Fact]
        public async Task ClearExternalInfoAsync_WithCookies_Ok()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var cookie = cypher.Crypt(string.Empty);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.Session, cookie).ToString());
            context.Request.Headers.Add("AAA", new CookieHeaderValue("AAA", "AAA").ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var repo = new CookieHostingAppManager(mockHttpContextAccessor.Object);
            await repo.ClearExternalInfoAsync();

            Assert.True(true);
        }

        [Fact]
        public async Task ClearAllInfoAsync_WithCookies_Ok()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var cookie = cypher.Crypt(string.Empty);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.Session, cookie).ToString());
            context.Request.Headers.Add("AAA", new CookieHeaderValue("AAA", "AAA").ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var repo = new CookieHostingAppManager(mockHttpContextAccessor.Object);
            await repo.ClearAllInfoAsync();

            Assert.True(true);
        }
    }
}

using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Moq;
using Passi.Authentication.Cookie.Const;
using Passi.Authentication.Cookie.Extensions;
using Passi.Authentication.Cookie.Repository;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Test.Unit.Fixtures;
using System.Net.Http.Headers;
using System.Web;
using Mocks = Passi.Test.Unit.Fixtures.Mocks;

namespace Passi.Test.Unit.Authentication.Cookie
{
    public class ContactCenterInfoRepositoryTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public ContactCenterInfoRepositoryTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task UpdateAsync_NotImplemented()
        {
            // Arrange
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var repo = new ContactCenterInfoRepository(mockHttpContextAccessor.Object);
            try
            {
                await repo.UpdateAsync(new Passi.Core.Domain.Entities.Info.ContactCenterInfo());
                Assert.True(false);
            }
            catch (NotImplementedException)
            {
                Assert.True(true);
            }
        }

        [Theory]
        [InlineData(1)]
        public async Task RetrieveAsync_Ok(int serviceId)
        {
            // Arrange
            var moks = fixture.Mocks(serviceId);

            (string SCC, string VSU) = moks.ContactCenterInfo.Serialize();

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.ContactCenterSCC, HttpUtility.UrlEncodeUnicode(SCC)).ToString()
                + "," + new CookieHeaderValue(Cookies.ContactCenterVSU, HttpUtility.UrlEncodeUnicode(VSU)).ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var repo = new ContactCenterInfoRepository(mockHttpContextAccessor.Object);

            var output = await repo.RetrieveAsync();

            Assert.NotNull(output);
            Assert.Equal(moks.ContactCenterInfo.UserId, output.UserId);
        }

        [Theory]
        [InlineAutoData]
        public async Task RetrieveAsync_NullContext_NotFound(ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks moks = fixture.Mocks(serviceIdInt);

            (string SCC, string VSU) = moks.ContactCenterInfo.Serialize();

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns((HttpContext?)null);

            ContactCenterInfoRepository repo = new(mockHttpContextAccessor.Object);

            await Assert.ThrowsAsync<NotFoundException>(() => repo.RetrieveAsync());
        }

        [Theory]
        [InlineData(1)]
        public async Task RetrieveAsync_CookieNotFoundError(int serviceId)
        {
            // Arrange
            var moks = fixture.Mocks(serviceId);

            (string SCC, string VSU) = moks.ContactCenterInfo.Serialize();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var repo = new ContactCenterInfoRepository(mockHttpContextAccessor.Object);

            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.ContactCenterVSU, HttpUtility.UrlEncodeUnicode(VSU)).ToString());
            await Assert.ThrowsAsync<NotFoundException>(async () => await repo.RetrieveAsync());

            context.Request.Headers.Remove(Keys.Cookie);
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.ContactCenterSCC, HttpUtility.UrlEncodeUnicode(SCC)).ToString());
            await Assert.ThrowsAsync<NotFoundException>(async () => await repo.RetrieveAsync());
        }

        [Theory]
        [InlineAutoData]
        public async Task RetrieveAsync_IncompleteData_Ok(ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            string scc = "TSEDI001&2030&&Nome Cognome";
            string vsu = "nome=Nome&cognome=Cognome&datnas=27/10/aaaa&email=abc@abc.it&codfis=MRAMHL73R27M955S&param=&sesso=M";

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            DefaultHttpContext context = new();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.ContactCenterSCC, HttpUtility.UrlEncodeUnicode(scc)).ToString()
                + "," + new CookieHeaderValue(Cookies.ContactCenterVSU, HttpUtility.UrlEncodeUnicode(vsu)).ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            ContactCenterInfoRepository repo = new(mockHttpContextAccessor.Object);

            ContactCenterInfo output = await repo.RetrieveAsync();

            Assert.NotNull(output);
        }

    }
}

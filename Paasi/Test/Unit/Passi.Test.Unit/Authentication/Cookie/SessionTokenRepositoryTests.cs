using AutoFixture.Xunit2;
using Bogus;
using Bogus.Extensions.Italy;
using Microsoft.AspNetCore.Http;
using Moq;
using Passi.Authentication.Cookie.Repository;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Test.Unit.Fixtures;
using System.Security.Cryptography;
using Mocks = Passi.Test.Unit.Fixtures.Mocks;

namespace Passi.Test.Unit.Authentication.Cookie
{
    public class SessionTokenRepositoryTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public SessionTokenRepositoryTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData(1)]
        public async Task RetrieveAsync_Ok(int serviceId)
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var person = new Person();

            var now = DateTime.UtcNow;
            now = now.AddMilliseconds(-now.Millisecond);

            var input = new SessionToken
            {
                InstitutionCode = "009",
                LoggedIn = now,
                UserTypeId = RandomNumberGenerator.GetInt32(0, 100),
                SessionId = Guid.NewGuid().ToString(),
                UserId = person.CodiceFiscale(),
                OfficeCode = Guid.NewGuid().ToString(),
                ServiceId = serviceId,
            };

            var toCypher = input.Serialize();

            var cookie = cypher.Crypt(toCypher);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.SessionToken, cookie);
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var mocks = fixture.Mocks(serviceId);
            var sessionRepo = new SessionTokenRepository(mockHttpContextAccessor.Object,
                cypher,
                mocks.ConfigurationOptions);

            var output = await sessionRepo.RetrieveAsync();

            Assert.NotNull(output);
            Assert.True(output.InstitutionCode == input.InstitutionCode);
            Assert.True(output.LoggedIn.ToString() == input.LoggedIn.ToString());
            Assert.True(output.UserTypeId == input.UserTypeId);
            Assert.True(output.SessionId == input.SessionId);
            Assert.True(output.UserId == input.UserId);
            Assert.True(output.ServiceId == input.ServiceId);
            Assert.True(output.OfficeCode == input.OfficeCode);
        }

        [Theory]
        [InlineAutoData]
        public async Task Retrieve_NullContext_Ok(ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            IDataCypherService cypher = PassiFixture.DataCypherUnderTest();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns((HttpContext?)null);
            Mocks mocks = fixture.Mocks(serviceIdInt);
            SessionTokenRepository sessionRepo = new(mockHttpContextAccessor.Object, cypher, mocks.ConfigurationOptions);
            SessionToken output = await sessionRepo.RetrieveAsync();

            Assert.NotNull(output);
        }

        [Theory]
        [InlineData(1)]
        public async Task RetrieveAsync_TokenTooShort_NotFound(int serviceId)
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var toCypher = "a|b|c";

            var cookie = cypher.Crypt(toCypher);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.SessionToken, cookie);
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var mocks = fixture.Mocks(serviceId);
            var sessionRepo = new SessionTokenRepository(mockHttpContextAccessor.Object,
                cypher,
                mocks.ConfigurationOptions);

            var output = await sessionRepo.RetrieveAsync();

            Assert.False(output.IsValid);
        }

        [Theory]
        [InlineData(1)]
        public async Task RetrieveAsync_TokenNotFound_NotFound(int serviceId)
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var mocks = fixture.Mocks(serviceId);
            var sessionRepo = new SessionTokenRepository(mockHttpContextAccessor.Object,
                cypher,
                mocks.ConfigurationOptions);

            var output = await sessionRepo.RetrieveAsync();
            Assert.False(output.IsValid);

        }

        [Theory]
        [InlineData(1)]
        public async Task UpdateAsync_Error_NotImplemented(int serviceId)
        {
            var mocks = fixture.Mocks(serviceId);
            var sessionRepo = new SessionTokenRepository(PassiFixture.AccessorUnderTest(),
                PassiFixture.DataCypherUnderTest(),
                mocks.ConfigurationOptions);

            /// Assert.throws non funziona col notimplemented...strano
            try
            {
                await sessionRepo.UpdateAsync(new SessionToken());
                Assert.True(false);
            }
            catch (NotImplementedException)
            {
                Assert.True(true);
            }
        }
    }
}

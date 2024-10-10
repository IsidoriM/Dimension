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
    public class UserInfoRepositoryTests : IClassFixture<PassiFixture>
    {
        public UserInfoRepositoryTests(PassiFixture _)
        {
        }

        [Fact]
        public async Task RetrieveAsync_CookieTooShort_NotFound()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var cookie = cypher.Crypt(string.Empty);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.Session, cookie).ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var repo = new UserInfoRepository(mockHttpContextAccessor.Object, cypher);

            await Assert.ThrowsAsync<InvalidDataException>(() => repo.RetrieveAsync());
        }

        [Fact]
        public async Task RetrieveAsync_CookieNotFound_NotFound()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var repo = new UserInfoRepository(mockHttpContextAccessor.Object, cypher);

            await Assert.ThrowsAsync<NotFoundException>(() => repo.RetrieveAsync());
        }

        [Fact]
        public async Task RetrieveAsync_Ok()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var person = new Person();

            var now = DateTime.UtcNow;
            now = now.AddMilliseconds(-now.Millisecond);

            var input = new SessionInfo
            {
                AnonymousId = Guid.NewGuid().ToString(),
                AuthenticationType = StringExtensions.RandomString<CommonAuthenticationTypes>(),
                BirthPlaceCode = Guid.NewGuid().ToString(),
                BirthDate = now.AddDays(-1),
                BirthProvince = Guid.NewGuid().ToString(),
                DelegateUserId = Guid.NewGuid().ToString(),
                Email = person.Email,
                InstitutionDescription = Guid.NewGuid().ToString(),
                InstitutionFiscalCode = Guid.NewGuid().ToString(),
                InstitutionCode = "009",
                FiscalCode = person.CodiceFiscale(),
                MultipleProfile = false,
                HasSessionFlag = false,
                InformationCampaign = true,
                IsFromLogin = true,
                IsInfoPrivacyAccepted = true,
                IsPinUnified = true,
                LastAccess = now.AddDays(-1),
                LastPinUpdate = now,
                LastSessionUpdate = now,
                LastUpdated = now,
                LoggedIn = now,
                Mobile = Guid.NewGuid().ToString(),
                Name = person.FirstName,
                Number = Guid.NewGuid().ToString(),
                PEC = Guid.NewGuid().ToString(),
                PECVerificationStatus = PecVerificationStatuses.Validated,
                Phone = Guid.NewGuid().ToString(),
                ProfileTypeId = RandomNumberGenerator.GetInt32(0, 100),
                SessionId = Guid.NewGuid().ToString(),
                SessionMaximumCachingTime = TimeSpan.FromSeconds(10),
                SessionMaximumTime = TimeSpan.FromSeconds(10),
                SessionTimeout = TimeSpan.FromSeconds(RandomNumberGenerator.GetInt32(0, 100)),
                Surname = person.LastName,
                UserId = person.CodiceFiscale(),
                OfficeCode = Guid.NewGuid().ToString(),
            };

            var toCypher = input.Serialize();

            var cookie = cypher.Crypt(toCypher);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.Session, cookie).ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var repo = new UserInfoRepository(mockHttpContextAccessor.Object, cypher);

            var output = await repo.RetrieveAsync();

            Assert.True(output != null);

            Assert.True(output.BirthPlaceCode == input.BirthPlaceCode);
            Assert.True(output.BirthDate.Date.ToMilliseconds() == input.BirthDate.Date.ToMilliseconds());
            Assert.True(output.Email == input.Email);
            Assert.True(output.FiscalCode == input.FiscalCode);
            Assert.True(output.Mobile == input.Mobile);
            Assert.True(output.Name == input.Name);
            Assert.True(output.PEC == input.PEC);
            Assert.True(output.Phone == input.Phone);
            Assert.True(output.Surname == input.Surname);
            Assert.True(output.UserId == input.UserId);
        }

        [Fact]
        public async Task RetrieveAsyncFederatedUser_Ok()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var person = new Person();

            var cf = person.CodiceFiscale();

            var now = DateTime.UtcNow;
            now = now.AddMilliseconds(-now.Millisecond);

            var input = new SessionInfo
            {
                AnonymousId = Guid.NewGuid().ToString(),
                AuthenticationType = StringExtensions.RandomString<CommonAuthenticationTypes>(),
                BirthPlaceCode = Guid.NewGuid().ToString(),
                BirthDate = now.AddDays(-1),
                BirthProvince = Guid.NewGuid().ToString(),
                DelegateUserId = Guid.NewGuid().ToString(),
                Email = person.Email,
                InstitutionDescription = Guid.NewGuid().ToString(),
                InstitutionFiscalCode = Guid.NewGuid().ToString(),
                InstitutionCode = "009",
                FiscalCode = string.Empty,
                MultipleProfile = false,
                HasSessionFlag = false,
                InformationCampaign = true,
                IsFromLogin = true,
                IsInfoPrivacyAccepted = true,
                IsPinUnified = true,
                LastAccess = now.AddDays(-1),
                LastPinUpdate = now,
                LastSessionUpdate = now,
                LastUpdated = now,
                LoggedIn = now,
                Mobile = Guid.NewGuid().ToString(),
                Name = person.FirstName,
                Number = Guid.NewGuid().ToString(),
                PEC = Guid.NewGuid().ToString(),
                PECVerificationStatus = PecVerificationStatuses.Validated,
                Phone = Guid.NewGuid().ToString(),
                ProfileTypeId = RandomNumberGenerator.GetInt32(0, 100),
                SessionId = Guid.NewGuid().ToString(),
                SessionMaximumCachingTime = TimeSpan.FromSeconds(10),
                SessionMaximumTime = TimeSpan.FromSeconds(10),
                SessionTimeout = TimeSpan.FromSeconds(RandomNumberGenerator.GetInt32(0, 100)),
                Surname = person.LastName,
                UserId = $"{cf}:{cf}",
                OfficeCode = Guid.NewGuid().ToString(),
            };

            var toCypher = input.Serialize();

            var cookie = cypher.Crypt(toCypher);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.Session, cookie).ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var repo = new UserInfoRepository(mockHttpContextAccessor.Object, cypher);

            var output = await repo.RetrieveAsync();

            Assert.True(output != null);
            Assert.True(output.FiscalCode == cf);
            Assert.True(output.UserId == cf);
        }

        [Fact]
        public async Task RetrieveAsyncNoInfoPrivacyAccepted_Ok()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var person = new Person();

            var now = DateTime.UtcNow;
            now = now.AddMilliseconds(-now.Millisecond);

            var input = new SessionInfo
            {
                AnonymousId = Guid.NewGuid().ToString(),
                AuthenticationType = StringExtensions.RandomString<CommonAuthenticationTypes>(),
                BirthPlaceCode = Guid.NewGuid().ToString(),
                BirthDate = now.AddDays(-1),
                BirthProvince = Guid.NewGuid().ToString(),
                DelegateUserId = Guid.NewGuid().ToString(),
                Email = person.Email,
                InstitutionDescription = Guid.NewGuid().ToString(),
                InstitutionFiscalCode = Guid.NewGuid().ToString(),
                InstitutionCode = "009",
                FiscalCode = person.CodiceFiscale(),
                MultipleProfile = false,
                HasSessionFlag = false,
                InformationCampaign = true,
                IsFromLogin = true,
                IsInfoPrivacyAccepted = false,
                IsPinUnified = true,
                LastAccess = now.AddDays(-1),
                LastPinUpdate = now,
                LastSessionUpdate = now,
                LastUpdated = now,
                LoggedIn = now,
                Mobile = Guid.NewGuid().ToString(),
                Name = person.FirstName,
                Number = Guid.NewGuid().ToString(),
                PEC = Guid.NewGuid().ToString(),
                PECVerificationStatus = PecVerificationStatuses.Validated,
                Phone = Guid.NewGuid().ToString(),
                ProfileTypeId = RandomNumberGenerator.GetInt32(0, 100),
                SessionId = Guid.NewGuid().ToString(),
                SessionMaximumCachingTime = TimeSpan.FromSeconds(10),
                SessionMaximumTime = TimeSpan.FromSeconds(10),
                SessionTimeout = TimeSpan.FromSeconds(RandomNumberGenerator.GetInt32(0, 100)),
                Surname = person.LastName,
                UserId = person.CodiceFiscale(),
                OfficeCode = Guid.NewGuid().ToString(),
            };

            var toCypher = input.Serialize();

            var cookie = cypher.Crypt(toCypher);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.Session, cookie).ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var repo = new UserInfoRepository(mockHttpContextAccessor.Object, cypher);

            var output = await repo.RetrieveAsync();

            Assert.True(output != null);

            Assert.True(output.BirthPlaceCode == input.BirthPlaceCode);
            Assert.True(output.BirthDate.Date.ToMilliseconds() == input.BirthDate.Date.ToMilliseconds());
            Assert.True(output.Email == string.Empty);
            Assert.True(output.FiscalCode == input.FiscalCode);
            Assert.True(output.Mobile == string.Empty);
            Assert.True(output.Name == input.Name);
            Assert.True(output.PEC == string.Empty);
            Assert.True(output.Phone == string.Empty);
            Assert.True(output.Surname == input.Surname);
            Assert.True(output.UserId == input.UserId);
        }

        [Fact]
        public async Task RetrieveAsync_NullContext_NotFound()
        {
            // Arrange
            IDataCypherService cypher = PassiFixture.DataCypherUnderTest();

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns((HttpContext?)null);

            UserInfoRepository repo = new(mockHttpContextAccessor.Object, cypher);

            await Assert.ThrowsAsync<NotFoundException>(() => repo.RetrieveAsync());
        }

        [Fact]
        public async Task UpdateAsync_NotImplemented()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var repo = new UserInfoRepository(mockHttpContextAccessor.Object, cypher);
            try
            {
                await repo.UpdateAsync(new UserInfo());
                Assert.True(false);
            }
            catch (NotImplementedException)
            {
                Assert.True(true);
            }
        }

        [Theory]
        [InlineAutoData("|||||||||||||||||||||||||||||||true||1||28/01/202a|||||")]
        [InlineAutoData("|||||||||||||||||||||||||||||||true||3||28/01/202a|||||")]
        public async Task RetrieveAsync_ParticularData_Ok(string toCypher)
        {
            // Arrange
            IDataCypherService cypher = PassiFixture.DataCypherUnderTest();
            string cookie = cypher.Crypt(toCypher);
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            DefaultHttpContext context = new();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.Session, cookie).ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
            UserInfoRepository repo = new(mockHttpContextAccessor.Object, cypher);

            UserInfo output = await repo.RetrieveAsync();

            Assert.NotNull(output);
        }

    }
}

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
    public class SessionInfoRepositoryTests : IClassFixture<PassiFixture>
    {
        public SessionInfoRepositoryTests(PassiFixture _)
        {
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

            var userRepo = new UserInfoRepository(mockHttpContextAccessor.Object, cypher);
            var sessionRepo = new SessionInfoRepository(mockHttpContextAccessor.Object, cypher, userRepo);

            var output = await sessionRepo.RetrieveAsync();

            Assert.True(output != null);
            Assert.True(output.AnonymousId == input.AnonymousId);
            Assert.True(output.AuthenticationType == input.AuthenticationType);
            Assert.True(output.BirthPlaceCode == input.BirthPlaceCode);
            Assert.True(output.BirthDate.Date.ToMilliseconds() == input.BirthDate.Date.ToMilliseconds());
            Assert.True(output.DelegateUserId == input.DelegateUserId);
            Assert.True(output.Email == input.Email);
            Assert.True(output.InstitutionDescription == input.InstitutionDescription);
            Assert.True(output.InstitutionFiscalCode == input.InstitutionFiscalCode);
            Assert.True(output.InstitutionCode == input.InstitutionCode);
            Assert.True(output.FiscalCode == input.FiscalCode);
            Assert.True(output.MultipleProfile == input.MultipleProfile);
            Assert.True(output.HasSessionFlag == input.HasSessionFlag);
            Assert.True(output.InformationCampaign == input.InformationCampaign);
            Assert.True(output.IsFromLogin == input.IsFromLogin);
            Assert.True(output.IsInfoPrivacyAccepted == input.IsInfoPrivacyAccepted);
            Assert.True(output.IsPinUnified == input.IsPinUnified);
            Assert.True(output.LastAccess.ToMilliseconds() == input.LastAccess.ToMilliseconds());
            Assert.True(output.LastPinUpdate.ToMilliseconds() == input.LastPinUpdate.ToMilliseconds());
            Assert.True(output.LastSessionUpdate.ToMilliseconds() == input.LastSessionUpdate.ToMilliseconds());
            Assert.True(output.LastUpdated.ToMilliseconds() == input.LastUpdated.ToMilliseconds());
            Assert.True(output.LoggedIn.ToMilliseconds() == input.LoggedIn.ToMilliseconds());
            Assert.True(output.Mobile == input.Mobile);
            Assert.True(output.Name == input.Name);
            Assert.True(output.Number == input.Number);
            Assert.True(output.PEC == input.PEC);
            Assert.True(output.PECVerificationStatus == input.PECVerificationStatus);
            Assert.True(output.Phone == input.Phone);
            Assert.True(output.ProfileTypeId == input.ProfileTypeId);
            Assert.True(output.SessionId == input.SessionId);
            Assert.True(output.SessionMaximumCachingTime == input.SessionMaximumCachingTime);
            Assert.True(output.SessionMaximumTime == input.SessionMaximumTime);
            Assert.True(output.SessionTimeout == input.SessionTimeout);
            Assert.True(output.Surname == input.Surname);
            Assert.True(output.UserId == input.UserId);
            Assert.True(output.OfficeCode == input.OfficeCode);
        }

        [Fact]
        public async Task Retrieve_NullContext_NotFound()
        {
            // Arrange
            IDataCypherService cypher = PassiFixture.DataCypherUnderTest();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns((HttpContext?)null);
            UserInfoRepository userRepo = new(mockHttpContextAccessor.Object, cypher);
            SessionInfoRepository sessionRepo = new(mockHttpContextAccessor.Object, cypher, userRepo);

            await Assert.ThrowsAsync<NotFoundException>(() => sessionRepo.RetrieveAsync());
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

            var userRepo = new UserInfoRepository(mockHttpContextAccessor.Object, cypher);
            var sessionRepo = new SessionInfoRepository(mockHttpContextAccessor.Object, cypher, userRepo);

            var output = sessionRepo.RetrieveAsync();

            await Assert.ThrowsAsync<InvalidDataException>(() => output);
        }

        [Fact]
        public async Task RetrieveAsync_CookieNotFound_NotFound()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var userRepo = new UserInfoRepository(mockHttpContextAccessor.Object, cypher);
            var sessionRepo = new SessionInfoRepository(mockHttpContextAccessor.Object, cypher, userRepo);

            var output = sessionRepo.RetrieveAsync();

            await Assert.ThrowsAsync<NotFoundException>(() => output);
        }

        [Fact]
        public async Task UpdateAsync_Ok()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var person = new Person();

            var now = DateTime.UtcNow;
            now = now.AddMilliseconds(-now.Millisecond);

            SessionInfo input = new()
            {
                LastUpdated = now,
                ProfileTypeId = RandomNumberGenerator.GetInt32(0, 100),
                SessionId = Guid.NewGuid().ToString(),
                UserId = person.CodiceFiscale()
            };

            var toCypher = input.Serialize();

            var cookie = cypher.Crypt(toCypher);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.Session, cookie).ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var userRepo = new UserInfoRepository(mockHttpContextAccessor.Object, cypher);
            var sessionRepo = new SessionInfoRepository(mockHttpContextAccessor.Object, cypher, userRepo);

            await Task.Delay(1000);
            var output = await sessionRepo.UpdateAsync(input);

            Assert.True(output != null);
            Assert.True(output.LastUpdated > now);
            Assert.True(output.UserId == input.UserId);
            Assert.True(output.ProfileTypeId == input.ProfileTypeId);
            Assert.True(output.SessionId == input.SessionId);
        }

        [Fact]
        public async Task Update_NullContext_Ok()
        {
            // Arrange
            IDataCypherService cypher = PassiFixture.DataCypherUnderTest();
            SessionInfo input = new();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns((HttpContext?)null);
            UserInfoRepository userRepo = new(mockHttpContextAccessor.Object, cypher);
            SessionInfoRepository sessionRepo = new(mockHttpContextAccessor.Object, cypher, userRepo);

            SessionInfo output = await sessionRepo.UpdateAsync(input);

            Assert.NotNull(output);
        }
    }
}

using AutoFixture.Xunit2;
using Moq;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Test.Unit.Fixtures;
using Mocks = Passi.Test.Unit.Fixtures.Mocks;

namespace Passi.Test.Unit.Core.AuthServices.Web
{
    public class SessionTokenTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public SessionTokenTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }


        [Theory]
        [InlineData(1)]
        public async Task NoToken_Unauthorized_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            IPassiAuthenticationService service = mocks.PackApiAuthService();

            mocks.SessionTokenRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(new SessionToken());

            var result = service.IsAuthorizedAsync(serviceId);

            var ex = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => result);
            //Assert
            Assert.Equal(Reason.SessionTokenNotFound, ex.Reason);
        }

        [Theory]
        [InlineData(1)]
        public async Task DifferentUserIds_Unauthorized_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            IPassiAuthenticationService service = mocks.PackApiAuthService();

            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(new SessionInfo()
            {
                UserId = Guid.NewGuid().ToString(),
            });
            mocks.SessionTokenRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(new SessionToken()
            {
                UserId = Guid.NewGuid().ToString(),
            });

            var result = service.IsAuthorizedAsync(serviceId);

            var ex = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => result);
            //Assert
            Assert.Equal(Reason.SessionUserIdDifferentProfileUserId, ex.Reason);
        }

        [Theory]
        [InlineData(1)]
        public async Task DifferentUserData_Unauthorized_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            IPassiAuthenticationService service = mocks.PackApiAuthService();

            var userId = Guid.NewGuid().ToString();

            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(new SessionInfo()
            {
                UserId = userId,
                ProfileTypeId = 1
            });
            mocks.SessionTokenRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(new SessionToken()
            {
                UserId = userId,
                UserTypeId = 2
            });

            var result = service.IsAuthorizedAsync(serviceId);

            var ex = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => result);
            //Assert
            Assert.Equal(Reason.SessionUserIdDifferentProfileUserId, ex.Reason);
        }

        [Theory]
        [InlineAutoData]
        public async Task NoContext_Ok(ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            IPassiAuthenticationService service = mocks.PackApiAuthService(true);

            mocks.SessionTokenRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(new SessionToken());

            SessionInfo session = await service.IsAuthorizedAsync(serviceIdInt);

            Assert.NotNull(session);
            Assert.NotEmpty(session.SessionId);
        }

        [Theory]
        [InlineAutoData]
        public async Task IsAuthorized_Ok(ushort serviceId, string userId, int userTypeId, string officeCode)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            mocks.SessionInfo.UserId = userId;
            mocks.SessionInfo.ProfileTypeId = userTypeId;
            mocks.SessionInfo.OfficeCode = officeCode;
            mocks.SessionToken.UserId = userId;
            mocks.SessionToken.UserTypeId = userTypeId;
            mocks.SessionToken.OfficeCode = officeCode;
            mocks.ProfileInfo.FiscalCode = userId;

            IPassiAuthenticationService service = mocks.PackApiAuthService();

            SessionInfo session = await service.IsAuthorizedAsync(serviceIdInt);

            Assert.NotNull(session);
            Assert.NotEmpty(session.SessionId);
        }

        [Theory]
        [InlineAutoData(1, 0, "", "")]
        [InlineAutoData(0, 1, "", "")]
        [InlineAutoData(0, 0, "a", "")]
        [InlineAutoData(0, 0, "", "b")]
        public async Task IsAuthorizedInvalidSessionToken_Unauthorized(int userTypeIdDifference, int loggedInDifference, string officeCodeDifference, string institutionCodeDifference, ushort serviceId, string userId, int userTypeId, string officeCode)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            DateTime now = DateTime.UtcNow;
            mocks.SessionInfo.UserId = userId;
            mocks.SessionInfo.ProfileTypeId = userTypeId;
            mocks.SessionInfo.OfficeCode = officeCode;
            mocks.SessionInfo.LoggedIn = now;
            mocks.SessionToken.UserId = userId;
            mocks.SessionToken.UserTypeId = userTypeId + userTypeIdDifference;
            mocks.SessionToken.OfficeCode = $"{officeCode}{officeCodeDifference}";
            mocks.SessionToken.InstitutionCode = $"{mocks.SessionInfo.InstitutionCode}{institutionCodeDifference}";
            mocks.SessionToken.LoggedIn = now.AddSeconds(loggedInDifference);
            mocks.ProfileInfo.FiscalCode = userId;

            IPassiAuthenticationService service = mocks.PackApiAuthService();

            Task<SessionInfo> result = service.IsAuthorizedAsync(serviceIdInt);
            PassiUnauthorizedException ex = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => result);
            // Assert
            Assert.Equal(Reason.InvalidSessionToken, ex.Reason);
        }

        [Theory]
        [InlineAutoData]
        public async Task IsAuthorizedDifferentIds_Unauthorized(ushort serviceId, string userId, int userTypeId, string officeCode)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            mocks.SessionInfo.UserId = userId;
            mocks.SessionInfo.ProfileTypeId = userTypeId;
            mocks.SessionInfo.OfficeCode = officeCode;
            mocks.SessionToken.UserId = $"{userId}_a";
            mocks.SessionToken.UserTypeId = userTypeId;
            mocks.SessionToken.OfficeCode = officeCode;
            mocks.ProfileInfo.FiscalCode = userId;

            IPassiAuthenticationService service = mocks.PackApiAuthService();

            Task<SessionInfo> result = service.IsAuthorizedAsync(serviceIdInt);
            PassiUnauthorizedException ex = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => result);
            // Assert
            Assert.Equal(Reason.SessionUserIdDifferentProfileUserId, ex.Reason);
        }
    }
}

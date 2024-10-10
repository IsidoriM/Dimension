using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Moq;
using Passi.Authentication.Cookie.Const;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Services;
using Passi.Test.Unit.Fixtures;
using System.Web;
using Mocks = Passi.Test.Unit.Fixtures.Mocks;

namespace Passi.Test.Unit.Core
{
    public class CLogServiceTests : IClassFixture<PassiFixture>
    {
        private readonly Mock<ICLogRepository> repoMock;
        private readonly PassiFixture fixture;

        public CLogServiceTests(PassiFixture fixture)
        {
            repoMock = new Mock<ICLogRepository>();
            repoMock.Setup(s => s.LogAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>()
                )).Returns(Task.CompletedTask);

            this.fixture = fixture;
        }

        [Theory]
        [InlineAutoData]
        [InlineAutoData("", "", "")]
        public async Task LogAsyncUserLogged_Ok(string delegateUserId, string institutionCode, string officeCode)
        {
            Mocks mocks = fixture.Mocks(1);
            mocks.SessionInfo.DelegateUserId = delegateUserId;
            mocks.SessionInfo.InstitutionCode = institutionCode;
            mocks.SessionInfo.OfficeCode = officeCode;
            CLogService currentClogService = new(
                repoMock.Object,
                PassiFixture.AccessorUnderTest(),
                mocks.SessionRepo.Object,
                mocks.ContactCenterRepo.Object
                );
            await currentClogService.LogAsync(0, 200, new Dictionary<string, string>());
            Assert.True(true);
        }

        [Fact]
        public async Task LogAsyncUserLoggedFromCC_Ok()
        {
            Mocks mocks = fixture.Mocks(1);
            mocks.SessionInfo.ProfileTypeId = SpecialProfiles.ContactCenter;
            mocks.ProfileInfo.ProfileTypeId = SpecialProfiles.ContactCenter;

            var _cLogService = new CLogService(
                repoMock.Object,
                PassiFixture.AccessorUnderTest(),
                mocks.SessionRepo.Object,
                mocks.ContactCenterRepo.Object
                );
            await _cLogService.LogAsync(0, 200, new Dictionary<string, string>());
            Assert.True(true);
        }

        [Fact]
        public async Task LogAsyncUserLogged_Error()
        {
            Mock<IInfoRepository<SessionInfo>> sessionInfoMock = new();
            sessionInfoMock.Setup(s => s.RetrieveAsync()).ThrowsAsync(new NotFoundException());
            Mock<IInfoRepository<ContactCenterInfo>> contactCenterInfoMock = new();
            ICLogService cLogServiceWithError = new CLogService(
                repoMock.Object,
                PassiFixture.AccessorUnderTest(),
                sessionInfoMock.Object,
                contactCenterInfoMock.Object
                );
            var task = cLogServiceWithError.LogAsync(0, 200, new Dictionary<string, string>());
            await Assert.ThrowsAsync<CLogException>(async () => await task);
        }

        [Fact]
        public async Task LogAsyncUserLogged_ContactCenterInfoNotFound_Ok()
        {
            Mocks mocks = fixture.Mocks(1);
            Mock<IInfoRepository<ContactCenterInfo>> contactCenterInfoMock = new(MockBehavior.Strict);
            contactCenterInfoMock.Setup(c => c.RetrieveAsync()).ThrowsAsync(new NotFoundException());
            ICLogService cLogServiceWithError = new CLogService(
                repoMock.Object,
                PassiFixture.AccessorUnderTest(),
                mocks.SessionRepo.Object,
                contactCenterInfoMock.Object
                );
            await cLogServiceWithError.LogAsync(0, 200, new Dictionary<string, string>());
            Assert.True(true);
        }

        [Fact]
        public async Task LogAsyncUserLogged_AccessorNull_Ok()
        {
            Mocks mocks = fixture.Mocks(1);
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new(MockBehavior.Strict);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            ICLogService cLogServiceWithError = new CLogService(
                repoMock.Object,
                mockHttpContextAccessor.Object,
                mocks.SessionRepo.Object,
                mocks.ContactCenterRepo.Object
                );
            await cLogServiceWithError.LogAsync(0, 200, new Dictionary<string, string>());
            Assert.True(true);
        }

        [Fact]
        public async Task LogAsyncUserLogged_RemoteIpNull_Ok()
        {
            Mocks mocks = fixture.Mocks(1);
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new(MockBehavior.Strict);
            DefaultHttpContext context = new();
            context.Connection.RemoteIpAddress = null;
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);
            ICLogService cLogServiceWithError = new CLogService(
                repoMock.Object,
                mockHttpContextAccessor.Object,
                mocks.SessionRepo.Object,
                mocks.ContactCenterRepo.Object
                );
            await cLogServiceWithError.LogAsync(0, 200, new Dictionary<string, string>());
            Assert.True(true);
        }
    }
}

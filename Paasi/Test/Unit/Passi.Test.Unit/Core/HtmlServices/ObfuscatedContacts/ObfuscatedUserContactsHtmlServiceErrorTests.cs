using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http;
using Moq;
using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Services;
using Passi.Test.Unit.Fixtures;
using Mocks = Passi.Test.Unit.Fixtures.Mocks;

namespace Passi.Test.Unit.Core.HtmlServices.Contacts
{
    public class ObfuscatedUserContactsHtmlServiceErrorTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public ObfuscatedUserContactsHtmlServiceErrorTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task ObfuscatedContactsAsync_Unauthorized_Error()
        {
            //Arrange
            Mock<IInfoRepository<UserInfo>> userInfoRepoMock = new();
            Mock<IInfoRepository<SessionInfo>> sessionInfoRepoMock = new();
            Mock<IInfoRepository<ContactCenterInfo>> ccRepoMock = new();
            Mock<IUserRepository> userRepoMock = new();

            SessionInfo si = new()
            {
                ProfileTypeId = SpecialProfiles.ContactCenter
            };

            string cf = Guid.NewGuid().ToString();
            UserInfo ui = new()
            {
                UserId = cf,
                FiscalCode = cf
            };

            sessionInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(si);
            userInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(ui);
            userRepoMock.Setup(x => x.UserAsync(cf, CommonAuthenticationTypes.Undefined, string.Empty)).ThrowsAsync(new ArgumentException(string.Empty));
            ccRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(new ContactCenterInfo() { UserId = cf });

            PassiUserContactsService service = new(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            ContactsException cex = await Assert.ThrowsAsync<ContactsException>(() => service.UserContactsAsync(cf));
            Assert.Equal(Outcomes.AUC007, cex.Outcome);
        }

        [Fact]
        public async Task ObfuscatedContactsAsync_UserNoPrivacy_Error()
        {
            //Arrange
            Mock<IInfoRepository<UserInfo>> userInfoRepoMock = new();
            Mock<IInfoRepository<SessionInfo>> sessionInfoRepoMock = new();
            Mock<IInfoRepository<ContactCenterInfo>> ccRepoMock = new();
            Mock<IUserRepository> userRepoMock = new();

            SessionInfo si = new()
            {
                ProfileTypeId = SpecialProfiles.ContactCenter
            };

            string cf = Guid.NewGuid().ToString();
            UserInfo ui = new()
            {
                UserId = cf,
                FiscalCode = cf
            };

            sessionInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(si);
            userInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(ui);
            userRepoMock.Setup(x => x.UserAsync(cf, CommonAuthenticationTypes.Undefined, string.Empty)).ReturnsAsync(ui);
            ccRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(new ContactCenterInfo() { UserId = cf });

            PassiUserContactsService service = new(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            ContactsException cex = await Assert.ThrowsAsync<ContactsException>(() => service.UserContactsAsync(cf));
            Assert.Equal(Outcomes.AUC007, cex.Outcome);
        }

        [Theory]
        [InlineData(1)]
        public async Task ObfuscatedContactsAsync_NotFound_Error(int serviceId)
        {

            //Arrange
            SessionInfo si = new();

            string cf = Guid.NewGuid().ToString();
            UserInfo ui = new()
            {
                UserId = string.Empty,
                FiscalCode = string.Empty,
            };

            Mocks mocks = fixture.Mocks(serviceId);
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(si);
            mocks.UserInfoRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(ui);
            mocks.UserRepo.Setup(x => x.UserAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(ui);
            mocks.ContactCenterRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(new ContactCenterInfo() { UserId = cf });

            PassiUserContactsService service = new(
                PassiFixture.AccessorUnderTest(),
                mocks.SessionRepo.Object,
                mocks.UserInfoRepo.Object,
                mocks.ContactCenterRepo.Object,
                mocks.UserRepo.Object);

            ContactsException cex = await Assert.ThrowsAsync<ContactsException>(() => service.UserContactsAsync(cf));
            Assert.Contains("non ha contatti", cex.Message);
        }

        [Fact]
        public async Task ObfuscatedContactsAsync_Generic_Error()
        {
            //Arrange
            Mock<IInfoRepository<UserInfo>> userInfoRepoMock = new();
            Mock<IInfoRepository<SessionInfo>> sessionInfoRepoMock = new();
            Mock<IInfoRepository<ContactCenterInfo>> ccRepoMock = new();
            Mock<IUserRepository> userRepoMock = new();

            SessionInfo si = new();

            string cf = Guid.NewGuid().ToString();
            UserInfo ui = new()
            {
                UserId = cf,
                FiscalCode = cf
            };

            sessionInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(si);
            userInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(ui);
            userRepoMock.Setup(x => x.UserAsync(cf, CommonAuthenticationTypes.Undefined, string.Empty)).ThrowsAsync(new ArgumentException(string.Empty));
            ccRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(new ContactCenterInfo() { UserId = cf });

            PassiUserContactsService service = new(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            ContactsException cex = await Assert.ThrowsAsync<ContactsException>(() => service.UserContactsAsync(cf));
            Assert.Equal(Outcomes.AUC005, cex.Outcome);
        }

        [Theory]
        [InlineAutoData]
        public async Task ObfuscatedContactsAsync_NoContext_Error(string fiscalCode)
        {
            // Arrange
            Mock<IInfoRepository<UserInfo>> userInfoRepoMock = new();
            Mock<IInfoRepository<SessionInfo>> sessionInfoRepoMock = new();
            Mock<IInfoRepository<ContactCenterInfo>> ccRepoMock = new();
            Mock<IUserRepository> userRepoMock = new();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();

            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            PassiUserContactsService service = new(
                mockHttpContextAccessor.Object,
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            ContactsException cex = await Assert.ThrowsAsync<ContactsException>(() => service.UserContactsAsync(fiscalCode));
            Assert.Equal(Outcomes.AUC006, cex.Outcome);
        }

        [Theory]
        [InlineAutoData]
        public async Task ObfuscatedContactsAsync_UserNotFound_Error(string fiscalCode)
        {
            // Arrange
            fiscalCode = fiscalCode.ToUpper();
            Mock<IInfoRepository<UserInfo>> userInfoRepoMock = new();
            Mock<IInfoRepository<SessionInfo>> sessionInfoRepoMock = new();
            Mock<IInfoRepository<ContactCenterInfo>> ccRepoMock = new();
            Mock<IUserRepository> userRepoMock = new();
            SessionInfo si = new();
            UserInfo ui = new()
            {
                UserId = fiscalCode,
                FiscalCode = fiscalCode
            };

            sessionInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(si);
            userInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(ui);
            userRepoMock.Setup(x => x.UserAsync(fiscalCode, CommonAuthenticationTypes.Undefined, string.Empty)).ThrowsAsync(new NotFoundException());
            ccRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(new ContactCenterInfo() { UserId = fiscalCode });

            PassiUserContactsService service = new(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            ContactsException cex = await Assert.ThrowsAsync<ContactsException>(() => service.UserContactsAsync(fiscalCode));
            Assert.Equal(Outcomes.Two, cex.Outcome);
        }

        [Theory]
        [InlineAutoData]
        public async Task ObfuscatedContactsAsync_UserGenericError_Error(string fiscalCode)
        {
            // Arrange
            fiscalCode = fiscalCode.ToUpper();
            Mock<IInfoRepository<UserInfo>> userInfoRepoMock = new();
            Mock<IInfoRepository<SessionInfo>> sessionInfoRepoMock = new();
            Mock<IInfoRepository<ContactCenterInfo>> ccRepoMock = new();
            Mock<IUserRepository> userRepoMock = new();
            SessionInfo si = new();
            UserInfo ui = new()
            {
                UserId = fiscalCode,
                FiscalCode = fiscalCode
            };

            sessionInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(si);
            userInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(ui);
            userRepoMock.Setup(x => x.UserAsync(fiscalCode, CommonAuthenticationTypes.Undefined, string.Empty)).ThrowsAsync(new Exception());
            ccRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(new ContactCenterInfo() { UserId = fiscalCode });

            PassiUserContactsService service = new(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            ContactsException cex = await Assert.ThrowsAsync<ContactsException>(() => service.UserContactsAsync(fiscalCode));
            Assert.Equal(Outcomes.AUC005, cex.Outcome);
        }


    }
}

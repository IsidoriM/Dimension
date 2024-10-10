using Microsoft.AspNetCore.Http;
using Moq;
using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Services;
using Passi.Test.Unit.Fixtures;


namespace Passi.Test.Unit.Core.HtmlServices.Contacts
{
    public class UserContactsHtmlServiceErrorTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public UserContactsHtmlServiceErrorTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task ContactsAsync_NoRequest_Error()
        {

            //Arrange
            var userInfoRepoMock = new Mock<IInfoRepository<UserInfo>>();
            var sessionInfoRepoMock = new Mock<IInfoRepository<SessionInfo>>();
            var ccRepoMock = new Mock<IInfoRepository<ContactCenterInfo>>();
            var userRepoMock = new Mock<IUserRepository>();

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var service = new PassiUserContactsService(
                mockHttpContextAccessor.Object,
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            try
            {
                await service.UserContactsAsync();
            }
            catch (ContactsException cex)
            {
                Assert.Equal(Outcomes.AUC006, cex.Outcome);
            }
        }

        [Fact]
        public async Task ContactsAsync_GenericException_Error()
        {

            //Arrange
            var userInfoRepoMock = new Mock<IInfoRepository<UserInfo>>();
            var sessionInfoRepoMock = new Mock<IInfoRepository<SessionInfo>>();
            var ccRepoMock = new Mock<IInfoRepository<ContactCenterInfo>>();
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(s => s.UserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());

            var service = new PassiUserContactsService(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            try
            {
                await service.UserContactsAsync();
            }
            catch (ContactsException cex)
            {
                Assert.Equal(Outcomes.AUC005, cex.Outcome);
            }
        }

        [Fact]
        public async Task ContactsAsync_NoUserInfo_Error()
        {

            //Arrange
            var userInfoRepoMock = new Mock<IInfoRepository<UserInfo>>();
            var sessionInfoRepoMock = new Mock<IInfoRepository<SessionInfo>>();
            var ccRepoMock = new Mock<IInfoRepository<ContactCenterInfo>>();
            var userRepoMock = new Mock<IUserRepository>();
            userInfoRepoMock.Setup(s => s.RetrieveAsync()).ThrowsAsync(new NotFoundException());

            var service = new PassiUserContactsService(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);
            try
            {
                await service.UserContactsAsync();
            }
            catch (ContactsException cex)
            {
                Assert.Equal(Outcomes.AUC001, cex.Outcome);
            }
        }

        [Fact]
        public async Task ContactsAsync_NoUserInfoGeneric_Error()
        {
            //Arrange
            var userInfoRepoMock = new Mock<IInfoRepository<UserInfo>>();
            var sessionInfoRepoMock = new Mock<IInfoRepository<SessionInfo>>();
            var ccRepoMock = new Mock<IInfoRepository<ContactCenterInfo>>();
            var userRepoMock = new Mock<IUserRepository>();
            userInfoRepoMock.Setup(s => s.RetrieveAsync()).ThrowsAsync(new Exception());

            var service = new PassiUserContactsService(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            try
            {
                await service.UserContactsAsync();
            }
            catch (ContactsException cex)
            {
                Assert.Equal(Outcomes.AUC002, cex.Outcome);
            }
        }

        [Fact]
        public async Task ContactsAsync_NotFound_Error()
        {

            //Arrange
            var userInfoRepoMock = new Mock<IInfoRepository<UserInfo>>();
            var sessionInfoRepoMock = new Mock<IInfoRepository<SessionInfo>>();
            var ccRepoMock = new Mock<IInfoRepository<ContactCenterInfo>>();
            var userRepoMock = new Mock<IUserRepository>();

            sessionInfoRepoMock.Setup(x => x.RetrieveAsync()).ThrowsAsync(new NotFoundException());

            var service = new PassiUserContactsService(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            try
            {
                await service.UserContactsAsync();
            }
            catch (ContactsException cex)
            {
                Assert.Equal(Outcomes.AUC001, cex.Outcome);
            }
        }

        [Fact]
        public async Task ContactsAsync_Generic_Error()
        {
            //Arrange
            var userInfoRepoMock = new Mock<IInfoRepository<UserInfo>>();
            var sessionInfoRepoMock = new Mock<IInfoRepository<SessionInfo>>();
            var ccRepoMock = new Mock<IInfoRepository<ContactCenterInfo>>();
            var userRepoMock = new Mock<IUserRepository>();

            sessionInfoRepoMock.Setup(x => x.RetrieveAsync()).ThrowsAsync(new ArgumentException(string.Empty));

            var service = new PassiUserContactsService(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            try
            {
                await service.UserContactsAsync();
            }
            catch (ContactsException cex)
            {
                Assert.Equal(Outcomes.AUC002, cex.Outcome);
            }
        }

        [Fact]
        public async Task ContactsAsync_ContactCenterUserNotFound_Error()
        {
            //Arrange
            var userInfoRepoMock = new Mock<IInfoRepository<UserInfo>>();
            var sessionInfoRepoMock = new Mock<IInfoRepository<SessionInfo>>();
            var ccRepoMock = new Mock<IInfoRepository<ContactCenterInfo>>();
            var userRepoMock = new Mock<IUserRepository>();

            var si = new SessionInfo();
            si.ProfileTypeId = SpecialProfiles.ContactCenter;
            var ui = new UserInfo();
            sessionInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(si);
            userInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(ui);
            ccRepoMock.Setup(x => x.RetrieveAsync()).ThrowsAsync(new NotFoundException());

            var service = new PassiUserContactsService(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            try
            {
                await service.UserContactsAsync();
            }
            catch (ContactsException cex)
            {
                Assert.Equal(Outcomes.AUC004, cex.Outcome);
            }
        }

        [Fact]
        public async Task ContactsAsync_ContactCenterGeneric_Error()
        {
            //Arrange
            var userInfoRepoMock = new Mock<IInfoRepository<UserInfo>>();
            var sessionInfoRepoMock = new Mock<IInfoRepository<SessionInfo>>();
            var ccRepoMock = new Mock<IInfoRepository<ContactCenterInfo>>();
            var userRepoMock = new Mock<IUserRepository>();

            var si = new SessionInfo();
            si.ProfileTypeId = SpecialProfiles.ContactCenter;
            var ui = new UserInfo();
            sessionInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(si);
            userInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(ui);
            ccRepoMock.Setup(x => x.RetrieveAsync()).ThrowsAsync(new ArgumentException(string.Empty));

            var service = new PassiUserContactsService(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            try
            {
                await service.UserContactsAsync();
            }
            catch (ContactsException cex)
            {
                Assert.Equal(Outcomes.AUC003, cex.Outcome);
            }
        }

        [Fact]
        public async Task ContactsAsync_UserNoCF_Error()
        {
            //Arrange
            var userInfoRepoMock = new Mock<IInfoRepository<UserInfo>>();
            var sessionInfoRepoMock = new Mock<IInfoRepository<SessionInfo>>();
            var ccRepoMock = new Mock<IInfoRepository<ContactCenterInfo>>();
            var userRepoMock = new Mock<IUserRepository>();

            var si = new SessionInfo();
            si.ProfileTypeId = SpecialProfiles.ContactCenter;

            var cf = string.Empty;
            var ui = new UserInfo();
            ui.UserId = cf;
            ui.FiscalCode = cf;

            sessionInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(si);
            userInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(ui);
            userRepoMock.Setup(x => x.UserAsync(cf, CommonAuthenticationTypes.Undefined, string.Empty)).ReturnsAsync(ui);
            ccRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(new ContactCenterInfo() { UserId = cf });

            var service = new PassiUserContactsService(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            try
            {
                await service.UserContactsAsync();
            }
            catch (ContactsException cex)
            {
                Assert.Equal(Outcomes.Two, cex.Outcome);
            }
        }

        [Fact]
        public async Task ContactsAsync_UserNoPrivacy_Error()
        {
            //Arrange
            var userInfoRepoMock = new Mock<IInfoRepository<UserInfo>>();
            var sessionInfoRepoMock = new Mock<IInfoRepository<SessionInfo>>();
            var ccRepoMock = new Mock<IInfoRepository<ContactCenterInfo>>();
            var userRepoMock = new Mock<IUserRepository>();

            var si = new SessionInfo();
            si.ProfileTypeId = SpecialProfiles.ContactCenter;

            var cf = Guid.NewGuid().ToString();
            var ui = new UserInfo();
            ui.UserId = cf;
            ui.FiscalCode = cf;

            sessionInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(si);
            userInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(ui);
            userRepoMock.Setup(x => x.UserAsync(cf, CommonAuthenticationTypes.Undefined, string.Empty)).ReturnsAsync(ui);
            ccRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(new ContactCenterInfo() { UserId = cf, FiscalCode = cf });

            var service = new PassiUserContactsService(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            try
            {
                await service.UserContactsAsync();
            }
            catch (ContactsException cex)
            {
                Assert.Contains("non ha preso visione", cex.Message);
            }
        }
    }
}

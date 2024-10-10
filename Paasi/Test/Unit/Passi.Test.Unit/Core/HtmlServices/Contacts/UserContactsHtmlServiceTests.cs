using Moq;
using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Services;
using Passi.Test.Unit.Fixtures;


namespace Passi.Test.Unit.Core.HtmlServices.Contacts
{
    public class UserContactsHtmlServiceTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public UserContactsHtmlServiceTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task ContactsAsync_OwnProfile_Ok()
        {
            //Arrange
            var userInfoRepoMock = new Mock<IInfoRepository<UserInfo>>();
            var sessionInfoRepoMock = new Mock<IInfoRepository<SessionInfo>>();
            var ccRepoMock = new Mock<IInfoRepository<ContactCenterInfo>>();
            var userRepoMock = new Mock<IUserRepository>();

            var si = new SessionInfo()
            {
                ProfileTypeId = SpecialProfiles.Citizen,
                PECVerificationStatus = PecVerificationStatuses.Validated,
                IsInfoPrivacyAccepted = true
            };

            var cf = Guid.NewGuid().ToString();
            var ui = new UserInfo
            {
                UserId = cf,
                FiscalCode = cf,
                Email = Guid.NewGuid().ToString(),
                PEC = Guid.NewGuid().ToString(),
                Mobile = Guid.NewGuid().ToString(),
                Phone = Guid.NewGuid().ToString()
            };

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

            var result = await service.UserContactsAsync();

            Assert.NotNull(result);
            Assert.Contains(ui.Email, result.Email);
            Assert.Contains(ui.PEC, result.Pec);
            Assert.Contains(ui.Mobile, result.Mobile);
            Assert.Contains(ui.Phone, result.Phone);
        }

        [Fact]
        public async Task ContactsAsync_ProfileFromCC_Ok()
        {
            //Arrange
            var userInfoRepoMock = new Mock<IInfoRepository<UserInfo>>();
            var sessionInfoRepoMock = new Mock<IInfoRepository<SessionInfo>>();
            var ccRepoMock = new Mock<IInfoRepository<ContactCenterInfo>>();
            var userRepoMock = new Mock<IUserRepository>();

            var si = new SessionInfo
            {
                ProfileTypeId = SpecialProfiles.ContactCenter,
                PECVerificationStatus = PecVerificationStatuses.Validated,
                IsInfoPrivacyAccepted = true,
            };

            var cf = Guid.NewGuid().ToString();
            var ui = new UserInfo
            {
                UserId = cf,
                FiscalCode = cf,
                Email = Guid.NewGuid().ToString(),
                PEC = Guid.NewGuid().ToString(),
                Mobile = Guid.NewGuid().ToString(),
                Phone = Guid.NewGuid().ToString()
            };

            sessionInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(si);
            userInfoRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(ui);
            userRepoMock.Setup(x => x.UserAsync(cf, CommonAuthenticationTypes.Undefined, string.Empty)).ReturnsAsync(ui);
            ccRepoMock.Setup(x => x.RetrieveAsync()).ReturnsAsync(new ContactCenterInfo() { UserId = cf, FiscalCode = cf, Email = ui.Email, PEC = ui.PEC, Mobile = ui.Mobile, Phone = ui.Phone });

            var service = new PassiUserContactsService(
                PassiFixture.AccessorUnderTest(),
                sessionInfoRepoMock.Object,
                userInfoRepoMock.Object,
                ccRepoMock.Object,
                userRepoMock.Object);

            var result = await service.UserContactsAsync();

            Assert.NotNull(result);
            Assert.Contains(ui.Email, result.Email);
            Assert.Contains(ui.PEC, result.Pec);
            Assert.Contains(ui.Mobile, result.Mobile);
            Assert.Contains(ui.Phone, result.Phone);
        }



    }
}

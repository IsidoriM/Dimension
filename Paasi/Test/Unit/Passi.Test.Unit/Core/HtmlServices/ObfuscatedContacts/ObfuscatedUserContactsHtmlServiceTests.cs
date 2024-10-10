using Moq;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Services;
using Passi.Test.Unit.Fixtures;
using Mocks = Passi.Test.Unit.Fixtures.Mocks;

namespace Passi.Test.Unit.Core.HtmlServices.Contacts
{
    public class ObfuscatedUserContactsHtmlServiceTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public ObfuscatedUserContactsHtmlServiceTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData(1)]
        public async Task ObfuscatedContactsAsync_ProfileFromCC_Ok(int serviceId)
        {
            //Arrange
            Mocks mocks = fixture.Mocks(serviceId);

            SessionInfo si = new()
            {
                PECVerificationStatus = PecVerificationStatuses.Validated,
                IsInfoPrivacyAccepted = true
            };

            string cf = Guid.NewGuid().ToString();
            UserInfo ui = new()
            {
                UserId = cf,
                FiscalCode = cf,
                Email = Guid.NewGuid().ToString(),
                PEC = Guid.NewGuid().ToString(),
                Mobile = Guid.NewGuid().ToString(),
                Phone = Guid.NewGuid().ToString()
            };

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

            UserContacts result = await service.UserContactsAsync(cf);

            Assert.NotNull(result);
            Assert.Contains(ui.Email[1..4], result.Email);
            Assert.Contains(ui.PEC, result.Pec);
            Assert.Contains(ui.Mobile[1..4], result.Mobile);
            Assert.Contains(ui.Phone[1..4], result.Phone);
        }
    }
}

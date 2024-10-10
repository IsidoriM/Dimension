using AutoFixture.Xunit2;
using Moq;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Entities;
using Passi.Core.Exceptions;
using Passi.Test.Unit.Fixtures;
using Mocks = Passi.Test.Unit.Fixtures.Mocks;

namespace Passi.Test.Unit.Core
{
    public class PassiServiceTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public PassiServiceTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void UrlOptions_Ok()
        {
            //Arrange
            var mocks = fixture.Mocks(0);

            var service = mocks.PackPassiService();
            Assert.True(service.SwitchProfileUrl() == Fixtures.Mocks.UrlOptions.Value.SwitchProfile);
            Assert.True(service.LogoutUrl() == Fixtures.Mocks.UrlOptions.Value.Logout);
        }

        [Fact]
        public async Task HtmlContactsAsync_Ok()
        {
            //Arrange
            var mocks = fixture.Mocks(0);

            var service = mocks.PackPassiService();
            var result = await service.UserContactsAsync();
            Assert.NotNull(result);
            Assert.NotEmpty(result.Email);
        }

        [Theory]
        [InlineData("AAAAAA")]
        public async Task HtmlContactsAsyncFiscalCode_Ok(string fiscalCode)
        {
            //Arrange
            var mocks = fixture.Mocks(0);
            var service = mocks.PackPassiService();
            var result = await service.UserContactsAsync(fiscalCode);
            Assert.NotNull(result);
            Assert.NotEmpty(result.Email);
        }

        [Theory]
        [InlineData(1)]
        public async Task IsAuthorized_Ok(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);
            var service = mocks.PackPassiService();
            var result = await service.IsAuthorizedAsync(serviceId);
            Assert.True(result);
        }

        [Theory]
        [InlineData(1)]
        public async Task IsAuthorizedEnteId_Ok(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);
            mocks.UserRepo.Setup(s => s.IsGrantedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            var service = mocks.PackPassiService();
            var result = await service.IsAuthorizedAsync(serviceId);
            Assert.True(result);
        }

        [Fact]
        public async Task MeAsync_Ok()
        {
            //Arrange
            var mocks = fixture.Mocks(0);
            var service = mocks.PackPassiService();
            var result = await service.MeAsync();
            Assert.NotNull(result.UserId);
        }

        [Fact]
        public async Task ProfileAsync_Ok()
        {
            //Arrange
            var mocks = fixture.Mocks(0);
            var service = mocks.PackPassiService();
            var result = await service.ProfileAsync();
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(1)]
        public async Task ServicesAsync_Ok(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);
            var service = mocks.PackPassiService();

            var result = await service.AuthorizedServicesAsync();
            Assert.NotNull(result);
            Assert.True(result.Any());
            Assert.Equal(serviceId, result.First());
        }

        [Theory]
        [InlineData("AAAA")]
        public async Task HasPatronageAsync_Ok(string delegatedFiscalCode)
        {
            //Arrange
            var mocks = fixture.Mocks(0);
            var service = mocks.PackPassiService();
            var result = await service.HasPatronageDelegationAsync(delegatedFiscalCode);
            Assert.False(result);
        }

        [Fact]
        public async Task MeAsyncNotFound_Ok()
        {
            // Arrange
            Mocks mocks = fixture.Mocks(0);
            mocks.ContactCenterRepo.Setup(c => c.RetrieveAsync()).ThrowsAsync(new NotFoundException());
            IPassiService service = mocks.PackPassiService();
            User result = await service.MeAsync();
            Assert.NotNull(result.UserId);
        }

        [Theory]
        [InlineAutoData(false, 0, 0, true, "")]
        [InlineAutoData(true, 0, 0, true)]
        [InlineAutoData(false, 1, 0, true, "123")]
        [InlineAutoData(false, 0, 0, false)]
        [InlineAutoData(false, 0, 1, true)]
        public async Task IsAuthorized_OtherResults(bool expectedResult, ushort serviceIdDifference, ushort serviceIdConventionsDifference, bool compareResult, string id, ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;
            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt + serviceIdDifference);
            mocks.LevelsRepo.Setup(l => l.CompareAuthorizationAsync(It.IsAny<char>(), It.IsAny<char>())).ReturnsAsync(compareResult);
            mocks.ConventionInfo.Conventions.First().ServiceId = serviceIdInt + serviceIdConventionsDifference;
            mocks.ProfileInfo.Id = id;
            IPassiService service = mocks.PackPassiService();
            bool result = await service.IsAuthorizedAsync(serviceIdInt);
            Assert.Equal(expectedResult, result);
        }
    }
}

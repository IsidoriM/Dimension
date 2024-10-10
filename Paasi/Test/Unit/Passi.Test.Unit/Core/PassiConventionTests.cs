using AutoFixture.Xunit2;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Entities;
using Passi.Test.Unit.Fixtures;

namespace Passi.Test.Unit.Core
{
    public class PassiConventionTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture passiFixture;

        public PassiConventionTests(PassiFixture passiFixture)
        {
            this.passiFixture = passiFixture;
        }

        [Fact]
        public async Task ConventionRolesAsync_Ok()
        {
            //Arrange
            var mocks = passiFixture.Mocks(1);
            var service = mocks.PackPassiConventionService();
            var result = await service.ConventionRolesAsync();
            Assert.NotEmpty(result);
            Assert.True(!string.IsNullOrWhiteSpace(result.First().Value));
        }

        [Fact]
        public async Task ConventionRoles_NotMatchingServiceId_Ok()
        {
            Mocks mocks = passiFixture.Mocks(2);
            IPassiConventionService service = mocks.PackPassiConventionService();
            ICollection<Role> result = await service.ConventionRolesAsync();
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("AAA")]
        public async Task ConventionHasRolesAsync_Ok(string role)
        {
            //Arrange
            var mocks = passiFixture.Mocks(1);
            var service = mocks.PackPassiConventionService();
            var result = await service.ConventionHasRoleAsync(role);
            Assert.True(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task ConventionHasRole_NotMatchingServiceId_Ok(string role)
        {
            Mocks mocks = passiFixture.Mocks(2);
            IPassiConventionService service = mocks.PackPassiConventionService();
            bool result = await service.ConventionHasRoleAsync(role);
            Assert.False(result);
        }

        [Fact]
        public async Task ConventionFiltersAsync_Ok()
        {
            //Arrange
            var mocks = passiFixture.Mocks(1);
            var service = mocks.PackPassiConventionService();
            var result = await service.ConventionFiltersAsync();
            Assert.NotEmpty(result);
            Assert.True(!string.IsNullOrWhiteSpace(result.First().Value));
            Assert.True(!string.IsNullOrWhiteSpace(result.First().Scope));
            Assert.True(!string.IsNullOrWhiteSpace(result.First().Type));
        }

        [Fact]
        public async Task ConventionFilters_NotMatchingServiceId_Ok()
        {
            Mocks mocks = passiFixture.Mocks(2);
            IPassiConventionService service = mocks.PackPassiConventionService();
            ICollection<Filter> result = await service.ConventionFiltersAsync();
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("R")]
        public async Task ConventionFiltersAsyncGetType_Ok(string type)
        {
            //Arrange
            var mocks = passiFixture.Mocks(1);
            var service = mocks.PackPassiConventionService();
            var result = await service.ConventionFiltersAsync(type);
            Assert.NotEmpty(result);
            Assert.True(!string.IsNullOrWhiteSpace(result.First().Value));
            Assert.True(!string.IsNullOrWhiteSpace(result.First().Scope));
            Assert.True(!string.IsNullOrWhiteSpace(result.First().Type));
        }

        [Theory]
        [InlineAutoData]
        public async Task ConventionFiltersWithType_NotMatchingServiceId_Ok(string type)
        {
            Mocks mocks = passiFixture.Mocks(2);
            IPassiConventionService service = mocks.PackPassiConventionService();
            ICollection<Filter> result = await service.ConventionFiltersAsync(type);
            Assert.Empty(result);
        }
    }
}

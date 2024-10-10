using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Moq;
using Passi.Authentication.Cookie.Const;
using Passi.Authentication.Cookie.Repository;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Test.Unit.Fixtures;
using System.Data;
using System.Security.Cryptography;

namespace Passi.Test.Unit.Authentication.Cookie
{
    public class ConventionInfoRepositoryTests : IClassFixture<PassiFixture>
    {
        public ConventionInfoRepositoryTests(PassiFixture _)
        {
        }

        [Fact]
        public async Task RetrieveAsync_NoCookie_Ok()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var repo = new ConventionInfoRepository(mockHttpContextAccessor.Object, cypher);

            var output = await repo.RetrieveAsync();

            Assert.True(output != null);
            Assert.True(output.UserId == string.Empty);
            Assert.True(!output.Conventions.Any());
        }

        [Fact]
        public async Task RetrieveAsync_Ok()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var input = new ConventionInfo()
            {
                WorkOfficeCode = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid().ToString(),
                UserTypeId = 32,
            };

            var serviceId = RandomNumberGenerator.GetInt32(0, 1000);
            input.Conventions.Add(new Convention()
            {
                IsAvailable = true,
                ServiceId = serviceId,
                Roles = new List<Role>() { new Role() { Value = Guid.NewGuid().ToString() } },
                Filters = new List<Filter>() { new Filter() { Value = Guid.NewGuid().ToString(), Scope = "a", Type = "R" } }
            });

            var toCypher = input.Serialize();

            var cookie = cypher.Crypt(toCypher);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.Convention, cookie).ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var repo = new ConventionInfoRepository(mockHttpContextAccessor.Object, cypher);

            var output = await repo.RetrieveAsync();

            Assert.True(output != null);
            Assert.True(output.UserId == input.UserId);
            Assert.True(output.UserTypeId == input.UserTypeId);
            Assert.True(output.WorkOfficeCode == input.WorkOfficeCode);
            Assert.Contains(output.Conventions.Where(x => x.ServiceId == serviceId), x => x.IsAvailable);
        }

        [Fact]
        public async Task RetrieveAsync_NullContext_Ok()
        {
            // Arrange
            IDataCypherService cypher = PassiFixture.DataCypherUnderTest();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns((HttpContext?)null);
            ConventionInfoRepository repo = new(mockHttpContextAccessor.Object, cypher);

            ConventionInfo output = await repo.RetrieveAsync();

            Assert.NotNull(output);
        }

        [Fact]
        public async Task UpdateAsync_Ok()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var repo = new ConventionInfoRepository(mockHttpContextAccessor.Object, cypher);
            var input = new ConventionInfo()
            {
                UserId = Guid.NewGuid().ToString(),
            };
            input.Conventions.Add(new Convention()
            {
                IsAvailable = true,
            });

            var output = await repo.UpdateAsync(input);
            Assert.True(output != null);
            Assert.Equal(input.UserId, output.UserId);
            Assert.Contains(output.Conventions, x => x.IsAvailable);
        }

        [Theory]
        [InlineAutoData]
        public async Task UpdateAsync_NullContext_Ok(string userId)
        {
            // Arrange
            IDataCypherService cypher = PassiFixture.DataCypherUnderTest();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns((HttpContext?)null);
            ConventionInfoRepository repo = new(mockHttpContextAccessor.Object, cypher);
            ConventionInfo input = new()
            {
                UserId = userId,
            };

            Exception? ex = await Record.ExceptionAsync(() => repo.UpdateAsync(input));

            Assert.Null(ex);
        }

        [Fact]
        public void Compress_ErrorNotImplementedException()
        {
            var repo = new ConventionInfoRepository(new Mock<IHttpContextAccessor>().Object, PassiFixture.DataCypherUnderTest());
            Assert.Throws<NotImplementedException>(() => repo.Compress(new ConventionInfo()));
        }

        [Theory]
        [InlineAutoData]
        public async Task Update_NotImplemented(string userId)
        {
            ConventionInfoRepository repo = new(new Mock<IHttpContextAccessor>().Object, PassiFixture.DataCypherUnderTest());
            await Assert.ThrowsAsync<NotImplementedException>(() => repo.UpdateAsync(userId));
        }
    }
}

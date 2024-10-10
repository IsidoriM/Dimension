using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http;
using Moq;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Test.Unit.Fixtures;
using AutoFixture;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Domain.Entities;

namespace Passi.Test.Unit.Authentication.Cookie
{
    public class ExtensionsTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public ExtensionsTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineAutoData]
        public void RemoveCookie_Ok(string name)
        {
            // Arrange
            DefaultHttpContext context = new();

            Exception exception = Record.Exception(() => context.RemoveCookie(name));

            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData]
        public void AddCypheredCookie_Ok(string name, string value)
        {
            // Arrange
            DefaultHttpContext context = new();
            Mock<IDataCypherService> mockCypher = new(MockBehavior.Strict);
            mockCypher.Setup(c => c.Crypt(It.IsAny<string>(), It.IsAny<Crypto>())).Returns(fixture.Fixture.Create<string>());

            Exception exception = Record.Exception(() => context.AddCypheredCookie(name, value, mockCypher.Object));

            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData]
        public void AddCypheredCookieTypeLoadException_Ok(string name, string value)
        {
            // Arrange
            DefaultHttpContext context = new();
            Mock<IDataCypherService> mockCypher = new(MockBehavior.Strict);
            mockCypher.Setup(c => c.Crypt(It.IsAny<string>(), It.IsAny<Crypto>())).Throws(new TypeLoadException());

            Exception exception = Record.Exception(() => context.AddCypheredCookie(name, value, mockCypher.Object));

            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData(true)]
        [InlineAutoData(false)]
        public void ConventionInfoSerialize_Ok(bool isAvailable)
        {
            ConventionInfo conventionInfo = fixture.Fixture.Create<ConventionInfo>();
            conventionInfo.Conventions.First().IsAvailable = isAvailable;

            string output = conventionInfo.Serialize();

            Assert.NotNull(output);
            Assert.NotEmpty(output);
        }

    }
}

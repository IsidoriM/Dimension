using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using Passi.Authentication.Cookie.Const;
using Passi.Core.Application.Options;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Exceptions;
using Passi.Core.Services;
using Passi.Test.Unit.Fixtures;
using System.Web;
using Mocks = Passi.Test.Unit.Fixtures.Mocks;

namespace Passi.Test.Unit.Core
{
    public class PassiSecureTests : IClassFixture<PassiFixture>
    {
        private readonly IPassiSecureService passiSecure;
        private readonly PassiFixture passiFixture;

        public PassiSecureTests(PassiFixture passiFixture)
        {
            var mocks = passiFixture.Mocks(1);
            passiSecure = new PassiSecureService
                (
                    PassiFixture.DataCypherUnderTest(),
                    mocks.SessionRepo.Object,
                    mocks.ContactCenterRepo.Object,
                    mocks.ConfigurationOptions,
                    PassiFixture.AccessorUnderTest()
                );
            this.passiFixture = passiFixture;
        }

        [Fact]
        public async Task SecureAsyncDic_Ok()
        {
            Dictionary<string, string> data = new() {
                { "a", "1" },
                { "b", "2" }
            };
            var result = await passiSecure.SecureAsync(data);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task SessionToken_InvalidSession(Uri redirectUrl)
        {
            Mocks mocks = passiFixture.Mocks(1);
            mocks.SessionRepo.Setup(a => a.RetrieveAsync()).Throws(new NotFoundException("test"));
            IOptions<ConfigurationOptions> configurationOptions = Options.Create(new ConfigurationOptions()
            {
                SessionManagementFlag = "3",
                ServiceId = 1,
                RedirectUrl = redirectUrl.ToString()
            });
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new(MockBehavior.Strict);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            PassiSecureService myPassiSecure = new(PassiFixture.DataCypherUnderTest(), mocks.SessionRepo.Object, mocks.ContactCenterRepo.Object, configurationOptions, mockHttpContextAccessor.Object);
            var result = await myPassiSecure.SessionTokenAsync();
            Assert.True(string.IsNullOrWhiteSpace(result));
        }

        [Theory]
        [InlineAutoData]
        public async Task SessionToken_InvalidData(Uri redirectUrl)
        {
            Mocks mocks = passiFixture.Mocks(1);
            mocks.SessionRepo.Setup(a => a.RetrieveAsync()).Throws(new InvalidDataException("test"));
            IOptions<ConfigurationOptions> configurationOptions = Options.Create(new ConfigurationOptions()
            {
                SessionManagementFlag = "3",
                ServiceId = 1,
                RedirectUrl = redirectUrl.ToString()
            });
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new(MockBehavior.Strict);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            PassiSecureService myPassiSecure = new(PassiFixture.DataCypherUnderTest(), mocks.SessionRepo.Object, mocks.ContactCenterRepo.Object, configurationOptions, mockHttpContextAccessor.Object);
            var result = await myPassiSecure.SessionTokenAsync();
            Assert.True(string.IsNullOrWhiteSpace(result));
        }

        [Theory]
        [InlineAutoData]
        public async Task SessionToken_Ok(Uri redirectUrl)
        {
            Mocks mocks = passiFixture.Mocks(1);
            IOptions<ConfigurationOptions> configurationOptions = Options.Create(new ConfigurationOptions()
            {
                SessionManagementFlag = "3",
                ServiceId = 1,
                RedirectUrl = redirectUrl.ToString()
            });
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new(MockBehavior.Strict);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            PassiSecureService myPassiSecure = new(PassiFixture.DataCypherUnderTest(), mocks.SessionRepo.Object, mocks.ContactCenterRepo.Object, configurationOptions, mockHttpContextAccessor.Object);
            var result = await myPassiSecure.SessionTokenAsync();
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task SessionToken_IsApi_Ok(Uri redirectUrl)
        {
            Mocks mocks = passiFixture.Mocks(1);
            IOptions<ConfigurationOptions> configurationOptions = Options.Create(new ConfigurationOptions()
            {
                SessionManagementFlag = "3",
                ServiceId = 1,
                RedirectUrl = redirectUrl.ToString()
            });

            var context = new DefaultHttpContext();
            context.Request.Path = "/api/test";
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new(MockBehavior.Strict);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);
            PassiSecureService myPassiSecure = new(PassiFixture.DataCypherUnderTest(), mocks.SessionRepo.Object, mocks.ContactCenterRepo.Object, configurationOptions, mockHttpContextAccessor.Object);
            var result = await myPassiSecure.SessionTokenAsync();
            Assert.True(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public async Task SessionTokenContactCenterException_Ok()
        {
            Mocks mocks = passiFixture.Mocks(1);
            mocks.ContactCenterRepo.Setup(c => c.RetrieveAsync()).ThrowsAsync(new NotFoundException());
            PassiSecureService currentPassiSecure = new(PassiFixture.DataCypherUnderTest(), mocks.SessionRepo.Object, mocks.ContactCenterRepo.Object, mocks.ConfigurationOptions, PassiFixture.AccessorUnderTest());

            string result = await currentPassiSecure.SessionTokenAsync();

            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("AAAA", 0)]
        [InlineData("AAAA", 4)]
        [InlineData("A&AA", 0, new string[] { "&" })]
        public async Task CheckParameter_Ok(string parameter, int maxLength, string[]? escapeStrings = null)
        {
            //Arrange
            var result = await passiSecure.CheckParameterAsync(parameter, maxLength, escapeStrings);
            Assert.True(result);
        }

        [Theory]
        [InlineData("AA", 1)]
        [InlineData("AAA%3B", 0)]
        [InlineData("AAA;", 0)]
        [InlineData("A;;A", 4)]
        [InlineData("A;AA", 0, new string[] { "&" })]
        public async Task CheckParameter_Fail(string parameter, int maxLength, string[]? escapeStrings = null)
        {
            //Arrange
            var result = await passiSecure.CheckParameterAsync(parameter, maxLength, escapeStrings);
            Assert.False(result);
        }


        [Theory]
        [InlineAutoData]
        public async Task SecureUnsecure_Ok(Dictionary<string, string> dictionary)
        {
            string cypheredData = await passiSecure.SecureAsync(dictionary);
            IDictionary<string, string> decypheredData = await passiSecure.UnsecureAsync(cypheredData);

            Assert.NotNull(decypheredData);
            Assert.NotEmpty(decypheredData);
            Assert.Equal(dictionary.Count, decypheredData.Count);
            Assert.Equal(dictionary, decypheredData);
        }
    }
}

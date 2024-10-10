using AutoFixture.Xunit2;
using Bogus;
using Bogus.Extensions.Italy;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Moq;
using Passi.Authentication.Cookie.Const;
using Passi.Authentication.Cookie.Repository;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Test.Unit.Fixtures;
using System.Security.Cryptography;

namespace Passi.Test.Unit.Authentication.Cookie
{
    public class ProfileInfoRepositoryTests : IClassFixture<PassiFixture>
    {
        public ProfileInfoRepositoryTests(PassiFixture _)
        {
        }

        [Fact]
        public async Task RetrieveAsync_NoProfile_Ok()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.Profile, string.Empty).ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var mockUserRepository = new Mock<IUserRepository>();

            var repo = new ProfileInfoRepository(mockHttpContextAccessor.Object,
                cypher,
                mockUserRepository.Object);

            var output = await repo.RetrieveAsync();

            Assert.True(output != null);
            Assert.True(output.Id == string.Empty);
            Assert.True(output.ProfileTypeId == int.MinValue);

        }

        [Fact]
        public async Task RetrieveAsync_Ok()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var person = new Person();

            var now = DateTime.UtcNow;
            now = now.AddMilliseconds(-now.Millisecond);

            var input = new ProfileInfo
            {
                Id = person.CodiceFiscale(),
                ProfileTypeId = RandomNumberGenerator.GetInt32(0, 100),
                FiscalCode = person.CodiceFiscale(),
                InstitutionCode = "009",
                LastUpdate = now.AddDays(-1),
                Opening = new DateTime(now.Year, now.Month, now.Day, now.AddHours(-1).Hour, 00, 00, DateTimeKind.Local),
                Closing = new DateTime(now.Year, now.Month, now.Day, now.AddHours(1).AddMinutes(-1).Hour, now.AddHours(1).AddMinutes(-1).Minute, 59, DateTimeKind.Local),
                OfficeCode = Guid.NewGuid().ToString(),
                Timeout = TimeSpan.FromSeconds(RandomNumberGenerator.GetInt32(0, 1800))
            };

            var iterations = RandomNumberGenerator.GetInt32(5, 20);
            for (int i = 1; i < iterations; i++)
            {
                input.Services.Add(new Service()
                {
                    Id = i,
                    GroupId = 0,
                    HasConvention = i % 2 == 0,
                    RequiredAuthenticationType = StringExtensions.RandomString<CommonAuthenticationTypes>().ShortDescribe()
                });
            }


            var toCypher = input.Serialize();

            var cookie = cypher.Crypt(toCypher);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.Profile, cookie).ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var mockUserRepository = new Mock<IUserRepository>();

            var repo = new ProfileInfoRepository(mockHttpContextAccessor.Object,
                cypher,
                mockUserRepository.Object);

            var output = await repo.RetrieveAsync();

            Assert.True(output != null);
            Assert.True(output.Id == input.Id);
            Assert.True(output.ProfileTypeId == input.ProfileTypeId);
            Assert.True(output.FiscalCode == input.FiscalCode);
            Assert.True(output.InstitutionCode == input.InstitutionCode);
            Assert.True(output.LastUpdate.ToMilliseconds() == input.LastUpdate.ToMilliseconds());
            Assert.True(output.Opening.ToString("HH:mm:ss") == input.Opening.ToUniversalTime().ToString("HH:mm:ss"));
            Assert.True(output.Closing.ToString("HH:mm:ss") == input.Closing.ToUniversalTime().ToString("HH:mm:ss"));
            Assert.True(output.Timeout == input.Timeout);
            Assert.True(output.ProfileTypeId == input.ProfileTypeId);
            Assert.True(output.OfficeCode == input.OfficeCode);
            Assert.True(output.Services.Count == input.Services.Count);
            Assert.True(output.Services.First().RequiredAuthenticationType == input.Services.First().RequiredAuthenticationType);
            Assert.True(output.Services.First().HasConvention == input.Services.First().HasConvention);
            Assert.True(output.Services.First().Id == input.Services.First().Id);
        }

        [Fact]
        public async Task RetrieveAsyncFederated_Ok()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var person = new Person();

            var now = DateTime.UtcNow;
            now = now.AddMilliseconds(-now.Millisecond);

            string cf = person.CodiceFiscale();
            var input = new ProfileInfo
            {
                Id = cf,
                ProfileTypeId = RandomNumberGenerator.GetInt32(0, 1800),
                FiscalCode = $"{cf}:{cf}",
                InstitutionCode = "009",
                LastUpdate = now.AddDays(-1),
                Opening = new DateTime(now.Year, now.Month, now.Day, now.AddHours(-1).Hour, 00, 00, DateTimeKind.Local),
                Closing = new DateTime(now.Year, now.Month, now.Day, now.AddHours(1).AddMinutes(-1).Hour, now.AddHours(1).AddMinutes(-1).Minute, 59, DateTimeKind.Local),
                OfficeCode = Guid.NewGuid().ToString(),
                Timeout = TimeSpan.FromSeconds(RandomNumberGenerator.GetInt32(0, 1800))
            };

            var iterations = RandomNumberGenerator.GetInt32(5, 20);
            for (int i = 1; i < iterations; i++)
            {
                input.Services.Add(new Service()
                {
                    Id = i,
                    GroupId = 0,
                    HasConvention = i % 2 == 0,
                    RequiredAuthenticationType = StringExtensions.RandomString<CommonAuthenticationTypes>().ShortDescribe()
                });
            }


            var toCypher = input.Serialize();

            var cookie = cypher.Crypt(toCypher);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.Profile, cookie).ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var mockUserRepository = new Mock<IUserRepository>();

            var repo = new ProfileInfoRepository(mockHttpContextAccessor.Object,
                cypher,
                mockUserRepository.Object);

            var output = await repo.RetrieveAsync();

            Assert.True(output != null);
            Assert.True(output.Id == input.Id);
            Assert.True(output.ProfileTypeId == input.ProfileTypeId);
            Assert.True(output.FiscalCode == input.FiscalCode);
            Assert.True(output.InstitutionCode == input.InstitutionCode);
            Assert.True(output.LastUpdate.ToMilliseconds() == input.LastUpdate.ToMilliseconds());
            Assert.True(output.Opening.ToString("HH:mm:ss") == input.Opening.ToUniversalTime().ToString("HH:mm:ss"));
            Assert.True(output.Closing.ToString("HH:mm:ss") == input.Closing.ToUniversalTime().ToString("HH:mm:ss"));
            Assert.True(output.Timeout == input.Timeout);
            Assert.True(output.ProfileTypeId == input.ProfileTypeId);
            Assert.True(output.OfficeCode == input.OfficeCode);
            Assert.True(output.Services.Count == input.Services.Count);
            Assert.True(output.Services.First().RequiredAuthenticationType == input.Services.First().RequiredAuthenticationType);
            Assert.True(output.Services.First().HasConvention == input.Services.First().HasConvention);
            Assert.True(output.Services.First().Id == input.Services.First().Id);
        }

        [Fact]
        public async Task RetrieveAsync_NullContext_Ok()
        {
            // Arrange
            IDataCypherService cypher = PassiFixture.DataCypherUnderTest();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns((HttpContext?)null);
            Mock<IUserRepository> mockUserRepository = new();
            ProfileInfoRepository repo = new(mockHttpContextAccessor.Object, cypher, mockUserRepository.Object);

            ProfileInfo output = await repo.RetrieveAsync();

            Assert.NotNull(output);
        }

        [Fact]
        public async Task UpdateAsync_Ok()
        {
            // Arrange
            var cypher = PassiFixture.DataCypherUnderTest();

            var person = new Person();

            var now = DateTime.UtcNow;
            now = now.AddMilliseconds(-now.Millisecond);

            var input = new ProfileInfo
            {
                Id = person.CodiceFiscale(),
                ProfileTypeId = RandomNumberGenerator.GetInt32(0, 100),
                FiscalCode = person.CodiceFiscale(),
                InstitutionCode = "009",
                LastUpdate = now
            };

            var iterations = RandomNumberGenerator.GetInt32(5, 20);
            for (int i = 0; i < iterations; i++)
            {
                input.Services.Add(new Service()
                {
                    Id = i,
                    GroupId = 0,
                    HasConvention = i % 2 == 0,
                    RequiredAuthenticationType = StringExtensions.RandomString<CommonAuthenticationTypes>().ShortDescribe()
                });
            }


            var toCypher = input.Serialize();

            var cookie = cypher.Crypt(toCypher);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(Keys.Cookie, new CookieHeaderValue(Cookies.Profile, cookie).ToString());
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var serviceId = RandomNumberGenerator.GetInt32(0, 1000);
            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(x => x.ServicesAsync(input.Id, input.InstitutionCode))
                .ReturnsAsync(new List<Service>()
                {
                    new Service()
                    {
                        Id = serviceId
                    }
                });

            var profileRepo = new ProfileInfoRepository(mockHttpContextAccessor.Object,
                cypher,
                mockUserRepository.Object);

            var output = await profileRepo.UpdateAsync(input);

            Assert.True(output != null);
            Assert.True(output.LastUpdate > now);
            Assert.True(output.Services.Any());
        }

        [Theory]
        [InlineAutoData]
        public async Task Update_NullContext_Ok(string userId)
        {
            // Arrange
            IDataCypherService cypher = PassiFixture.DataCypherUnderTest();
            ProfileInfo input = new()
            {
                Id = userId,
                Timeout = TimeSpan.FromSeconds(10),
                LastUpdate = DateTime.UtcNow.AddSeconds(-100)
            };
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns((HttpContext?)null);

            Mock<IUserRepository> mockUserRepository = new(MockBehavior.Strict);
            mockUserRepository.Setup(r => r.ServicesAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<Service>());
            ProfileInfoRepository profileRepo = new(mockHttpContextAccessor.Object, cypher, mockUserRepository.Object);

            ProfileInfo output = await profileRepo.UpdateAsync(input);
            Assert.NotNull(output);
        }

        [Theory]
        [InlineAutoData("")]
        [InlineAutoData("notempty", 10000, 1000)]
        public async Task Update_ConditionsNotMet_Ok(string userId, int timeoutMs, int lastUpdateMs)
        {
            // Arrange
            IDataCypherService cypher = PassiFixture.DataCypherUnderTest();
            ProfileInfo input = new()
            {
                Id = userId,
                Timeout = TimeSpan.FromMilliseconds(timeoutMs),
                LastUpdate = DateTime.UtcNow.AddMilliseconds(-lastUpdateMs)
            };
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns((HttpContext?)null);

            Mock<IUserRepository> mockUserRepository = new(MockBehavior.Strict);
            ProfileInfoRepository profileRepo = new(mockHttpContextAccessor.Object, cypher, mockUserRepository.Object);

            ProfileInfo output = await profileRepo.UpdateAsync(input);
            Assert.NotNull(output);
        }
    }
}

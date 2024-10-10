using AutoFixture.Xunit2;
using Moq;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Test.Unit.Fixtures;
using Mocks = Passi.Test.Unit.Fixtures.Mocks;

namespace Passi.Test.Unit.Core.AuthServices.Web
{
    public class ConventionTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public ConventionTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Ho un utente correttamente autenticato con il giusto livello
        /// Ma che non prevede alcuna convenzione
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task NoConventions_Ok(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = 1 }
            };

            var conventionInfo = new ConventionInfo();
            mocks.ConventionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(conventionInfo);
            mocks.ConventionRepo.Setup(x => x.UpdateAsync(It.IsAny<ConventionInfo>())).ReturnsAsync(conventionInfo);

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.UserRepo.Setup(m => m.ConventionsAsync(
                It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(conventionInfo.Conventions);

            mocks.ProfileInfo.Services.First().HasConvention = false;
            mocks.ProfileInfo.Services.Add(new Service() { Id = 100, RequiredAuthenticationType = "1SPI".ShortDescribe(), HasConvention = true });
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(mocks.ProfileInfo);
            mocks.ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(mocks.ProfileInfo);


            var result = await service.IsAuthorizedAsync(serviceId);

            //Assert
            Assert.Equal(result.SessionId, mocks.SessionInfo.SessionId);
        }

        /// <summary>
        /// Ho un utente correttamente autenticato con il giusto livello
        /// Ed ha la corretta convenzione nel cookie
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task ConventionsCookie_Ok(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = 1 }
            };

            var conventionInfo = new ConventionInfo()
            {
                UserTypeId = 1,
                UserId = mocks.SessionInfo.UserId,
                Conventions = new List<Convention>
                {
                    new Convention()
                    {
                        IsAvailable = true,
                        ServiceId = serviceId
                    }
                }
            };
            mocks.ConventionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(conventionInfo);
            mocks.ConventionRepo.Setup(x => x.UpdateAsync(It.IsAny<ConventionInfo>())).ReturnsAsync(conventionInfo);

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.UserRepo.Setup(m => m.ConventionsAsync(
                It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(conventionInfo.Conventions);

            mocks.ProfileInfo.Services.Add(new Service() { Id = 100, RequiredAuthenticationType = "1SPI".ShortDescribe(), HasConvention = true });
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(mocks.ProfileInfo);
            mocks.ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(mocks.ProfileInfo);

            var result = await service.IsAuthorizedAsync(serviceId);

            //Assert
            Assert.Equal(result.SessionId, mocks.SessionInfo.SessionId);
        }

        /// <summary>
        /// Ho un utente correttamente autenticato con il giusto livello
        /// Ed ha la corretta convenzione nel DB
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task ConventionsDB_Ok(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = 1 }
            };

            var conventionInfo = new ConventionInfo()
            {
                UserTypeId = 1,
                UserId = mocks.SessionInfo.UserId,
                Conventions = new List<Convention>
                {
                    new Convention()
                    {
                        IsAvailable = true,
                        ServiceId = serviceId
                    }
                }
            };
            mocks.ConventionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(new ConventionInfo()
            {
                UserTypeId = 1,
                UserId = mocks.SessionInfo.UserId,
                LastUpdate = DateTime.UtcNow.AddMinutes(-10),
                Timeout = TimeSpan.FromMinutes(2),
                Conventions = new List<Convention>
                {
                    new Convention()
                    {
                        IsAvailable = true,
                        ServiceId = serviceId
                    }
                }
            });
            mocks.ConventionRepo.Setup(x => x.UpdateAsync(It.IsAny<ConventionInfo>())).ReturnsAsync(new ConventionInfo());

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.UserRepo.Setup(m => m.ConventionsAsync(
                It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(conventionInfo.Conventions);

            mocks.ProfileInfo.Services.Add(new Service() { Id = 100, RequiredAuthenticationType = "1SPI".ShortDescribe(), HasConvention = true });
            mocks.ProfileInfo.LastUpdate = DateTime.UtcNow.AddMinutes(-10);
            mocks.ProfileInfo.Timeout = TimeSpan.FromMinutes(2);
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(mocks.ProfileInfo);
            mocks.ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(mocks.ProfileInfo);

            var result = await service.IsAuthorizedAsync(serviceId);

            //Assert
            Assert.Equal(result.SessionId, mocks.SessionInfo.SessionId);
        }

        /// <summary>
        /// Ho un utente correttamente autenticato con il giusto livello
        /// Ma non ho una convenzione nel cookie delle convenzioni per quel service id
        /// Errore tipo 84
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task MatchingConvention_ErrorNotFound(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = 1 }
            };

            var conventionInfo = new ConventionInfo()
            {
                UserTypeId = 1,
                UserId = mocks.SessionInfo.UserId,
                Conventions = new List<Convention>()
                {
                    new Convention()
                    {
                        ServiceId = 100
                    }
                }
            };
            mocks.ConventionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(conventionInfo);
            mocks.ConventionRepo.Setup(x => x.UpdateAsync(It.IsAny<ConventionInfo>())).ReturnsAsync(conventionInfo);

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.UserRepo.Setup(m => m.ConventionsAsync(
                It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(conventionInfo.Conventions);

            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(mocks.SessionInfo);
            mocks.SessionRepo.Setup(x => x.UpdateAsync(It.IsAny<SessionInfo>())).ReturnsAsync(mocks.SessionInfo);

            mocks.ProfileInfo.Services.Add(new Service() { Id = 100, RequiredAuthenticationType = "1SPI".ShortDescribe(), HasConvention = true });
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(mocks.ProfileInfo);
            mocks.ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(mocks.ProfileInfo);

            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(async () => await service.IsAuthorizedAsync(serviceId));

            // Controlla che vada in errore
            Assert.Equal(Reason.ConventionNotFound, result.Reason);

            // Controlla che la redirect sia giusta
            Assert.Contains(Fixtures.Mocks.UrlOptions.Value.ErrorPage.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Ho un utente correttamente autenticato con il giusto livello
        /// Ma non ho una convenzione nel cookie delle convenzioni per quel service id
        /// Errore tipo 83
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task MatchingConvention_ErrorExpired(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = 1 }
            };

            var conventionInfo = new ConventionInfo()
            {
                UserTypeId = 1,
                UserId = mocks.SessionInfo.UserId,
                Conventions = new List<Convention>()
                {
                    new Convention()
                    {
                        ServiceId = 1
                    }
                }
            };
            mocks.ConventionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(conventionInfo);
            mocks.ConventionRepo.Setup(x => x.UpdateAsync(It.IsAny<ConventionInfo>())).ReturnsAsync(conventionInfo);

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.UserRepo.Setup(m => m.ConventionsAsync(
                It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(conventionInfo.Conventions);

            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(mocks.SessionInfo);
            mocks.SessionRepo.Setup(x => x.UpdateAsync(It.IsAny<SessionInfo>())).ReturnsAsync(mocks.SessionInfo);

            mocks.ProfileInfo.Services.Add(new Service() { Id = 100, RequiredAuthenticationType = "1SPI".ShortDescribe(), HasConvention = true });
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(mocks.ProfileInfo);
            mocks.ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(mocks.ProfileInfo);

            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(async () => await service.IsAuthorizedAsync(serviceId));

            // Controlla che vada in errore
            Assert.Equal(Reason.ConventionExpired, result.Reason);

            // Controlla che la redirect sia giusta
            Assert.Contains(Fixtures.Mocks.UrlOptions.Value.ErrorPage.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Ho un utente correttamente autenticato con il giusto livello ma che non prevede alcuna convenzione, anche se le info utente implicano che debba averne
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task NoConventionsButHasConvention_Ok(ushort serviceId, string userId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            List<Profile> profiles = new()
            {
                new Profile() { ProfileTypeId = 1 }
            };

            ConventionInfo conventionInfo = new()
            {
                UserId = userId
            };
            mocks.ConventionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(conventionInfo);
            mocks.ConventionRepo.Setup(x => x.UpdateAsync(It.IsAny<ConventionInfo>())).ReturnsAsync(conventionInfo);

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.UserRepo.Setup(m => m.ConventionsAsync(
                It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(conventionInfo.Conventions);

            mocks.ProfileInfo.Services.First().HasConvention = true;
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(mocks.ProfileInfo);
            mocks.ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(mocks.ProfileInfo);


            var result = await service.IsAuthorizedAsync(serviceIdInt);

            //Assert
            Assert.Equal(result.SessionId, mocks.SessionInfo.SessionId);
        }

    }
}

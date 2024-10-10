using Moq;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Test.Unit.Fixtures;
using System.Web;


namespace Passi.Test.Unit.Core.AuthServices.Web
{
    public class SwitchProfileTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public SwitchProfileTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Non ho un profilo, e l'utente non ne ha nessuno
        /// Il servizio non è speciale
        /// devo sloggare l'utente
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task NoProfileWithService_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var profiles = new List<Profile>();
            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.ProfileRepo
                .Setup(x => x.RetrieveAsync())
                .ReturnsAsync(new ProfileInfo());

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore

            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get(Keys.ErrorMessage);

            Assert.Equal(param, ((int)ErrorCodes.Four).ToString());
            Assert.Contains(Fixtures.Mocks.UrlOptions.Value.ErrorPage.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Test Case #2
        /// Non ho un profilo, e l'utente non ne ha nessuno
        /// Il servizio non è speciale
        /// devo sloggare l'utente
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(-1)]
        public async Task NoProfileNeeded_Ok(int serviceId)
        {
            var mocks = fixture.Mocks(serviceId);

            var configOptions = mocks.ConfigurationOptions;
            configOptions.Value.SessionManagementFlag = SessionFlags.Production.ToString();

            var profiles = new List<Profile>();
            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            var service = mocks.PackWebAuthService();

            var result = await service.IsAuthorizedAsync(serviceId);

            Assert.Equal(result.SessionId, mocks.SessionInfo.SessionId);
        }

        /// <summary>
        /// Test Case #3
        /// Non ho un profilo, e l'utente ne ha alcuni
        /// Il servizio richiede solo che ci sia un profilo
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(0, 1)]
        public async Task EveryProfileOK_Ok(int serviceId, int userTypeId)
        {
            var mocks = fixture.Mocks(serviceId);

            var configOptions = mocks.ConfigurationOptions;
            configOptions.Value.SessionManagementFlag = SessionFlags.Production.ToString();

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = userTypeId, InstitutionCode = Fixtures.Mocks.InstitutionCode }
            };

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            var service = mocks.PackWebAuthService();

            var result = await service.IsAuthorizedAsync(serviceId);

            Assert.Equal(result.SessionId, mocks.SessionInfo.SessionId);
        }

        /// <summary>
        /// Test Case #4
        /// Ho un profilo, ma non è quello desiderato
        /// Tra i miei profili ho quello desiderato con il livello richiesto
        /// Devo far cambiare il profilo
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1, 1)]
        public async Task SwitchProfile_ExistsAndAuthorized_Ok(int serviceId, int userTypeId)
        {
            var mocks = fixture.Mocks(serviceId);

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = userTypeId, InstitutionCode = Fixtures.Mocks.InstitutionCode }
            };

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.ProfileInfo.Services.Add(new Service() { Id = serviceId, RequiredAuthenticationType = "3SPI".ShortDescribe() });

            mocks.ConventionInfo.Conventions.Add(new Convention() { ServiceId = serviceId, IsAvailable = true });

            var service = mocks.PackWebAuthService();

            var result = await service.IsAuthorizedAsync(serviceId);

            Assert.Equal(result.SessionId, mocks.SessionInfo.SessionId);
        }

        /// <summary>
        /// Ho un profilo, ed è quello desiderato
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1, 2)]
        public async Task SwitchProfile_WithRequiredProfile_Ok(int serviceId, int requiredUserTypeId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = requiredUserTypeId, InstitutionCode = Fixtures.Mocks.InstitutionCode }
            };

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            var profileInfo = mocks.ProfileInfo;
            profileInfo.ProfileTypeId = requiredUserTypeId;
            profileInfo.InstitutionCode = Fixtures.Mocks.InstitutionCode;
            profileInfo.Services.Add(new Service() { Id = serviceId, RequiredAuthenticationType = "3SPI".ShortDescribe() });
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);
            mocks.ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(profileInfo);

            var service = mocks.PackWebAuthService();

            var result = await service.IsAuthorizedAsync(serviceId);

            Assert.Equal(result.SessionId, mocks.SessionInfo.SessionId);
        }

        /// <summary>
        /// Ho un profilo, ma non è quello desiderato
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1, 2)]
        public async Task SwitchProfile_WithDifferentRequiredProfile_Error(int serviceId, int requiredUserTypeId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId, requiredUserTypeId);

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = requiredUserTypeId },
                new Profile() { ProfileTypeId = requiredUserTypeId + 1 }
            };

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            var profileInfo = mocks.ProfileInfo;
            profileInfo.ProfileTypeId = requiredUserTypeId + 1;
            profileInfo.Services.Add(new Service() { Id = serviceId, RequiredAuthenticationType = "3SPI".ShortDescribe() });
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);
            mocks.ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(profileInfo);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get(Keys.ServiceId);

            Assert.Equal(param, serviceId.ToString().ToString());

            // Verifica del redirect
            Assert.Contains(Fixtures.Mocks.UrlOptions.Value.SwitchProfile.ToString(), result.RedirectUrl.ToString().ToLower());
        }


        /// <summary>
        /// Ho un profilo, ma non è quello desiderato
        /// Tra i servizi disponibili però non ho il servizio desiderato
        /// Deve farmi sloggare
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1, 2)]
        public async Task SwitchProfile_WithDifferentRequiredProfileButNoService_Error(int serviceId, int requiredUserTypeId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId, requiredUserTypeId);

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = requiredUserTypeId },
                new Profile() { ProfileTypeId = requiredUserTypeId+1 }
            };

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            var profileInfo = mocks.ProfileInfo;
            profileInfo.ProfileTypeId = requiredUserTypeId + 1;
            profileInfo.Services.Clear();
            profileInfo.Services.Add(new Service() { Id = serviceId + 1, RequiredAuthenticationType = "3SPI".ShortDescribe() });
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);
            mocks.ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(profileInfo);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get(Keys.ErrorMessage);

            Assert.Equal(param, ((int)ErrorCodes.Four).ToString());

            // Verifica del redirect
            Assert.Contains(Fixtures.Mocks.UrlOptions.Value.ErrorPage.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Non ho ancora un profilo
        /// Il servizio è tra quelli disponibili.
        /// Ho dei profili associati per questo servizio.
        /// Deve fare lo switch
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task SwitchProfile_NoProfileWithDifferentProfile(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = 1, InstitutionCode = "001" },
                new Profile() { ProfileTypeId = 12, InstitutionCode = "012"  },
                new Profile() { ProfileTypeId = 12, InstitutionCode = "013"  }
            };

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.UserRepo.Setup(m => m.AuthorizedServicesAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(new List<Service>()
                {
                    new Service()
                    {
                        GroupId= 1,
                        HasConvention = false,
                        Id = serviceId,
                        RequiredAuthenticationType = "2SPI".ShortDescribe(),
                    }
                });

            mocks.LevelsRepo
                .Setup(x => x.CompareAuthorizationAsync(It.IsAny<char>(), It.IsAny<char>()))
                .ReturnsAsync(false);

            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(new ProfileInfo());

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get("uri");

            Assert.True(!string.IsNullOrWhiteSpace(param));

            // Verifica del redirect
            Assert.Contains(Fixtures.Mocks.UrlOptions.Value.SwitchProfile.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Ho un profilo ma diverso da quello richiesto dal servizio
        /// Il servizio è tra quelli disponibili, però il profilo corrente non è quello richiesto 
        /// ma lo ho tra quelli che ho a disposizione.
        /// Deve fare lo switch
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task SwitchProfile_CurrentWithDifferentProfile(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = 1, InstitutionCode = "001" },
                new Profile() { ProfileTypeId = 12, InstitutionCode = "012"  },
                new Profile() { ProfileTypeId = 12, InstitutionCode = "013"  }
            };

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.UserRepo.Setup(m => m.AuthorizedServicesAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(new List<Service>()
                {
                    new Service()
                    {
                        GroupId= 1,
                        HasConvention = false,
                        Id = serviceId,
                        RequiredAuthenticationType = "2SPI".ShortDescribe(),
                    }
                });

            mocks.LevelsRepo
                .Setup(x => x.CompareAuthorizationAsync(It.IsAny<char>(), It.IsAny<char>()))
                .ReturnsAsync(true);

            var profileInfo = mocks.ProfileInfo;
            profileInfo.ProfileTypeId = 1;
            profileInfo.Services.Clear();
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);
            mocks.ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(profileInfo);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get("uri");

            Assert.True(!string.IsNullOrWhiteSpace(param));

            // Verifica del redirect
            Assert.Contains(Fixtures.Mocks.UrlOptions.Value.SwitchProfile.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Ho un profilo ma diverso da quello richiesto dal servizio
        /// Il servizio è tra quelli disponibili, però il profilo corrente non è quello richiesto 
        /// ma lo ho tra quelli che ho a disposizione.
        /// Deve fare lo switch
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task SwitchProfile_WithDifferentProfile(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = 1, InstitutionCode = "001" },
                new Profile() { ProfileTypeId = 12, InstitutionCode = "012"  },
                new Profile() { ProfileTypeId = 12, InstitutionCode = "013"  }
            };

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.UserRepo.Setup(m => m.AuthorizedServicesAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(new List<Service>()
                {
                    new Service()
                    {
                        GroupId= 1,
                        HasConvention = false,
                        Id = serviceId,
                        RequiredAuthenticationType = "2SPI".ShortDescribe(),
                    }
                });

            mocks.LevelsRepo
                .Setup(x => x.CompareAuthorizationAsync(It.IsAny<char>(), It.IsAny<char>()))
                .ReturnsAsync(false);

            var profileInfo = mocks.ProfileInfo;
            profileInfo.ProfileTypeId = 2;
            profileInfo.Services.Clear();
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);
            mocks.ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(profileInfo);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get("uri");

            Assert.True(!string.IsNullOrWhiteSpace(param));

            // Verifica del redirect
            Assert.Contains(Fixtures.Mocks.UrlOptions.Value.SwitchProfile.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Ho un profilo, ma non è quello desiderato
        /// Il servizio è tra quelli disponibili, però il profilo corrente non è quello richiesto 
        /// ma lo ho tra quelli che ho a disposizione
        /// Deve fare lo switch
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1, 2)]
        public async Task SwitchProfile_WithDifferentRequiredProfileButLevelTooLow_Error(int serviceId, int requiredUserTypeId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId, requiredUserTypeId);

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = requiredUserTypeId },
                new Profile() { ProfileTypeId = requiredUserTypeId+1 }
            };

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.LevelsRepo
                .Setup(x => x.CompareAuthorizationAsync(It.IsAny<char>(), It.IsAny<char>()))
                .ReturnsAsync(false);

            var profileInfo = mocks.ProfileInfo;
            profileInfo.ProfileTypeId = requiredUserTypeId + 1;
            profileInfo.Services.Clear();
            profileInfo.Services.Add(new Service() { Id = serviceId, RequiredAuthenticationType = CommonAuthenticationTypes.CNS.ShortDescribe() });
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);
            mocks.ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(profileInfo);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get("uri");

            Assert.True(!string.IsNullOrWhiteSpace(param));

            // Verifica del redirect
            Assert.Contains(Fixtures.Mocks.UrlOptions.Value.SwitchProfile.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Ho un profilo, ma non è quello desiderato
        /// Il servizio è tra quelli disponibili, però il profilo corrente non è quello richiesto 
        /// e non ce l'ho nemmeno tra quelli disponibili
        /// Deve fare login
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1, 2)]
        public async Task Logout_WithDifferentRequiredProfileButLevelTooLow_Error(int serviceId, int requiredUserTypeId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId, requiredUserTypeId);

            var profiles = new List<Profile>
            {
                new Profile() { ProfileTypeId = requiredUserTypeId + 1 },
                new Profile() { ProfileTypeId = requiredUserTypeId + 2 },
            };

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.LevelsRepo
                .Setup(x => x.CompareAuthorizationAsync(It.IsAny<char>(), It.IsAny<char>()))
                .ReturnsAsync(false);

            var profileInfo = mocks.ProfileInfo;
            profileInfo.ProfileTypeId = requiredUserTypeId + 1;
            profileInfo.Services.Clear();
            profileInfo.Services.Add(new Service() { Id = serviceId, RequiredAuthenticationType = CommonAuthenticationTypes.CNS.ShortDescribe() });
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);
            mocks.ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(profileInfo);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get("errorMsg");

            Assert.True(!string.IsNullOrWhiteSpace(param));

            // Verifica del redirect
            Assert.Contains(Fixtures.Mocks.UrlOptions.Value.ErrorPage.ToString(), result.RedirectUrl.ToString().ToLower());
        }
    }
}

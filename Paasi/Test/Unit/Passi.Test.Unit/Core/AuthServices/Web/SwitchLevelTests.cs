using AutoFixture.Xunit2;
using Moq;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Test.Unit.Fixtures;
using System.Web;
using Mocks = Passi.Test.Unit.Fixtures.Mocks;

namespace Passi.Test.Unit.Core.AuthServices.Web
{
    public class SwitchLevelTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public SwitchLevelTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Se ho un profilo, ma il servizio non è tra quelli a me disponibile
        /// devo sloggare
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task NoServiceAvailable_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var profileInfo = mocks.ProfileInfo;
            profileInfo.Services.Clear();
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            /// Controllo dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get(Keys.ErrorMessage);

            Assert.Equal(param, ((int)ErrorCodes.Four).ToString());
            Assert.Equal(Reason.ServiceUnavailable, result.Reason);

            /// Controllo del redirect
            Assert.Contains(Mocks.UrlOptions.Value.ErrorPage.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Ho un profilo ma diverso da quello richiesto dal servizio (autenticazione CNS)
        /// Il servizio è tra quelli disponibili, però il profilo corrente non è quello richiesto 
        /// ma lo ho tra quelli che ho a disposizione. Il livello di autenticazione richiesta è diverso.
        /// Deve fare il change level e switch
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task ChangeLevel_WithDifferentProfile(int serviceId)
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
                        RequiredAuthenticationType = CommonAuthenticationTypes.CNS.ShortDescribe()
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
            Assert.Contains(Mocks.UrlOptions.Value.PassiWebCns.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Ho un profilo ma diverso da quello richiesto dal servizio (autenticazione OTP)
        /// Il servizio è tra quelli disponibili, però il profilo corrente non è quello richiesto 
        /// ma lo ho tra quelli che ho a disposizione. Il livello di autenticazione richiesta è diverso.
        /// Deve fare il change level e switch
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task ChangeLevelOtp_WithDifferentProfile(int serviceId)
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
                        RequiredAuthenticationType = CommonAuthenticationTypes.OTP.ShortDescribe()
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
            Assert.Contains(Mocks.UrlOptions.Value.PassiWebOtp.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Se ho un profilo, ma il servizio non è tra quelli a me disponibile
        /// devo sloggare
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineAutoData("3SPI")]
        [InlineAutoData(CommonAuthenticationTypes.PIN)]
        public async Task LevelTooLow_Error(string requiredAuthenticationType, ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;

            //Arrange
            var mocks = fixture.Mocks(serviceIdInt);

            var sessionInfo = mocks.SessionInfo;
            sessionInfo.AuthenticationType = CommonAuthenticationTypes.CNS;
            mocks.SessionRepo
                .Setup(x => x.RetrieveAsync())
                .ReturnsAsync(sessionInfo);

            var profileInfo = mocks.ProfileInfo;
            profileInfo.Services.Clear();
            profileInfo.Services.Add(new Service() { Id = serviceIdInt, RequiredAuthenticationType = requiredAuthenticationType.ShortDescribe() });
            mocks.ProfileRepo
                .Setup(x => x.RetrieveAsync())
                .ReturnsAsync(profileInfo);

            mocks.LevelsRepo
                .Setup(x => x.CompareAuthorizationAsync(It.IsAny<char>(), It.IsAny<char>()))
                .ReturnsAsync(false);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceIdInt);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            /// Controllo del redirect
            Assert.Equal(Reason.LevelSwitchNeeded, result.Reason);
            Assert.Contains(Mocks.UrlOptions.Value.PassiWebI.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Se ho un profilo, ma il servizio non è tra quelli a me disponibile (autenticazione CNS)
        /// Devo sloggare
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task LevelTooLowCns_Error(ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            SessionInfo sessionInfo = mocks.SessionInfo;
            sessionInfo.AuthenticationType = CommonAuthenticationTypes.CNS;
            mocks.SessionRepo
                .Setup(x => x.RetrieveAsync())
                .ReturnsAsync(sessionInfo);

            ProfileInfo profileInfo = mocks.ProfileInfo;
            profileInfo.Services.Clear();
            profileInfo.Services.Add(new Service() { Id = serviceIdInt, RequiredAuthenticationType = CommonAuthenticationTypes.CNS.ShortDescribe() });
            mocks.ProfileRepo
                .Setup(x => x.RetrieveAsync())
                .ReturnsAsync(profileInfo);

            mocks.LevelsRepo
                .Setup(x => x.CompareAuthorizationAsync(It.IsAny<char>(), It.IsAny<char>()))
                .ReturnsAsync(false);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            Task<SessionInfo> task = service.IsAuthorizedAsync(serviceIdInt);
            PassiUnauthorizedException result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            /// Controllo del redirect
            Assert.Equal(Reason.LevelSwitchNeeded, result.Reason);
            Assert.Contains(Mocks.UrlOptions.Value.PassiWebCns.ToString().Split('?')[0], result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Se ho un profilo, ma il servizio non è tra quelli a me disponibile (autenticazione OTP)
        /// Devo sloggare
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task LevelTooLowOtp_Error(ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            SessionInfo sessionInfo = mocks.SessionInfo;
            sessionInfo.AuthenticationType = CommonAuthenticationTypes.CNS;
            mocks.SessionRepo
                .Setup(x => x.RetrieveAsync())
                .ReturnsAsync(sessionInfo);

            ProfileInfo profileInfo = mocks.ProfileInfo;
            profileInfo.Services.Clear();
            profileInfo.Services.Add(new Service() { Id = serviceIdInt, RequiredAuthenticationType = CommonAuthenticationTypes.OTP.ShortDescribe() });
            mocks.ProfileRepo
                .Setup(x => x.RetrieveAsync())
                .ReturnsAsync(profileInfo);

            mocks.LevelsRepo
                .Setup(x => x.CompareAuthorizationAsync(It.IsAny<char>(), It.IsAny<char>()))
                .ReturnsAsync(false);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            Task<SessionInfo> task = service.IsAuthorizedAsync(serviceIdInt);
            PassiUnauthorizedException result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            /// Controllo del redirect
            Assert.Equal(Reason.LevelSwitchNeeded, result.Reason);
            Assert.Contains(Mocks.UrlOptions.Value.PassiWebOtp.ToString().Split('?')[0], result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Se ho un profilo, ma il servizio non è tra quelli a me disponibile
        /// devo sloggare
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task LevelHighEnough_Ok(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var sessionInfo = mocks.SessionInfo;
            sessionInfo.AuthenticationType = "3SPI";
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            var profileInfo = mocks.ProfileInfo;
            profileInfo.Services.Clear();
            profileInfo.Services.Add(new Service() { Id = serviceId, RequiredAuthenticationType = "3SPI".ShortDescribe() });
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);

            var service = mocks.PackWebAuthService();

            var result = await service.IsAuthorizedAsync(serviceId);

            Assert.True(result.SessionId == sessionInfo.SessionId);
        }


        /// <summary>
        /// Se il livello è sufficiente, con profilo obbligato ma corrispondente con quello dell'utente, risponde OK
        /// </summary>
        /// <param name="serviceId">ID servizio</param>
        /// <param name="requiredProfileId">ID profilo richiesto</param>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task LevelHighEnoughWithRequiredProfile_Ok(ushort serviceId, ushort requiredProfileId)
        {
            int serviceIdInt = serviceId + 1;
            int requiredProfileIdInt = requiredProfileId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceId, requiredProfileIdInt);

            SessionInfo sessionInfo = mocks.SessionInfo;
            sessionInfo.AuthenticationType = "3SPI";
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            ProfileInfo profileInfo = mocks.ProfileInfo;
            profileInfo.Services.Clear();
            profileInfo.Services.Add(new Service() { Id = serviceId, RequiredAuthenticationType = "3SPI".ShortDescribe() });
            profileInfo.ProfileTypeId = requiredProfileIdInt;
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            SessionInfo result = await service.IsAuthorizedAsync(serviceId);

            Assert.True(result.SessionId == sessionInfo.SessionId);
        }


    }
}

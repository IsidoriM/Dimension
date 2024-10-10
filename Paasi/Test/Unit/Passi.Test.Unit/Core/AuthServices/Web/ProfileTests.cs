using AutoFixture;
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
    public class ProfileTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public ProfileTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Se il cookie di sessione riporta un utente, mentre quello di profilo ne riporta un altro
        /// mi aspetto un uunauthorized
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task UserDifferentFromProfile_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var profileInfo = mocks.ProfileInfo;
            profileInfo.FiscalCode = Guid.NewGuid().ToString();
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(1);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            /// Controllo dell'errore
            Assert.Equal(Reason.SessionUserIdDifferentProfileUserId, result.Reason);

            /// Controllo del redirect
            Assert.Contains(Fixtures.Mocks.UrlOptions.Value.Logout.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        ///  
        /// 
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task NoProfileWithService_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var profileInfo = new ProfileInfo();
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);

            mocks.UserRepo.Setup(x => x.ProfilesAsync(It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(new List<Profile>());

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get(Keys.ErrorMessage);

            Assert.Equal(param, ((int)ErrorCodes.Four).ToString());

            // Verifica del redirect
            Assert.Contains(Mocks.UrlOptions.Value.ErrorPage.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Se nella sessione non risulta un profilo ma l'utente ne ha altri, chiedo lo switch anche se sarebbe stato accettato qualsiasi profilo
        /// </summary>
        /// <param name="serviceId">Service ID del profilo utente</param>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task NoProfile_Switch(ushort serviceId)
        {
            // Arrange
            Mocks mocks = fixture.Mocks(serviceId + 1);

            ProfileInfo profileInfo = mocks.ProfileInfo;
            profileInfo.Id = string.Empty;  // faccio in modo che l'utente attuale non abbia un profilo in questo momento
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            Task<SessionInfo> task = service.IsAuthorizedAsync(SpecialServices.EveryProfileIsOK);
            PassiUnauthorizedException result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // controllo dell'errore
            Assert.Equal(Reason.ProfileNotFound, result.Reason);

            // controllo del redirect
            Assert.Contains(Mocks.UrlOptions.Value.SwitchProfile.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Profilo non autorizzato per il service ID anche se il service ID è tra quelli autorizzati, faccio logout
        /// </summary>
        /// <param name="serviceId">Service ID del profilo utente (l'autorizzato sarà diverso)</param>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task ProfileNotFound_Unauthorized(ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            List<Profile> profiles = fixture.Fixture.Create<List<Profile>>();
            profiles.First().InstitutionCode = mocks.ProfileInfo.InstitutionCode;

            mocks.UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(profiles);

            mocks.UserRepo.Setup(u => u.AuthorizedServicesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<Service> { new Service { Id = serviceIdInt + 1 } });

            ProfileInfo profileInfo = mocks.ProfileInfo;
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            Task<SessionInfo> task = service.IsAuthorizedAsync(serviceIdInt + 1);
            PassiUnauthorizedException result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // cerifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get("errorMsg");

            Assert.True(!string.IsNullOrWhiteSpace(param));

            // cerifica del redirect
            Assert.Contains(Mocks.UrlOptions.Value.ErrorPage.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Utente con delegazione non valida, non autorizzato
        /// </summary>
        /// <param name="serviceId">Service ID dell'utente</param>
        /// <param name="delegatedUserId">ID utente delegato</param>
        /// <param name="userId">ID utente</param>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task InvalidDelegation_Unauthorized(ushort serviceId, string userId, string delegatedUserId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            SessionInfo sessionInfo = mocks.SessionInfo;
            sessionInfo.DelegateUserId = delegatedUserId;
            sessionInfo.UserId = userId;
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            ProfileInfo profileInfo = mocks.ProfileInfo;
            profileInfo.Id = userId;
            profileInfo.FiscalCode = userId;
            profileInfo.LastUpdate = DateTime.UtcNow.AddDays(-1);
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            Task<SessionInfo> task = service.IsAuthorizedAsync(serviceIdInt);
            PassiUnauthorizedException result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // cerifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get("errorMsg");

            // cerifica del redirect
            Assert.Contains(Mocks.UrlOptions.Value.Logout.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Utente con delegazione valida, autorizzato
        /// </summary>
        /// <param name="serviceId">Service ID dell'utente</param>
        /// <param name="delegatedUserId">ID utente delegato</param>
        /// <param name="userId">ID utente</param>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task CorrectDelegation_Ok(ushort serviceId, string userId, string delegatedUserId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            SessionInfo sessionInfo = mocks.SessionInfo;
            sessionInfo.DelegateUserId = delegatedUserId;
            sessionInfo.UserId = userId;
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            ProfileInfo profileInfo = mocks.ProfileInfo;
            profileInfo.Id = userId;
            profileInfo.FiscalCode = userId;
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(profileInfo);

            mocks.UserRepo.Setup(u => u.HasDelegationAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            SessionInfo session = await service.IsAuthorizedAsync(serviceIdInt);

            Assert.NotNull(session);
            Assert.NotNull(session.SessionId);
        }

        /// <summary>
        /// Nessun accessor trovato per l'ID tipo utente richiesto, autorizzato
        /// </summary>
        /// <param name="serviceId">Service ID dell'utente</param>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task NoAccessor_Ok(ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            IPassiAuthenticationService service = mocks.PackWebAuthService(true);

            await Assert.ThrowsAsync<NotFoundException>(async () => await service.IsAuthorizedAsync(serviceIdInt));
        }

        /// <summary>
        /// Tipo utente richiesto non intero
        /// </summary>
        /// <param name="serviceId">Service ID dell'utente</param>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task RequiredUserTypeNoInt_Ok(ushort serviceId, string requiredUserType)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            IPassiAuthenticationService service = mocks.PackWebAuthService(false, requiredUserType);

            Task<SessionInfo> task = service.IsAuthorizedAsync(serviceIdInt);
            PassiUnauthorizedException result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // cerifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get("uri");

            // cerifica del redirect
            Assert.Contains(Mocks.UrlOptions.Value.Logout.ToString(), result.RedirectUrl.ToString().ToLower());
        }
    }
}

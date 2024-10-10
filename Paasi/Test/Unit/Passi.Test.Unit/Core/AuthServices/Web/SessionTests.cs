using AutoFixture.Xunit2;
using Moq;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Test.Unit.Fixtures;
using System.Net;
using System.Web;
using Mocks = Passi.Test.Unit.Fixtures.Mocks;

namespace Passi.Test.Unit.Core.AuthServices.Web
{
    public class SessionTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public SessionTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Se il cookie di sessione splittato per pipe
        /// e lo userId nel cookie di sessione è vuoto deve dare errore
        /// Dal servizio di sessione viene sparato un NotFoundException
        /// </summary>
        [Theory]
        [InlineData(1)]
        public async Task UserIdNotFound_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var sessionInfo = mocks.SessionInfo;
            sessionInfo.UserId = string.Empty;
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ThrowsAsync(new NotFoundException());

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Controlla che vada in errore
            Assert.Equal(Reason.InvalidSessionCookie, result.Reason);

            // Controlla che la redirect sia giusta
            Assert.Contains(Mocks.UrlOptions.Value.PassiWeb.ToString().Split('?')[0], result.RedirectUrl.ToString().ToLower());


        }

        /// <summary>
        /// Se il cookie di sessione splittato per pipe ha una lunghezza inferiore a 38
        /// Dal servizio di sessione viene sparato un InvalidDataException
        /// </summary>
        [Theory]
        [InlineData(1)]
        public async Task InvalidData_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var sessionInfo = mocks.SessionInfo;
            sessionInfo.UserId = string.Empty;
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ThrowsAsync(new InvalidDataException());

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Controlla che vada in errore
            Assert.Equal(Reason.InvalidSessionCookie, result.Reason);

            // Controlla che la redirect sia giusta
            Assert.Contains(Mocks.UrlOptions.Value.PassiWeb.ToString().Split('?')[0], result.RedirectUrl.ToString().ToLower());


        }

        /// <summary>
        /// Verifica che la sessione non abbia superato la durata massima di attesa
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task SessionExpired_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var sessionInfo = mocks.SessionInfo;
            sessionInfo.LastUpdated = DateTime.UtcNow.AddSeconds(-100);
            sessionInfo.SessionTimeout = TimeSpan.FromSeconds(10);
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // verifica dell'errore
            Assert.Equal(Reason.SessionExpiredForTimeout, result.Reason);

            // Verifica del redirect
            //Assert.Contains("linkpassiweb", result.RedirectUrl.ToLower());
        }

        /// <summary>
        /// Verifica che la data di autenticazione non abbia superato il timeout massimo consentito
        /// per la sessione
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task SessionMaximumTime_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var sessionInfo = mocks.SessionInfo;
            sessionInfo.SessionMaximumTime = TimeSpan.FromSeconds(10);
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore
            Assert.Equal(Reason.SessionExpiredForMaximumTime, result.Reason);

            // Verifica del redirect
            //Assert.Contains("linkpassiweb", result.RedirectUrl.ToLower());
        }

        /// <summary>
        /// Verifica che la data di autenticazione non abbia superato il timeout massimo consentito
        /// generale
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task SessionExpiredForMaximumTime_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            mocks.UserRepo.Setup(m => m.HasSessionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                SessionFlags.Production)).ReturnsAsync(false);

            var sessionInfo = mocks.SessionInfo;
            sessionInfo.LastSessionUpdate = DateTime.UtcNow.AddSeconds(-100);
            sessionInfo.SessionMaximumCachingTime = TimeSpan.FromSeconds(10);
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore
            Assert.Equal(Reason.SessionExpiredForMaximumTime, result.Reason);

            // Verifica del redirect
            //Assert.Contains("linkpassiweb", result.RedirectUrl.ToLower());
        }

        /// <summary>
        /// Verifica che in produzione l'utente loggato, 
        /// nel momento in cui ha una sessione scaduta e che non è più valida
        /// venga buttato fuori
        /// generale
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task UserHasInvalidSessionInProductionEnvironment_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            mocks.UserRepo.Setup(m => m.HasSessionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                SessionFlags.Production)).ReturnsAsync(false);

            var sessionInfo = mocks.SessionInfo;
            sessionInfo.SessionMaximumCachingTime = TimeSpan.FromSeconds(1);
            sessionInfo.LastSessionUpdate = DateTime.UtcNow.AddMinutes(-10);
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get(Keys.ErrorMessage);

            Assert.Equal(param, ((int)ErrorCodes.Seven).ToString());

            // Verifica del redirect
            Assert.Contains(Mocks.UrlOptions.Value.Logout.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Verifica che in produzione l'utente loggato, 
        /// nel momento in cui ha una sessione scaduta e che non è più valida
        /// venga buttato fuori
        /// generale
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task IsAuthorizedAsync_UserHasInvalidSessionInProductionEnvironment_Ok(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            var sessionInfo = mocks.SessionInfo;
            sessionInfo.SessionMaximumCachingTime = TimeSpan.FromSeconds(1);
            sessionInfo.LastSessionUpdate = DateTime.UtcNow.AddMinutes(-10);
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            var service = mocks.PackWebAuthService();

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get(Keys.ErrorMessage);

            Assert.Equal(param, ((int)ErrorCodes.Seven).ToString());

            // Verifica del redirect
            Assert.Contains(Mocks.UrlOptions.Value.Logout.ToString(), result.RedirectUrl.ToString().ToLower());
        }

        /// <summary>
        /// Se il flag di gestione sessione non è settato, l'autenticazione funziona correttamente
        /// </summary>
        /// <param name="serviceId">Service ID dell'utente</param>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task NoSessionManagement_Ok(ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);
            mocks.ConfigurationOptions.Value.SessionManagementFlag = "0";

            SessionInfo sessionInfo = mocks.SessionInfo;
            sessionInfo.LastUpdated = DateTime.UtcNow.AddSeconds(-5);
            sessionInfo.SessionTimeout = TimeSpan.FromSeconds(10);
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            SessionInfo session = await service.IsAuthorizedAsync(serviceIdInt);

            Assert.NotNull(session);
            Assert.NotNull(session.SessionId);
        }

        /// <summary>
        /// La sessione è scaduta ma è comunque valida
        /// </summary>
        /// <param name="serviceId">Service ID dell'utente</param>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task UserHasValidSession(ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;
            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            mocks.UserRepo.Setup(m => m.HasSessionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                SessionFlags.Production)).ReturnsAsync(true);

            SessionInfo sessionInfo = mocks.SessionInfo;
            sessionInfo.SessionMaximumCachingTime = TimeSpan.FromSeconds(1);
            sessionInfo.LastSessionUpdate = DateTime.UtcNow.AddMinutes(-10);
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            SessionInfo session = await service.IsAuthorizedAsync(serviceIdInt);

            Assert.NotNull(session);
            Assert.NotNull(session.SessionId);
        }

        /// <summary>
        /// L'utente ha una sessione non valida ma non in produzione
        /// </summary>
        /// <param name="serviceId">Service ID dell'utente</param>
        /// <returns></returns>
        [Theory]
        [InlineAutoData(1)]
        [InlineAutoData(2)]
        public async Task UserHasInvalidSessionNotInProduction_Ok(int sessionManagementFlag, ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;
            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);
            mocks.ConfigurationOptions.Value.SessionManagementFlag = sessionManagementFlag.ToString();

            mocks.UserRepo.Setup(m => m.HasSessionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                SessionFlags.Production)).ReturnsAsync(true);

            SessionInfo sessionInfo = mocks.SessionInfo;
            sessionInfo.SessionMaximumCachingTime = TimeSpan.FromSeconds(1);
            sessionInfo.LastSessionUpdate = DateTime.UtcNow.AddMinutes(-10);
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            SessionInfo session = await service.IsAuthorizedAsync(serviceIdInt);

            Assert.NotNull(session);
            Assert.NotNull(session.SessionId);
        }

        /// <summary>
        /// Se non vengono trovate info relative al contact center, procedi comunque
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task ContactCenterInfoNotFound_Ok(ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            SessionInfo sessionInfo = mocks.SessionInfo;
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            mocks.ContactCenterRepo.Setup(c => c.RetrieveAsync()).ThrowsAsync(new NotFoundException());

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            SessionInfo session = await service.IsAuthorizedAsync(serviceIdInt);

            Assert.NotNull(session);
            Assert.NotNull(session.SessionId);
        }



        /// <summary>
        /// Eccezione di parametro errato in connessione a SQL
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineAutoData]
        public async Task ParameterException_Unauthorized(ushort serviceId)
        {
            int serviceIdInt = serviceId + 1;

            // Arrange
            Mocks mocks = fixture.Mocks(serviceIdInt);

            SessionInfo sessionInfo = mocks.SessionInfo;
            sessionInfo.SessionMaximumCachingTime = TimeSpan.FromSeconds(1);
            sessionInfo.LastSessionUpdate = DateTime.UtcNow.AddMinutes(-10);
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(sessionInfo);

            mocks.UserRepo.Setup(u => u.HasSessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ThrowsAsync(new ParameterException("missing parameter"));

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            Task<SessionInfo> task = service.IsAuthorizedAsync(serviceIdInt);
            PassiUnauthorizedException result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // verifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get(Keys.ErrorMessage);

            Assert.Null(param);

            // Verifica del redirect
            Assert.Contains(Mocks.UrlOptions.Value.PassiWeb.ToString().Split('?')[0], result.RedirectUrl.ToString().ToLower());
        }
    }
}

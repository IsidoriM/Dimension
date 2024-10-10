using Moq;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Exceptions;
using Passi.Test.Unit.Fixtures;
using System.Web;


namespace Passi.Test.Unit.Core.AuthServices.Web
{
    public class TimeSlotsTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public TimeSlotsTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Se ho un profilo, e il servizio non è tra quelli speciali
        /// verifico se il mio profilo è in orario di attività
        /// altrimenti, mando su una pagina di errore (non sloggo)
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task NotInWorkTime_Error(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            mocks.SessionInfo.AuthenticationType = "3SPI";
            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(mocks.SessionInfo);

            mocks.ProfileInfo.Opening = DateTime.UtcNow.AddMinutes(10);
            mocks.ProfileInfo.Closing = DateTime.UtcNow.AddMinutes(20);
            mocks.ProfileInfo.Services.Clear();
            mocks.ProfileInfo.Services.Add(new Service() { Id = serviceId, RequiredAuthenticationType = "3SPI".ShortDescribe() });
            mocks.ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(mocks.ProfileInfo);

            var task = service.IsAuthorizedAsync(serviceId);
            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(() => task);

            // Verifica dell'errore
            Uri myUri = result.RedirectUrl;
            string? param = HttpUtility.ParseQueryString(myUri.Query).Get(Keys.ErrorMessage);
            string? secure = HttpUtility.ParseQueryString(myUri.Query).Get(Keys.Secure);

            Assert.Equal(param, ((int)ErrorCodes.EightyOne).ToString());
            Assert.True(!string.IsNullOrWhiteSpace(secure));

            // Verifica del redirect
            Assert.Contains(Fixtures.Mocks.UrlOptions.Value.ErrorPage.ToString(), result.RedirectUrl.ToString().ToLower());
        }


    }
}

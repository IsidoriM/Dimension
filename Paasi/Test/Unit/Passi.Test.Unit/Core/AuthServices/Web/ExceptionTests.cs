using Moq;
using Passi.Core.Application.Services;
using Passi.Core.Exceptions;
using Passi.Test.Unit.Fixtures;

namespace Passi.Test.Unit.Core.AuthServices.Web
{
    public class ExceptionTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public ExceptionTests(PassiFixture fixture)
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
        public async Task Exception_Generic(int serviceId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);

            IPassiAuthenticationService service = mocks.PackWebAuthService();

            mocks.SessionRepo.Setup(x => x.RetrieveAsync()).ThrowsAsync(new ApplicationException());

            var result = await Assert.ThrowsAsync<PassiUnauthorizedException>(async () => await service.IsAuthorizedAsync(serviceId));

            // Controlla che vada in errore
            Assert.Equal(Reason.Unknown, result.Reason);
        }
    }
}

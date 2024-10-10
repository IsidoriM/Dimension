using Passi.Core.Domain.Const;
using Passi.Test.Unit.Fixtures;

namespace Passi.Test.Unit.Core.AuthServices.Web
{
    public class ContactCenterTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;


        public ContactCenterTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Ho un utente di tipo ContactCenter (ProfileId = 30), devo verificare che al termine dell'autenticazione
        /// l'utente in SessionInfo sia quello preso dai cookie SCC e VSU e non l'Operatore loggato.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1, SpecialProfiles.ContactCenter)]
        public async Task ContactCenter_Ok(int serviceId, int userTypeId)
        {
            //Arrange
            var mocks = fixture.Mocks(serviceId);
            mocks.SessionInfo.ProfileTypeId = userTypeId;
            mocks.ProfileInfo.ProfileTypeId = userTypeId;
            string prevName = mocks.SessionInfo.Name;
            string prevSurname = mocks.SessionInfo.Surname;
            string prevOfficeCode = mocks.SessionInfo.OfficeCode;

            var authService = mocks.PackWebAuthService();
            var sessionInfo = await authService.IsAuthorizedAsync(serviceId);

            Assert.NotNull(sessionInfo);
            Assert.NotEqual(prevName, sessionInfo.Name);
            Assert.NotEqual(prevSurname, sessionInfo.Surname);
            Assert.NotEqual(prevOfficeCode, sessionInfo.OfficeCode);
        }
    }
}

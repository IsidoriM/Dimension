using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using System.Security.Claims;

namespace Passi.Core.Extensions
{
    static class SessionInfoExtensions
    {
        /// <summary>
        /// Se la sessione appartiene a quella di un Operatore (idProfilo = 30), i dati anagrafici dovranno essere presi
        /// dai cookie SCC e VSU e sostituiti a quelli di sessione. <br/>
        /// Nella fattispecie i dati anagrafici presenti in <see cref="SessionInfo"/> saranno sostituiti con quelli presenti in <see cref="ContactCenterInfo"/>
        /// </summary>
        /// <param name="contactCenterInfo">Contiene informazioni dell'utente per il quale l'operatore del ContactCenter sta operando</param>
        /// <returns></returns>
        public static void CheckContactCenterInfo(this SessionInfo sessionInfo, ContactCenterInfo contactCenterInfo)
        {
            if (sessionInfo.ProfileTypeId == SpecialProfiles.ContactCenter)
            {
                sessionInfo.UserId = contactCenterInfo.UserId;
                sessionInfo.FiscalCode = contactCenterInfo.FiscalCode;
                sessionInfo.BirthDate = contactCenterInfo.BirthDate;
                sessionInfo.BirthPlaceCode = contactCenterInfo.BirthPlaceCode;
                sessionInfo.BirthProvince = contactCenterInfo.BirthProvince;
                sessionInfo.Gender = contactCenterInfo.Gender;
                sessionInfo.Name = contactCenterInfo.Name;
                sessionInfo.Surname = contactCenterInfo.Surname;
                sessionInfo.OfficeCode = contactCenterInfo.OfficeCode;
            }
        }

        /// <summary>
        /// Crea il <see cref="ClaimsPrincipal"/> con i dati del <see cref="SessionInfo"/> riferiti alla sessione dell'utenza loggata
        /// </summary>
        /// <param name="schemeName">Name of the authentication scheme used</param>
        /// <returns>L'oggetto <see cref="ClaimsPrincipal"/></returns>
        public static ClaimsPrincipal CreatePrincipal(this SessionInfo sessionInfo, string schemeName)
        {
            ClaimsIdentity identity = new(new List<Claim>()
            {
                new(ClaimTypes.NameIdentifier, sessionInfo.UserId),
                //new(nameof(SessionInfo.UserId), sessionInfo.UserId),
                new(nameof(SessionInfo.AuthenticationType), sessionInfo.AuthenticationType),
                new(nameof(SessionInfo.InstitutionCode), sessionInfo.InstitutionCode),
                //new(nameof(SessionInfo.OfficeCode), sessionInfo.OfficeCode),
                //new(nameof(SessionInfo.MultipleProfile), sessionInfo.MultipleProfile.ToString()),
                new(nameof(SessionInfo.ProfileTypeId), sessionInfo.ProfileTypeId.ToString()),
            },
            schemeName, ClaimTypes.NameIdentifier, ClaimTypes.Role);

            /// Autorizzazione per ID servizio
            /// Aggiungere come ruoli gli id servizi che si trovano nel cookie di profilo
            //identity.AddClaim(new Claim(ClaimTypes.Role, sessionInfo.ProfileTypeId.ToString()));

            return new(identity);
        }
    }
}

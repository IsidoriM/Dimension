using Microsoft.AspNetCore.Http;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Extensions;

namespace Passi.Core.Services
{
    internal class PassiUserContactsService : IPassiUserContactsService
    {
        private readonly IHttpContextAccessor accessor;
        private readonly IInfoRepository<SessionInfo> sessionInfoRepository;
        private readonly IInfoRepository<UserInfo> userInfoRepository;
        private readonly IInfoRepository<ContactCenterInfo> contactCenterRepository;
        private readonly IUserRepository userRepository;

        public PassiUserContactsService(
            IHttpContextAccessor accessor,
            IInfoRepository<SessionInfo> sessionInfoRepository,
            IInfoRepository<UserInfo> userInfoRepository,
            IInfoRepository<ContactCenterInfo> contactCenterRepository,
            IUserRepository userRepository)
        {
            this.accessor = accessor;
            this.sessionInfoRepository = sessionInfoRepository;
            this.userInfoRepository = userInfoRepository;
            this.contactCenterRepository = contactCenterRepository;
            this.userRepository = userRepository;
        }

        public Task<UserContacts> UserContactsAsync()
        {
            return UserContactsAsync(string.Empty);
        }

        public async Task<UserContacts> UserContactsAsync(string fiscalCode)
        {
            try
            {
                Validate();
                return await GetAsync(fiscalCode);
            }
            catch (ContactsException)
            {
                throw;
            }
            catch
            {
                throw new ContactsException(Outcomes.AUC005,
                    fiscalCode,
                    "Si è verificato un errore: non è stato possibile recuperare le informazioni dell'utente (AUC005).");
            }
        }

        #region Private methods

        private void Validate()
        {
            if (accessor.HttpContext?.Request == null)
            {
                throw new ContactsException(Outcomes.AUC006,
                    "Si è verificato un errore: disconnettersi ed effettuare nuovamente l'accesso (AUC006).");
            }
        }

        private async Task<SessionInfo> SessionInfoAsync()
        {
            try
            {
                return await sessionInfoRepository.RetrieveAsync();
            }
            catch (NotFoundException)
            {
                throw new ContactsException(Outcomes.AUC001, "Utente non autenticato (AUC001).");
            }
            catch
            {
                throw new ContactsException(Outcomes.AUC002, "Si è verificato un errore: disconnettersi ed effettuare nuovamente l'accesso (AUC002).");
            }
        }

        private async Task<UserInfo> LoggedUserInfoAsync()
        {
            try
            {
                return await userInfoRepository.RetrieveAsync();
            }
            catch (NotFoundException)
            {
                throw new ContactsException(Outcomes.AUC001, "Utente non autenticato (AUC001).");
            }
            catch
            {
                throw new ContactsException(Outcomes.AUC002, "Si è verificato un errore: disconnettersi ed effettuare nuovamente l'accesso (AUC002).");
            }
        }

        private async Task<UserInfo> UserInfoFromCCAsync()
        {
            // Operatore CC
            // reperire i contatti dell'utente dal cookie SCC impostato dal Contact Center
            try
            {
                return await contactCenterRepository.RetrieveAsync();
            }
            catch (NotFoundException)
            {
                throw new ContactsException(Outcomes.AUC004, "Si è verificato un errore: disconnettersi ed effettuare nuovamente l'accesso (AUC004).");
            }
            catch
            {
                throw new ContactsException(Outcomes.AUC003, "Si è verificato un errore: disconnettersi ed effettuare nuovamente l'accesso (AUC003).");
            }
        }

        private async Task<UserInfo> UserInfoAsync(string targetFiscalCode)
        {
            try
            {
                return await userRepository.UserAsync(targetFiscalCode, CommonAuthenticationTypes.Undefined, string.Empty);
            }
            catch (NotFoundException)
            {
                throw new ContactsException(Outcomes.Two, targetFiscalCode, "L'utente non ha contatti personali registrati.");
            }
            catch
            {
                throw new ContactsException(Outcomes.AUC005, targetFiscalCode, "Si è verificato un errore: non è stato possibile recuperare le informazioni dell'utente (AUC005).");
            }
        }

        private async Task<UserContacts> GetAsync(string targetFiscalCode = "")
        {
            // Recupera i dati di sessione e i dati dell'utente dai Cookie
            SessionInfo sessionInfo = await SessionInfoAsync();
            UserInfo userInfo = await LoggedUserInfoAsync();

            targetFiscalCode = targetFiscalCode.ToUpper();
            bool isObfuscated = !string.IsNullOrEmpty(targetFiscalCode);

            if (string.IsNullOrWhiteSpace(targetFiscalCode))
                targetFiscalCode = userInfo.UserId;

            // In questo caso è l'utenza loggata a richiedere i propri dati
            #region No Fiscal Code
            if (!isObfuscated)
            {
                // Se richiamato con profilo 30 (ContactCenter) deve prendere i dati dal coockie SCC/VSU
                // Se invece richiamato dal profilo 3 (Citizen) deve prendere i dati dal Cookie
                // Significa che un utente CC può richiedere i dati non offuscati di un utente loggato 
                if (sessionInfo.ProfileTypeId == SpecialProfiles.ContactCenter)
                {
                    userInfo = await UserInfoFromCCAsync();
                }
            }
            #endregion

            // In questo caso, sono utenti differenti da quello loggato a richiedere i dati
            // Anzichè dal cookie utente, che non ha, richiede i dati direttamente da sql
            #region Fiscal Code
            else
            {
                if (sessionInfo.ProfileTypeId == SpecialProfiles.Citizen
                    || sessionInfo.ProfileTypeId == SpecialProfiles.ContactCenter
                    || sessionInfo.ProfileTypeId == SpecialProfiles.Thousand)
                {
                    throw new ContactsException(Outcomes.AUC007,
                        targetFiscalCode,
                        "Si è verificato un errore: operazione non consentita per il profilo utente (AUC007).");
                }

                userInfo = await UserInfoAsync(targetFiscalCode);
            }
            #endregion

            return new UserContacts().With(userInfo, sessionInfo, isObfuscated);
        }

        #endregion
    }
}

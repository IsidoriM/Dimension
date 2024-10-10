using Microsoft.Extensions.Options;
using Passi.Core.Application.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Extensions;

namespace Passi.Core.Services
{
    internal class PassiService : IPassiService
    {
        private readonly UrlOptions options;
        private readonly IPassiUserContactsService userContactsService;
        private readonly IInfoRepository<ContactCenterInfo> contactCenterInfoRepo;
        private readonly IInfoRepository<SessionInfo> sessionInfoRepo;
        private readonly IInfoRepository<ProfileInfo> profileInfoRepo;
        private readonly IInfoRepository<ConventionInfo> conventionInfoRepo;
        private readonly ILevelsRepository levelsRepository;
        private readonly IUserRepository userRepo;

        public PassiService(IOptions<UrlOptions> options,
            IPassiUserContactsService userContactsService,
            IInfoRepository<ContactCenterInfo> contactCenterInfoRepo,
            IInfoRepository<SessionInfo> sessionInfoRepo,
            IInfoRepository<ProfileInfo> profileInfoRepo,
            IInfoRepository<ConventionInfo> conventionInfoRepo,
            ILevelsRepository levelsRepository,
            IUserRepository userRepo
            )
        {
            this.options = options.Value;
            this.userContactsService = userContactsService;
            this.contactCenterInfoRepo = contactCenterInfoRepo;
            this.sessionInfoRepo = sessionInfoRepo;
            this.profileInfoRepo = profileInfoRepo;
            this.conventionInfoRepo = conventionInfoRepo;
            this.levelsRepository = levelsRepository;
            this.userRepo = userRepo;
        }

        public Uri LogoutUrl() => options.Logout;
        public Uri SwitchProfileUrl() => options.SwitchProfile;

        public async Task<UserContacts> UserContactsAsync() => (await RetrieveUserContactsAsync());
        public async Task<UserContacts> UserContactsAsync(string fiscalCode) => (await RetrieveUserContactsAsync(fiscalCode));

        public async Task<User> MeAsync()
        {
            SessionInfo sessionInfo = await sessionInfoRepo.RetrieveAsync();
            try
            {
                //Devo verificare che non ci sia un Operatore CC che stia lavorando per un utente,
                //in tal caso devo prendere le generalità dell'utente e non quelle dell'Operatore
                var contactCenterInfo = await contactCenterInfoRepo.RetrieveAsync();
                sessionInfo.CheckContactCenterInfo(contactCenterInfo);
            }
            catch (NotFoundException)
            {
                //Do nothing
            }
            return sessionInfo.MapUser();
        }

        public async Task<Profile> ProfileAsync()
        {
            var profileInfo = await profileInfoRepo.RetrieveAsync();
            var sessionInfo = await sessionInfoRepo.RetrieveAsync();
            return new Profile()
            {
                InstitutionCode = profileInfo.InstitutionCode,
                InstitutionFiscalCode = sessionInfo.InstitutionFiscalCode,
                InstitutionDescription = sessionInfo.InstitutionDescription,
                OfficeCode = profileInfo.OfficeCode,
                ProfileTypeId = profileInfo.ProfileTypeId
            };
        }

        public async Task<bool> HasPatronageDelegationAsync(string delegatedFiscalCode)
        {
            var session = await sessionInfoRepo.RetrieveAsync();
            return await userRepo.IsDelegationAvailableAsync(delegatedFiscalCode, session.InstitutionCode);
        }

        public async Task<ICollection<int>> AuthorizedServicesAsync()
        {
            var profileInfo = await profileInfoRepo.RetrieveAsync();
            return profileInfo.Services.Select(x => x.Id).OrderBy(x => x).ToList();
        }

        public Task<bool> IsAuthorizedAsync(int serviceId)
        {
            return InternalIsAuthorizedAsync(serviceId);
        }

        #region Private Functions

        private Task<UserContacts> RetrieveUserContactsAsync()
        {
            return userContactsService.UserContactsAsync();
        }

        private Task<UserContacts> RetrieveUserContactsAsync(string fiscalCode)
        {
            return userContactsService.UserContactsAsync(fiscalCode);
        }

        private async Task<bool> InternalIsAuthorizedAsync(int serviceId)
        {
            var sessionInfo = await sessionInfoRepo.RetrieveAsync();
            var profileInfo = await profileInfoRepo.RetrieveAsync();
            var conventionInfo = await conventionInfoRepo.RetrieveAsync();

            if (!profileInfo.HasProfile)
                return false;

            /// Supponiamo che l'utente sia profilato e che il profilo sia quello richiesto
            var myService = profileInfo.Services.FirstOrDefault(x => x.Id == serviceId);

            /// Non ho questo servizio abilitato fra quelli disponibili
            if (myService == null)
            {
                return false;
            }

            /// Verifichiamo se l'autenticazione dell'utente è sufficientemente robusta
            var isProfileAuthorized = await levelsRepository.CompareAuthorizationAsync(sessionInfo.AuthenticationType.ShortDescribe(),
                myService.RequiredAuthenticationType);

            if (!isProfileAuthorized)
            {
                return false;
            }

            if (conventionInfo.Conventions.Any())
            {
                var matchingConvention = conventionInfo.Conventions.FirstOrDefault(x => x.ServiceId == serviceId);

                if (matchingConvention == null)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}

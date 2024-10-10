using Microsoft.Extensions.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Store.Fake.Options;

namespace Passi.Core.Store.Fake
{
    internal class UserRepository : IUserRepository
    {
        private readonly IInfoRepository<ConventionInfo> conventionInfoRepository;
        private readonly IInfoRepository<ProfileInfo> profileInfoRepository;
        private readonly IInfoRepository<SessionInfo> sessionInfoRepsitory;
        private readonly ICollection<Profile> userProfiles;

        public UserRepository(IInfoRepository<ConventionInfo> conventionInfoRepository,
            IInfoRepository<ProfileInfo> profileInfoRepository,
            IOptionsMonitor<UserOptions> userOptions,
            IInfoRepository<SessionInfo> sessionInfoRepsitory)
        {
            this.conventionInfoRepository = conventionInfoRepository;
            this.profileInfoRepository = profileInfoRepository;
            this.sessionInfoRepsitory = sessionInfoRepsitory;

            this.userProfiles = new HashSet<Profile>();

            userProfiles.Add(new()
            {
                InstitutionCode = userOptions.CurrentValue.Profile.InstitutionCode,
                ProfileTypeId = userOptions.CurrentValue.Profile.ProfileTypeId,
                InstitutionDescription = userOptions.CurrentValue.Profile.InstitutionDescription,
                OfficeCode = userOptions.CurrentValue.Profile.OfficeCode
            });
        }
        public async Task<ICollection<Service>> AuthorizedServicesAsync(string userId,
            string institutionCode,
            string authenticationType,
            bool retrieveSuspendedProfiles = false)
        {
            ProfileInfo profileInfo = await profileInfoRepository.RetrieveAsync();
            return profileInfo.Services;
        }

        public async Task<ICollection<Convention>> ConventionsAsync(string userId, string institutionCode)
        {
            ConventionInfo conventionInfo = await conventionInfoRepository.RetrieveAsync();
            return conventionInfo.Conventions;
        }

        public Task<bool> HasDelegationAsync(string userId, string delegatedUserId)
        {
            return Task.FromResult(false);
        }

        public Task<bool> HasSessionAsync(string userId, string sessionId, int uniqueSessionsCount)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsGrantedAsync(string userId, int serviceId, string institutionCode, string authenticationType = CommonAuthenticationTypes.Undefined)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsDelegationAvailableAsync(string fiscalCode, string institutionCode)
        {
            return Task.FromResult(true);
        }

        public Task<ICollection<Profile>> ProfilesAsync(string userId, int serviceId, int? userTypeId, string authenticationType = CommonAuthenticationTypes.Undefined, bool retrieveSuspendedProfiles = false)
        {
            ICollection<Profile> result = userTypeId.HasValue ? userProfiles.Where(w => w.ProfileTypeId == userTypeId).ToHashSet() : userProfiles;
            return Task.FromResult(result);
        }

        public async Task<ICollection<Service>> ServicesAsync(string userId, string institutionCode)
        {
            ProfileInfo profileInfo = await profileInfoRepository.RetrieveAsync();
            return profileInfo.Services;
        }

        public async Task<UserInfo> UserAsync(string id, string authenticationType, string institutionCode)
        {
            return await sessionInfoRepsitory.RetrieveAsync();
        }
    }
}

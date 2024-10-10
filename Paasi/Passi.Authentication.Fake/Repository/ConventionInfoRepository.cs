using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Entities.Info;

namespace Passi.Authentication.Fake.Repository
{
    class ConventionInfoRepository : IInfoRepository<ConventionInfo>
    {
        private readonly IInfoRepository<SessionInfo> sessionInfoRepository;

        public ConventionInfoRepository(IInfoRepository<SessionInfo> sessionInfoRepository)
        {
            this.sessionInfoRepository = sessionInfoRepository;
        }

        public async Task<ConventionInfo> RetrieveAsync()
        {
            SessionInfo sessionInfo = await sessionInfoRepository.RetrieveAsync();
            ConventionInfo conventionInfo = new()
            {
                UserTypeId = sessionInfo.ProfileTypeId,
                UserId = sessionInfo.UserId,
                WorkOfficeCode = sessionInfo.OfficeCode
            };
            return conventionInfo;
        }

        public Task<ConventionInfo> UpdateAsync(ConventionInfo item)
        {
            return Task.FromResult(item);
        }
    }
}

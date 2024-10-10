using Bogus;
using Microsoft.Extensions.Options;
using Passi.Core.Application.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;

namespace Passi.Authentication.Fake.Repository
{
    class ProfileInfoRepository : IInfoRepository<ProfileInfo>
    {
        private readonly IInfoRepository<SessionInfo> sessionInfoRepository;
        private readonly ConfigurationOptions configurationOptions;

        public ProfileInfoRepository(IInfoRepository<SessionInfo> sessionInfoRepository, IOptions<ConfigurationOptions> configurationOptions)
        {
            this.sessionInfoRepository = sessionInfoRepository;
            this.configurationOptions = configurationOptions.Value;
        }

        public async Task<ProfileInfo> RetrieveAsync()
        {
            SessionInfo sessionInfo = await sessionInfoRepository.RetrieveAsync();
            ProfileInfo profileInfo = new()
            {
                FiscalCode = sessionInfo.FiscalCode,
                Id = sessionInfo.ProfileTypeId.ToString(),
                ProfileTypeId = sessionInfo.ProfileTypeId,
                InstitutionCode = sessionInfo.InstitutionCode,
                OfficeCode = sessionInfo.OfficeCode,
                LastUpdate = sessionInfo.LastUpdated,
                Timeout = sessionInfo.SessionTimeout,
                Opening = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Closing = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 23, 59, 59, DateTimeKind.Utc)
            };

            profileInfo.Services.Add(new()
            {
                Id = configurationOptions.ServiceId,
                HasConvention = false,
                RequiredAuthenticationType = "2SPI".ShortDescribe(),
            });

            Faker<Service> service = new Faker<Service>("it");
            service.RuleFor(p => p.Id, f => f.Random.Int(100, 200));
            service.RuleFor(p => p.HasConvention, f => f.Random.Bool());
            service.RuleFor(p => p.RequiredAuthenticationType, f => "2SPI".ShortDescribe());

            profileInfo.Services.ToList().AddRange(service.Generate(10));

            return profileInfo;
        }

        public Task<ProfileInfo> UpdateAsync(ProfileInfo item)
        {
            return Task.FromResult(item);
        }
    }
}

using Microsoft.Extensions.Options;
using Passi.Core.Application.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;

namespace Passi.Core.Services
{
    internal class PassiConventionService : IPassiConventionService
    {
        private readonly IInfoRepository<ConventionInfo> conventionInfoRepo;
        private readonly ConfigurationOptions configOptions;

        public PassiConventionService(IInfoRepository<ConventionInfo> conventionInfoRepo, IOptions<ConfigurationOptions> configOptions)
        {
            this.conventionInfoRepo = conventionInfoRepo;
            this.configOptions = configOptions.Value;
        }

        public async Task<ICollection<Role>> ConventionRolesAsync()
        {
            var info = await conventionInfoRepo.RetrieveAsync();
            var service = info.Conventions.FirstOrDefault(x => x.ServiceId == configOptions.ServiceId);
            if (service != null)
                return service.Roles;
            return new HashSet<Role>();
        }

        public async Task<bool> ConventionHasRoleAsync(string role)
        {
            var info = await conventionInfoRepo.RetrieveAsync();
            var service = info.Conventions.FirstOrDefault(x => x.ServiceId == configOptions.ServiceId);
            if (service != null)
                return service.Roles.Any(a => a.Value.ToLower().Trim() == role.ToLower().Trim());
            return false;
        }

        public async Task<ICollection<Filter>> ConventionFiltersAsync()
        {
            var info = await conventionInfoRepo.RetrieveAsync();
            var service = info.Conventions.FirstOrDefault(x => x.ServiceId == configOptions.ServiceId);
            if (service != null)
                return service.Filters;
            return new HashSet<Filter>();
        }

        public async Task<ICollection<Filter>> ConventionFiltersAsync(string type)
        {
            var info = await conventionInfoRepo.RetrieveAsync();
            var service = info.Conventions.FirstOrDefault(x => x.ServiceId == configOptions.ServiceId);
            if (service != null)
                return service.Filters.Where(w => w.Type.ToLower().Trim() == type.ToLower().Trim()).ToHashSet();
            return new HashSet<Filter>();
        }

    }
}

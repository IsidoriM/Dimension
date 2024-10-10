using Passi.Core.Domain.Entities;

namespace Passi.Core.Application.Repositories
{
    internal interface ILevelsRepository
    {
        public Task<ICollection<AuthorizationLevel>> LevelsAsync();
        public Task<bool> CompareAuthorizationAsync(char myLevel, char requiredLevel);

    }
}

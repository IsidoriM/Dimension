using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;

namespace Passi.Core.Store.Fake
{
    internal class LevelsRepository : ILevelsRepository
    {
        public Task<bool> CompareAuthorizationAsync(char myLevel, char requiredLevel)
        {
            return Task.FromResult(true);
        }

        public Task<ICollection<AuthorizationLevel>> LevelsAsync()
        {
            ICollection<AuthorizationLevel> levels = new List<AuthorizationLevel>()
            {
                new ()
                {
                    AuthenticationType = "2SPI".ShortDescribe(),
                    Priority = 0
                }
            };
            return Task.FromResult(levels);
        }
    }
}

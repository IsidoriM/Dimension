using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using System.Data;
using System.Data.SqlClient;

namespace Passi.Core.Store.Sql
{
    internal class SqlLevelsRepository : ILevelsRepository
    {
        private readonly IDbConnectionFactory connectionFactory;
        private ICollection<AuthorizationLevel> levels;

        public SqlLevelsRepository(IDbConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
            this.levels = new List<AuthorizationLevel>();
        }

        public Task<ICollection<AuthorizationLevel>> LevelsAsync()
        {
            if (!levels.Any())
            {
                using var connection = connectionFactory.CreateConnection(Config.CONN_UTENZE);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "spGetLivelloAutorizzativo";
                using var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    var aType = dr.GetStringValue("codLivelloAutorizzativo").ShortDescribe();
                    var priority = dr.GetIntValue("Ordine", int.MinValue);
                    var exists = levels
                        .Where(x => x.AuthenticationType == aType).Any(x => x.Priority == priority);
                    if (priority > int.MinValue && !exists && aType != CommonAuthenticationTypes.Undefined.ShortDescribe())
                    {
                        levels.Add(new AuthorizationLevel()
                        {
                            AuthenticationType = aType,
                            Priority = priority,
                        });
                    }

                }
                dr.Close();
            }
            return Task.FromResult(levels);
        }

        public async Task<bool> CompareAuthorizationAsync(char myLevel,
            char requiredLevel)
        {
            var _levels = await LevelsAsync();
            if (_levels.Any())
            {
                var myPriority = _levels.FirstOrDefault(x => x.AuthenticationType == myLevel);
                var requiredPriority = _levels.FirstOrDefault(x => x.AuthenticationType == requiredLevel);
                if (myPriority != null && requiredPriority != null)
                {
                    return myPriority.Priority > requiredPriority.Priority;
                }
            }
            return false;
        }

        public void Set(ICollection<AuthorizationLevel> levels)
        {
            this.levels = levels;
        }
    }


}

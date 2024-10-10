using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Passi.Core.Application.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Const;
using Passi.Core.Exceptions;
using System.Data;
using System.Data.SqlClient;

namespace Passi.Core.Store.Sql
{
    internal class SqlCLogRepository : ICLogRepository
    {
        private readonly IDbConnectionFactory connectionFactory;
        private readonly ILogger? logger;
        private readonly ConfigurationOptions options;

        public SqlCLogRepository(IDbConnectionFactory connectionFactory, IOptions<ConfigurationOptions> options, ILogger<SqlCLogRepository>? logger)
        {
            this.connectionFactory = connectionFactory;
            this.logger = logger;
            this.options = options.Value;
        }
        public Task LogAsync(string userId, 
            int eventId, 
            string ip, 
            int executionTime, 
            int returnCode,
            int tipoUtente,
            string? institutionCode, 
            string? workOfficeCode, 
            string? parameters, 
            string? errorMessage)
        {
            try
            {
                using var connection = connectionFactory.CreateConnection(Config.CONN_LOG);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "spAppendToLogEnte";

                command.AddParameter("@Utente", userId, DbType.String);
                command.AddParameter("@idEvento", eventId, DbType.Int32);
                command.AddParameter("@idClasseUtente", tipoUtente, DbType.Int32);
                command.AddParameter("@ipClient ", ip, DbType.String, 15);
                command.AddParameter("@TempoEsecuzione ", executionTime, DbType.Int32);
                command.AddParameter("@ReturnCode ", returnCode, DbType.Int32);

                if (!string.IsNullOrWhiteSpace(institutionCode))
                {
                    command.AddParameter("@CodiceEnte", institutionCode, DbType.String);
                }
                else
                {
                    command.AddParameter("@CodiceEnte", DBNull.Value, DbType.String);
                }
                if (!string.IsNullOrWhiteSpace(workOfficeCode))
                {
                    command.AddParameter("@CodiceUfficio", workOfficeCode, DbType.String);
                }
                else
                {
                    command.AddParameter("@CodiceUfficio", DBNull.Value, DbType.String);
                }

                if (!string.IsNullOrWhiteSpace(parameters))
                {
                    command.AddParameter("@Parametri ", parameters, DbType.String);
                }
                else
                {
                    command.AddParameter("@Parametri ", DBNull.Value, DbType.String);
                }
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    command.AddParameter("@DescrizioneErrore ", errorMessage, DbType.String);
                }
                else
                {
                    command.AddParameter("@DescrizioneErrore ", DBNull.Value, DbType.String);
                }

                var outParameter = command.CreateParameter();
                outParameter.ParameterName = "@ErrorCode";
                outParameter.DbType = DbType.Int32;
                outParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(outParameter);

                command.ExecuteNonQuery();

                int result = (int?)outParameter.Value ?? 99;

                if(result != 0)
                {
                    throw new CLogException($"Il salvataggio non è andato a buon fine. Codice di errore: {result}");
                }
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    _ = Enum.TryParse(options.Log, out LogLevel logLevel);
                    logger.Log(logLevel, ex, "{message}", ex.Message);
                }

                throw new CLogException(ex.Message);
            }
            return Task.CompletedTask;
        }
    }
}

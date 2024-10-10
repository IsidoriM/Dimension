using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using System.Data;
using System.Data.SqlClient;

namespace Passi.Core.Store.Sql
{
    internal class SqlUserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory connectionFactory;

        public SqlUserRepository(IDbConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public Task<UserInfo> UserAsync(string id, string authenticationType, string institutionCode)
        {
            using var connection = connectionFactory.CreateConnection(Config.CONN_UTENZE);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "spGetUserInfobyEnte";

            command
                .AddParameter("UserName", id, DbType.String)
                .AddParameter("codEnte", institutionCode, DbType.String)
                .AddParameter("authenticationType", authenticationType, DbType.String);

            using var dr = command.ExecuteReader(CommandBehavior.CloseConnection);

            var user = new UserInfo
            {
                UserId = id
            };

            while (dr.Read())
            {
                if (id.Contains(':'))
                {
                    id = id.Split(":").First();
                    user.UserId = id;
                    user.FiscalCode = id;
                }
                else
                {
                    user.FiscalCode = dr.GetStringValue("codicefiscale");
                    user.Name = dr.GetStringValue("nome");
                    user.Surname = dr.GetStringValue("cognome");
                    user.Gender = dr.GetStringValue("sesso").ToUpper();
                    user.Email = dr.GetStringValue("indirizzoemail");
                    /// sarebbe il caso di togliere il codice ufficio da user info??
                    /// comunque, lo devo estrarre? devo considerare il codice ente quindi?
                    user.OfficeCode = dr.GetStringValue("codiceUfficio");
                    user.BirthPlaceCode = dr.GetStringValue("comuneNascita");
                    user.BirthProvince = dr.GetStringValue("provinciaNascita");
                    user.BirthDate = dr.GetDateValue("dataNascita");
                    user.Phone = dr.GetStringValue("telefono");
                    user.Mobile = dr.GetStringValue("cellulare");
                    user.PEC = dr.GetStringValue("indirizzoPEC");
                }
            }


            if (string.IsNullOrWhiteSpace(user.UserId) || string.IsNullOrWhiteSpace(user.FiscalCode))
                throw new NotFoundException();

            return Task.FromResult(user);
        }

        public Task<bool> IsDelegationAvailableAsync(string fiscalCode, string institutionCode)
        {
            using var connection = connectionFactory.CreateConnection(Config.CONN_DELEGAPATR);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "spVerificaDelega";
            command.AddParameter("@CodiceEnte", institutionCode, DbType.StringFixedLength, 10);
            command.AddParameter("@CodiceFiscale", fiscalCode, DbType.StringFixedLength, 16);

            var outParameter = command.CreateParameter();
            outParameter.ParameterName = "@RetCode";
            outParameter.Direction = ParameterDirection.Output;
            outParameter.DbType = DbType.Int32;
            command.Parameters.Add(outParameter);

            using var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
            dr.Close();

            var result = outParameter.Value?.ToString();

            return Task.FromResult(result?.ToString() == "1");
        }

        public Task<bool> HasDelegationAsync(string userId, string delegatedUserId)
        {
            using var connection = connectionFactory.CreateConnection(Config.CONN_UTENZE);
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "spGetDelegati";

            command.AddParameter("utente", userId, DbType.String);

            var dataset = connectionFactory.CreateDataset(command);

            if (dataset.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataset.Tables[0].Rows)
                {
                    var fc = row.GetStringValue("CodiceFiscaleDelegato");
                    if (!string.IsNullOrEmpty(fc) && fc.ToUpper().Equals(delegatedUserId.ToUpper()))
                        return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);

        }

        public Task<bool> HasSessionAsync(string userId, string sessionId, int uniqueSessionsCount)
        {
            using var connection = connectionFactory.CreateConnection(Config.CONN_UTENZE);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "spCheckSessionData";
            command.AddParameter("@Utente", userId, DbType.String);
            command.AddParameter("@SessionId", sessionId, DbType.String);
            /// Viene usato il tempo del server, e non UTC
            command.AddParameter("@DataAggiornamento", DateTime.Now, DbType.DateTime);
            command.AddParameter("@SessionConfig", uniqueSessionsCount, DbType.Int64);

            var outParameter = command.CreateParameter();
            outParameter.ParameterName = "@RetCode";
            outParameter.Direction = ParameterDirection.Output;
            outParameter.DbType = DbType.Int32;
            command.Parameters.Add(outParameter);

            using var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
            dr.Close();
            var result = outParameter.Value?.ToString();
            return Task.FromResult(result != "1");
        }

        public Task<ICollection<Profile>> ProfilesAsync(string userId,
            int serviceId,
            int? userTypeId,
            string authenticationType = CommonAuthenticationTypes.Undefined,
            bool retrieveSuspendedProfiles = false)
        {
            var userProfiles = new HashSet<Profile>();

            using var connection = connectionFactory.CreateConnection(Config.CONN_UTENZE);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "spGetUserProfiles";
            command.AddParameter("@utente", userId, DbType.String);

            if (userTypeId == null)
                command.AddParameter("@idTipoUtente", DBNull.Value, DbType.Int64);
            else
                command.AddParameter("@idTipoUtente", userTypeId!, DbType.Int64);

            command.AddParameter("@idServizio", serviceId, DbType.Int64);
            command.AddParameter("@authenticationType", authenticationType, DbType.String);
            //il parametro getProfiliSospesi permette l'estrazione anche dei profili sospesi
            //se non valorizzato non vengono estratti
            if (retrieveSuspendedProfiles)
                command.AddParameter("@getProfiliSospesi", "1", DbType.String);

            var ds = connectionFactory.CreateDataset(command);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var uP = new Profile
                {
                    InstitutionCode = row.GetStringValue("codEnte"),
                    InstitutionDescription = row.GetStringValue("DescrizioneEnte"),
                    ProfileTypeId = row.GetIntValue("idTipoUtente", 0),
                    OfficeCode = row.GetStringValue("idUfficio")
                };

                userProfiles.Add(uP);
            }

            ICollection<Profile> result = userProfiles.Where(x => !string.IsNullOrWhiteSpace(x.InstitutionCode)).ToList();
            return Task.FromResult(result);
        }

        public async Task<ICollection<Convention>> ConventionsAsync(string userId, string institutionCode)
        {
            var apps = await ServicesAsync(userId, institutionCode);
            var conventionedApps = apps
                .Where(x => x.HasConvention)
                .Where(x => x.GroupId > 0)
                .Select(x => x.GroupId);

            var conventions = new List<Convention>();

            using var connection = connectionFactory.CreateConnection(Config.CONN_UTENZE);
            connection.Open();

            if (conventionedApps.Any())
            {
                using var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "spGetConvenzione";

                command.AddParameter("utente", userId, DbType.String);
                command.AddParameter("codiceEnte", institutionCode, DbType.String);
                command.AddParameter("listaGruppoServiziInConvenzione", string.Join(",", conventionedApps), DbType.String);

                using var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    var filterType = dr.GetStringValue("TipoFiltro");
                    var scope = dr.GetStringValue("Ambito");
                    var value = dr.GetStringValue("Valore");
                    var serviceId = dr.GetIntValue("IdServizio");
                    DateTime expires = dr.GetDateValue("DataScadenza");

                    var convention = conventions.Find(x => x.ServiceId == serviceId);
                    if (convention == null)
                    {
                        convention = new Convention()
                        {
                            ServiceId = serviceId,
                            IsAvailable = expires < DateTime.UtcNow
                        };
                        conventions.Add(convention);
                    }

                    if (filterType.ToUpper() == "R")
                    {
                        convention.Roles.Add(new Role() { Value = value });
                    }
                    else
                    {
                        convention.Filters.Add(new Filter() { Value = value, Scope = scope, Type = filterType });
                    }
                }
                dr.Close();
            }


            return conventions;
        }

        public Task<ICollection<Service>> ServicesAsync(string userId, string institutionCode)
        {
            ICollection<Service> apps = new List<Service>();

            using var connection = connectionFactory.CreateConnection(Config.CONN_UTENZE);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "spGetServicesPASSI";
            command.AddParameter("UserName", userId, DbType.String);
            command.AddParameter("CodEnte", institutionCode, DbType.String);

            var ds = connectionFactory.CreateDataset(command);

            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    var application = new Service
                    {
                        HasConvention = row.GetStringValue("RichiedeConvenzione") == "1",
                        Id = row.GetIntValue("idservizio"),
                        GroupId = row.GetIntValue("IdGruppoServizi"),
                        RequiredAuthenticationType = row.GetStringValue("codLivelloAutorizzativo").ShortDescribe()
                    };
                    apps.Add(application);
                }
            }

            return Task.FromResult(apps);
        }

        public Task<bool> IsGrantedAsync(string userId,
            int serviceId,
            string institutionCode,
            string authenticationType = CommonAuthenticationTypes.Undefined)
        {
            using var connection = connectionFactory.CreateConnection(Config.CONN_UTENZE);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "spUserIsGrantedToServiceByEnte";
            command.AddParameter("UserName", userId, DbType.String);
            command.AddParameter("idServizio", serviceId, DbType.Int64);
            if (!string.IsNullOrWhiteSpace(institutionCode))
                command.AddParameter("@codEnte", institutionCode, DbType.String);
            if (authenticationType != CommonAuthenticationTypes.Undefined)
                command.AddParameter("@livelloAutorizzativo", authenticationType, DbType.String);

            var outParameter = command.CreateParameter();
            outParameter.ParameterName = "@RetCode";
            outParameter.Direction = ParameterDirection.Output;
            outParameter.DbType = DbType.Int32;
            command.Parameters.Add(outParameter);

            using var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
            dr.Close();
            var result = outParameter.Value?.ToString();
            return Task.FromResult(result == "1");
        }

        public Task<ICollection<Service>> AuthorizedServicesAsync(string userId,
            string institutionCode,
            string authenticationType,
            bool retrieveSuspendedProfiles = false)
        {
            ICollection<Service> result = new HashSet<Service>();

            using var connection = connectionFactory.CreateConnection(Config.CONN_UTENZE);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "spGetAuthorizedServicesByEnte";
            command.AddParameter("@UserName", userId, DbType.String);
            command.AddParameter("@authenticationType", authenticationType, DbType.String);

            if (!string.IsNullOrWhiteSpace(institutionCode))
                command.AddParameter("@CodEnte", institutionCode, DbType.String);

            if (retrieveSuspendedProfiles)
                command.AddParameter("@getProfiliSospesi", "1", DbType.String);

            var dataSet = connectionFactory.CreateDataset(command);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                var serviceId = row.GetIntValue("idservizio");
                var level = row.GetStringValue("codLivelloAutorizzativo");
                result.Add(new Service()
                {
                    Id = serviceId,
                    RequiredAuthenticationType = level.ShortDescribe()
                });
            }

            return Task.FromResult(result);
        }
    }


}

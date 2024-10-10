using Microsoft.Extensions.Configuration;
using Passi.Core.Exceptions;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

//[assembly: InternalsVisibleTo("Microsoft.Extensions.DependencyInjection")]
//[assembly: InternalsVisibleTo("Passi.Test.Unit")]
//[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
namespace Passi.Core.Store.Sql
{
    interface IDbConnectionFactory
    {
        IDbConnection CreateConnection(string name);
        DataSet CreateDataset(IDbCommand command);
        DataAdapter CreateDataAdapter(IDbCommand command);
    }

    internal class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration configuration;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IDbConnection CreateConnection(string name)
        {
            var connectionString = configuration[name];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ParameterException("Connection string not found");
            }

            var connection = new SqlConnection(connectionString);

            return connection;
        }

        public DataAdapter CreateDataAdapter(IDbCommand command)
        {
            var adapter = new SqlDataAdapter();
            // Purtroppo non trovo alternativa per far girare i test
            if (command is SqlCommand dbCommand)
            {
                adapter.SelectCommand = dbCommand;
            }

            return adapter;
        }

        public DataSet CreateDataset(IDbCommand command)
        {
            var adapter = CreateDataAdapter(command);

            var ds = new DataSet(Guid.NewGuid().ToString());
            adapter.Fill(ds);

            return ds;
        }
    }
}

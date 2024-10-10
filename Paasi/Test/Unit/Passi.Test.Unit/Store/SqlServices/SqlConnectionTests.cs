using Microsoft.Extensions.Configuration;
using Moq;
using Passi.Core.Exceptions;
using Passi.Core.Store.Sql;
using System.Data;
using System.Data.SqlClient;

namespace Passi.Test.Unit.Core.SqlServices
{
    /// <summary>
    /// Nomenclatura consigliata dei test di unità
    /// MethodName_StateUnderTest_ExpectedBehavior
    /// </summary>
    public class SqlConnectionTests
    {
        [Fact]
        public void Connection_Build_Ko()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "test", string.Empty },
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var serviceUnderTest = new SqlConnectionFactory(configuration);

            Assert.Throws<ParameterException>(() => serviceUnderTest.CreateConnection("test"));
        }

        [Fact]
        public void Connection_Adapter_Ok()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"test", "Server=db.inps,1433;Database=test;User Id=test;Password=test;"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var serviceUnderTest = new SqlConnectionFactory(configuration);
            var connection = serviceUnderTest.CreateConnection("test");
            var command = connection.CreateCommand();
            var adapter = serviceUnderTest.CreateDataAdapter(command);
            Assert.NotNull(adapter);
        }


        [Fact]
        public void Connection_CreateDataset_Ko()
        {
            SqlCommand command = new();
            Dictionary<string, string?> inMemorySettings = new()
            {
                { "test", string.Empty }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            SqlConnectionFactory serviceUnderTest = new(configuration);

            Assert.Throws<InvalidOperationException>(() => serviceUnderTest.CreateDataset(command));
        }

    }
}
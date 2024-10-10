using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using Moq;
using Passi.Core.Application.Repositories;
using Passi.Core.Exceptions;
using Passi.Core.Store.Sql;
using Passi.Test.Unit.Fixtures;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Passi.Test.Unit.Store.SqlServices
{
    public class SqlClogRepositoryTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture passiFixture;
        private readonly ICLogRepository cLogRepository;
        readonly Mock<IDbCommand> commandMock;
        int? lastValue = null;

        public SqlClogRepositoryTests(PassiFixture passiFixture)
        {
            this.passiFixture = passiFixture;

            Mock<DbDataReader> readerMock = new();
            readerMock.SetupSequence(_ => _.Read())
                .Returns(true)
                .Returns(false);

            Mock<DbParameterCollection> dataParameterCollection = new(MockBehavior.Strict);
            dataParameterCollection.Setup(c => c.Add(It.IsAny<object>())).Returns(1);

            Mock<DbParameter> parameterMock = new(MockBehavior.Strict);
            parameterMock.Setup(m => m.Value).Returns(0);
            parameterMock.SetupSet(m => m.ParameterName = It.IsAny<string>());
            parameterMock.SetupSet(m => m.Value = It.IsAny<object?>());
            parameterMock.SetupGet(m => m.Value).Returns(() => lastValue);
            parameterMock.SetupSet(m => m.DbType = It.IsAny<DbType>());
            parameterMock.SetupSet(m => m.Size = It.IsAny<int>());
            parameterMock.SetupSet(m => m.Direction = It.IsAny<ParameterDirection>());

            commandMock = new(MockBehavior.Strict);
            commandMock.Setup(m => m.CreateParameter()).Returns(parameterMock.Object);
            commandMock.Setup(m => m.Parameters).Returns(dataParameterCollection.Object);
            commandMock.Setup(m => m.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);
            commandMock.Setup(c => c.Dispose());
            commandMock.SetupSet(c => c.CommandType = It.IsAny<CommandType>());
            commandMock.SetupSet(c => c.CommandText = It.IsAny<string>());

            Mock<IDbConnection> connectionMock = new(MockBehavior.Strict);
            connectionMock.Setup(m => m.Open());
            connectionMock.Setup(m => m.CreateCommand()).Returns(commandMock.Object);
            connectionMock.Setup(c => c.Dispose());

            Mock<IDbConnectionFactory> factory = new(MockBehavior.Strict);
            factory.Setup(x => x.CreateConnection(It.IsAny<string>())).Returns(connectionMock.Object);

            cLogRepository = new SqlCLogRepository(
                factory.Object,
                passiFixture.Mocks(1).ConfigurationOptions,
                null!
                );
        }

        [Fact]
        public async Task WriteCLog_Ok()
        {
            commandMock.Setup(c => c.ExecuteNonQuery()).Callback(() => lastValue = 0).Returns(1);
            await cLogRepository.LogAsync("AAAA", 1, "127.0.0.1", 0, 200, 0, "Test", "Test", "Test", "Test");
            Assert.True(true);
        }

        [Theory]
        [InlineAutoData(null)]
        [InlineAutoData(1)]
        public async Task WriteCLog_WrongOutput_Error(int? outParameterValue)
        {
            commandMock.Setup(c => c.ExecuteNonQuery()).Callback(() => lastValue = outParameterValue).Returns(1);

            try
            {
                await cLogRepository.LogAsync("AAAA", 1, "127.0.0.1", 0, 200, 0, "Test", "Test", "Test", "Test");
                Assert.True(false);
            }
            catch (CLogException)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public async Task WriteCLog_Error()
        {
            var readerMock = new Mock<DbDataReader>();
            readerMock.SetupSequence(_ => _.Read())
                .Throws(new Exception("Test error"));

            var dataParameterCollection = new Mock<DbParameterCollection>();

            var parameterMock = new Mock<DbParameter>();
            parameterMock.Setup(m => m.Value).Returns(0);

            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(m => m.CreateParameter()).Returns(parameterMock.Object);
            commandMock.Setup(m => m.Parameters).Returns(dataParameterCollection.Object);
            commandMock.Setup(m => m.ExecuteNonQuery()).Throws(new Exception("Test error"));

            var connectionMock = new Mock<IDbConnection>();
            connectionMock.Setup(m => m.CreateCommand()).Returns(commandMock.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(x => x.CreateConnection(It.IsAny<string>())).Returns(connectionMock.Object);

            var logger = new Mock<ILogger<SqlCLogRepository>>();

            SqlCLogRepository cLogRepositoryError = new(factory.Object, passiFixture.Mocks(1).ConfigurationOptions, logger.Object);
            try
            {
                await cLogRepositoryError.LogAsync("AAAA", 1, "127.0.0.1", 0, 200, 0, null, null, null, null);
                Assert.True(false);
            }
            catch (CLogException)
            {
                Assert.True(true);
            }
        }
    }
}

using Moq;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Store.Sql;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;

namespace Passi.Test.Unit.Core.SqlServices
{
    /// <summary>
    /// Nomenclatura consigliata dei test di unità
    /// MethodName_StateUnderTest_ExpectedBehavior
    /// </summary>
    public class SqlLevelsTests
    {
        [Fact]
        public async Task LevelsAsync_Find_Ok()
        {
            //Arrange
            var repo = ServiceUnderTest();

            //Act
            var result = await repo.LevelsAsync();

            //Assert
            Assert.True(result.Any());
            Assert.True(result.First().Priority > int.MinValue);
        }

        [Fact]
        public async Task IsAuthorizedAsync_Unauthorized_Ok()
        {
            //Arrange
            var repo = ServiceUnderTest();
            repo.Set(new HashSet<AuthorizationLevel>()
            {
                new AuthorizationLevel() { AuthenticationType = "1SPI".ShortDescribe(), Priority= 25 },
                new AuthorizationLevel() { AuthenticationType = "3SPI".ShortDescribe(), Priority = 40 }
            });

            //Act
            var result = await repo.CompareAuthorizationAsync("1SPI".ShortDescribe(), "3SPI".ShortDescribe());

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_Authorized_Ok()
        {
            //Arrange
            var repo = ServiceUnderTest();
            repo.Set(new HashSet<AuthorizationLevel>()
            {
                new AuthorizationLevel() { AuthenticationType = "1SPI".ShortDescribe(), Priority= 25 },
                new AuthorizationLevel() { AuthenticationType = "3SPI".ShortDescribe(), Priority = 40 }
            });

            //Act
            var result = await repo.CompareAuthorizationAsync("3SPI".ShortDescribe(), "1SPI".ShortDescribe());

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CompareAuthorization_NoLevels_Ko()
        {
            SqlLevelsRepository repo = ServiceUnderTest();

            bool result = await repo.CompareAuthorizationAsync("3SPI".ShortDescribe(), "1SPI".ShortDescribe());

            Assert.False(result);
        }

        private static SqlLevelsRepository ServiceUnderTest()
        {
            var readerMock = new Mock<DbDataReader>();
            readerMock.SetupSequence(_ => _.Read())
                .Returns(true)
                .Returns(true)
                .Returns(true)
                .Returns(false);

            readerMock.Setup(reader => reader.GetOrdinal("codLivelloAutorizzativo")).Returns(0);
            readerMock.Setup(reader => reader.GetValue(0)).Returns(StringExtensions.RandomString<CommonAuthenticationTypes>(CommonAuthenticationTypes.Undefined));

            readerMock.Setup(reader => reader.GetOrdinal("Ordine")).Returns(1);
            readerMock.Setup(reader => reader.GetValue(1)).Returns(RandomNumberGenerator.GetInt32(0, 100).ToString());

            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(m => m.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);

            var connectionMock = new Mock<IDbConnection>();
            connectionMock.Setup(m => m.CreateCommand()).Returns(commandMock.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(x => x.CreateConnection(It.IsAny<string>())).Returns(connectionMock.Object);

            var data = new SqlLevelsRepository(factory.Object);

            return data;
        }

    }
}
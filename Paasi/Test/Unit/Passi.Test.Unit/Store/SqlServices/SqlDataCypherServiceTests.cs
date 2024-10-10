using AutoFixture.Xunit2;
using Moq;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Store.Sql;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Passi.Test.Unit.Core.SqlServices
{
    public class SqlDataCypherServiceTests
    {
        [Theory]
        [InlineAutoData(0)]
        [InlineAutoData(1)]
        [InlineAutoData(2)]
        [InlineAutoData(3)]
        public void Crypt_Decrypt_Ok(int cryptoType, string data)
        {
            var aService = ServiceUnderTest();
            var cypheredData = aService.Crypt(data, (Crypto)cryptoType);
            var decypheredData = aService.Decrypt(cypheredData, (Crypto)cryptoType);

            Assert.Equal(data, decypheredData);
        }

        [Theory]
        [InlineAutoData(1, "")]
        [InlineAutoData(2, null)]
        public void Crypt_DecryptNoKey_Ok(int cryptoType, string? returnValue, string data)
        {
            IDataCypherService aService = ServiceUnderTest(returnValue);
            string cypheredData = aService.Crypt(data, (Crypto)cryptoType);
            string decypheredData = aService.Decrypt(cypheredData, (Crypto)cryptoType);

            Assert.Equal(data, decypheredData);
        }

        private static IDataCypherService ServiceUnderTest(string? returnValue = "NBzwqiW9rpg9oniOlbsdGkoGP90QRul5YWOaR6ce/WUUTwW9RVZV3JzMQrvhPDUx")
        {
            var readerMock = new Mock<DbDataReader>();

            readerMock.SetupSequence(_ => _.Read())
                .Returns(true)
                .Returns(false);

            var dataParameterCollection = new Mock<DbParameterCollection>();

            var parameterMock = new Mock<DbParameter>();
            parameterMock.Setup(m => m.Value).Returns(returnValue);

            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(m => m.CreateParameter()).Returns(parameterMock.Object);
            commandMock.Setup(m => m.Parameters).Returns(dataParameterCollection.Object);
            commandMock.Setup(m => m.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);

            var connectionMock = new Mock<IDbConnection>();
            connectionMock.Setup(m => m.CreateCommand()).Returns(commandMock.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(x => x.CreateConnection(It.IsAny<string>())).Returns(connectionMock.Object);

            var value = Encoding.UTF8.GetBytes("83__.2jfoEId$$..fjksdg23.458DF1A");
            var bis = new ByteInstanceStore();
            bis.Add(Enum.GetName(Crypto.KCA) ?? "KCA", value);
            bis.Add(Enum.GetName(Crypto.OUT) ?? "OUT", value);
            bis.Add(Enum.GetName(Crypto.KTOK) ?? "KTOK", value);
            bis.Add(Enum.GetName(Crypto.KSTA) ?? "KSTA", value);

            var service = new SqlDataCypherService(factory.Object, bis);
            return service;
        }

        [Theory]
        [InlineAutoData]
        public void SecureUnsecure_Ok(Dictionary<string, string> dictionary)
        {
            NameValueCollection collection = new();
            foreach (KeyValuePair<string, string> kvp in dictionary)
            {
                collection.Add(kvp.Key.ToString(), kvp.Value.ToString());
            }
            IDataCypherService aService = ServiceUnderTest();
            string cypheredData = aService.Secure(collection);
            NameValueCollection decypheredData = aService.Unsecure(cypheredData);

            Assert.NotNull(decypheredData);
            Assert.NotEmpty(decypheredData);
            Assert.Equal(collection.Count, decypheredData.Count);
            Assert.Equal(collection, decypheredData);
        }

    }
}

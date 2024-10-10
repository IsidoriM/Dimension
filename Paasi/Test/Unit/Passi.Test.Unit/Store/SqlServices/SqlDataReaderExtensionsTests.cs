using AutoFixture.Xunit2;
using Moq;
using Passi.Test.Unit.Fixtures;
using System.Data;
using System.Data.SqlClient;

namespace Passi.Test.Unit.Core.SqlServices
{
    public class SqlDataReaderExtensionsTests : IClassFixture<PassiFixture>
    {
        public SqlDataReaderExtensionsTests(PassiFixture _)
        {
        }

        [Theory]
        [InlineAutoData]
        [InlineAutoData("@key", null)]
        public void AddParameter_Ok(string key, ushort? size, string value, DbType type)
        {
            Mock<IDbDataParameter> mockParameter = new(MockBehavior.Strict);
            mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>());
            mockParameter.SetupSet(p => p.Value = It.IsAny<object>());
            mockParameter.SetupSet(p => p.DbType = It.IsAny<DbType>());
            mockParameter.SetupSet(p => p.Size = It.IsAny<int>());
            Mock<IDataParameterCollection> mockParameterCollection = new(MockBehavior.Strict);
            mockParameterCollection.Setup(c => c.Add(It.IsAny<object?>())).Returns(1);
            Mock<IDbCommand> mockCommand = new(MockBehavior.Strict);
            mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);
            mockCommand.SetupGet(c => c.Parameters).Returns(mockParameterCollection.Object);
            IDbCommand result = SqlDataReaderExtensions.AddParameter(mockCommand.Object, key, value, type, size);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData("", "")]
        [InlineAutoData(null, "")]
        [InlineAutoData("ciao", "ciao")]
        public void GetStringValueDataRow_Ok(string? value, string expectedResult, string key)
        {
            DataTable table = new("TestTable");
            table.Columns.Add(key, typeof(string));
            DataRow row = table.NewRow();
            row[key] = value;
            string result = SqlDataReaderExtensions.GetStringValue(row, key);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineAutoData("", "")]
        [InlineAutoData("ciao", "ciao")]
        public void GetStringValueDataReader_Ok(string value, string expectedResult, string key)
        {
            Mock<IDataReader> mockDataReader = new(MockBehavior.Strict);
            mockDataReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns(1);
            mockDataReader.Setup(r => r.GetValue(It.IsAny<int>())).Returns(value);

            string result = SqlDataReaderExtensions.GetStringValue(mockDataReader.Object, key);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineAutoData(0, true)]
        [InlineAutoData(1000, false, 1000)]
        public void GetDateValue_Ok(int expectedResultSecondsFromMinValue, bool isDbNull, int valueSecondsFromMinValue, string key)
        {
            Mock<IDataReader> mockDataReader = new(MockBehavior.Strict);
            mockDataReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns(1);
            mockDataReader.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(isDbNull);
            mockDataReader.Setup(r => r.GetDateTime(It.IsAny<int>())).Returns(DateTime.MinValue.AddSeconds(valueSecondsFromMinValue));

            DateTime result = SqlDataReaderExtensions.GetDateValue(mockDataReader.Object, key);
            Assert.Equal(DateTime.MinValue.AddSeconds(expectedResultSecondsFromMinValue), result);
        }

        [Theory]
        [InlineAutoData(0, "")]
        [InlineAutoData(100, "0001-01-01 00:01:40")]
        [InlineAutoData(0, "ciao")]
        public void GetDateValueWhenString_Ok(int expectedResultSecondsFromMinValue, string stringValue, string key)
        {
            Mock<IDataReader> mockDataReader = new(MockBehavior.Strict);
            mockDataReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns(1);
            mockDataReader.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(false);
            mockDataReader.Setup(r => r.GetDateTime(It.IsAny<int>())).Throws(new FormatException());
            mockDataReader.Setup(r => r.GetValue(It.IsAny<int>())).Returns(stringValue);

            DateTime result = SqlDataReaderExtensions.GetDateValue(mockDataReader.Object, key);
            Assert.Equal(DateTime.MinValue.AddSeconds(expectedResultSecondsFromMinValue), result);
        }

        [Theory]
        [InlineAutoData("", 5, 5)]
        [InlineAutoData("1", 1)]
        public void GetIntValueDataRow_Ok(string value, int expectedResult, int defaultValue, string key)
        {
            DataTable table = new("TestTable");
            table.Columns.Add(key, typeof(string));
            DataRow row = table.NewRow();
            row[key] = value;
            int result = SqlDataReaderExtensions.GetIntValue(row, key, defaultValue);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineAutoData("", 5, 5)]
        [InlineAutoData("1", 1)]
        public void GetIntValueDataReader_Ok(string value, int expectedResult, int defaultValue, string key)
        {
            Mock<IDataReader> mockDataReader = new(MockBehavior.Strict);
            mockDataReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns(1);
            mockDataReader.Setup(r => r.GetValue(It.IsAny<int>())).Returns(value);

            int result = SqlDataReaderExtensions.GetIntValue(mockDataReader.Object, key, defaultValue);
            Assert.Equal(expectedResult, result);
        }

    }
}
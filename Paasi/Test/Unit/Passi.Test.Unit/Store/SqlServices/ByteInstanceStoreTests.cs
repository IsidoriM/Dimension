using Moq;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Store.Sql;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;

namespace Passi.Test.Unit.Core.SqlServices
{
    public class ByteInstanceStoreTests
    {
        [Fact]
        public void Get_whenNotFound_thenReturnsEmpty()
        {
            //Arrange
            var repo = new ByteInstanceStore();

            //Act
            var key = "test";
            var result = repo.Get(key);


            //Assert
            Assert.Empty(result);

        }

        [Fact]
        public void Get_whenFound_thenReturns()
        {
            //Arrange
            var repo = new ByteInstanceStore();

            //Act
            var key = "test";
            repo.Add(key, Encoding.UTF8.GetBytes(key));
            repo.Add(key, Encoding.UTF8.GetBytes(key));
            var result = repo.Get(key);

            //Assert
            Assert.True(result.Length > 0);

        }


    }
}
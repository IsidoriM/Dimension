using AutoFixture.Xunit2;
using Moq;
using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
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
    public class SqlUserRepositoryTests
    {
        [Fact]
        public async Task UserAsync_Find_Ok()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var name = Guid.NewGuid().ToString();

            var readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "nome", name  },
                { "codicefiscale", id },
            });

            var repo = ServiceUnderTest(readerMock.Object);

            //Act
            var result = await repo.UserAsync(id, CommonAuthenticationTypes.CNS, string.Empty);

            //Assert
            Assert.True(result.UserId == id);
            Assert.True(result.FiscalCode == id);
        }

        [Fact]
        public async Task IsPatronageAvailableAsync_IsPatronage_True()
        {
            var fiscalCode = Guid.NewGuid().ToString();
            var ente = Guid.NewGuid().ToString();

            var readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "CodiceEnte", ente  },
                { "CodiceFiscale", fiscalCode },
            });

            var repo = ServiceUnderTest(readerMock.Object, null, "1");

            //Act
            var result = await repo.IsDelegationAvailableAsync(fiscalCode, ente);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsPatronageAvailableAsync_IsPatronage_False()
        {
            var fiscalCode = Guid.NewGuid().ToString();
            var ente = Guid.NewGuid().ToString();

            var readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "CodiceEnte", ente  },
                { "CodiceFiscale", fiscalCode },
            });

            var repo = ServiceUnderTest(readerMock.Object, null, "0");

            //Act
            var result = await repo.IsDelegationAvailableAsync(fiscalCode, ente);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasDelegationAsync_HasDelegation_True()
        {
            var id = Guid.NewGuid().ToString();
            var delegatedId = Guid.NewGuid().ToString();

            var readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "utente", id  }
            });

            var dt = new DataTable();
            dt.Columns.Add("CodiceFiscaleDelegato", typeof(string));
            var dr = dt.NewRow();
            dr.SetField("CodiceFiscaleDelegato", delegatedId);
            dt.Rows.Add(dr);
            var ds = new DataSet();
            ds.Tables.Add(dt);

            var repo = ServiceUnderTest(readerMock.Object, ds);

            //Act
            var result = await repo.HasDelegationAsync(id, delegatedId);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasDelegationAsync_HasDelegation_False()
        {
            var id = Guid.NewGuid().ToString();
            var delegatedId = Guid.NewGuid().ToString();

            var readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "utente", id  }
            });

            var dt = new DataTable();
            dt.Columns.Add("CodiceFiscaleDelegato", typeof(string));
            var dr = dt.NewRow();
            dr.SetField("CodiceFiscaleDelegato", Guid.NewGuid().ToString());
            dt.Rows.Add(dr);
            var ds = new DataSet();
            ds.Tables.Add(dt);

            var repo = ServiceUnderTest(readerMock.Object, ds);

            //Act
            var result = await repo.HasDelegationAsync(id, delegatedId);

            //Assert
            Assert.False(result);
        }

        [Theory]
        [InlineAutoData("0")]
        [InlineAutoData(null)]
        public async Task HasSessionAsync_HasSession_False(string? parameterValue)
        {
            var fiscalCode = Guid.NewGuid().ToString();
            var sessionId = 1;

            var readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "Utente", fiscalCode  },
                { "SessionId", sessionId },
                { "SessionConfig", 1 },
                { "DataAggiornamento", DateTime.UtcNow.ToString() },
            });

            var repo = ServiceUnderTest(readerMock.Object, null, parameterValue);

            //Act
            var result = await repo.HasSessionAsync(fiscalCode, sessionId.ToString(), 1);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasSessionAsync_HasSession_True()
        {
            var fiscalCode = Guid.NewGuid().ToString();
            var sessionId = 1;

            var readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "Utente", fiscalCode  },
                { "SessionId", sessionId },
                { "SessionConfig", 1 },
                { "DataAggiornamento", DateTime.UtcNow.ToString() },
            });

            readerMock.Setup(reader => reader.GetOrdinal("@RetCode")).Returns(4);
            readerMock.Setup(reader => reader.GetValue(4)).Returns("test");

            var repo = ServiceUnderTest(readerMock.Object, null, "1");

            //Act
            var result = await repo.HasSessionAsync(fiscalCode, sessionId.ToString(), 1);

            //Assert
            Assert.False(result);
        }

        [Theory]
        [InlineAutoData(1, false)]
        [InlineAutoData(null, true)]
        public async Task ProfilesAsync_Retrieve_Any(int? userTypeId, bool retrieveSuspendedProfiles)
        {
            var fiscalCode = Guid.NewGuid().ToString();
            var serviceId = 1;
            var ente = Guid.NewGuid().ToString();

            var readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "utente", fiscalCode  },
                { "idTipoUtente", 1 },
                { "idServizio", 1 },
                { "authenticationType", CommonAuthenticationTypes.OTP },
                { "getProfiliSospesi", 1 },
            });

            var dt = new DataTable();
            dt.Columns.Add("codEnte", typeof(string));
            dt.Columns.Add("DescrizioneEnte", typeof(string));
            dt.Columns.Add("idTipoUtente", typeof(string));
            dt.Columns.Add("idUfficio", typeof(string));

            for (int i = 0; i < 3; i++)
            {
                var dr = dt.NewRow();
                dr.SetField("codEnte", ente);
                dr.SetField("DescrizioneEnte", ente);
                dr.SetField("idTipoUtente", "1");
                dr.SetField("idUfficio", RandomNumberGenerator.GetInt32(0, 10).ToString());
                dt.Rows.Add(dr);
            }

            var ds = new DataSet();
            ds.Tables.Add(dt);

            var repo = ServiceUnderTest(readerMock.Object, ds);

            //Act
            var result = await repo.ProfilesAsync(fiscalCode, serviceId, userTypeId, CommonAuthenticationTypes.OTP, retrieveSuspendedProfiles);

            //Assert
            Assert.True(result.Any());
            Assert.True(result.Count == 3);
            Assert.True(result.First().InstitutionCode == ente);
        }

        [Fact]
        public async Task ServicesAsync_Retrieve_Any()
        {
            var fiscalCode = Guid.NewGuid().ToString();
            var ente = Guid.NewGuid().ToString();

            var readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "UserName", fiscalCode  },
                { "CodEnte", ente },
            });

            var dt = new DataTable();
            dt.Columns.Add("RichiedeConvenzione", typeof(string));
            dt.Columns.Add("idservizio", typeof(string));
            dt.Columns.Add("IdGruppoServizi", typeof(string));
            dt.Columns.Add("codLivelloAutorizzativo", typeof(string));

            for (int i = 0; i < 3; i++)
            {
                var dr = dt.NewRow();
                dr.SetField("RichiedeConvenzione", "1");
                dr.SetField("idservizio", RandomNumberGenerator.GetInt32(1, 9));
                dr.SetField("IdGruppoServizi", RandomNumberGenerator.GetInt32(0, 9));
                dr.SetField("codLivelloAutorizzativo", CommonAuthenticationTypes.OTP);
                dt.Rows.Add(dr);
            }

            var ds = new DataSet();
            ds.Tables.Add(dt);

            var repo = ServiceUnderTest(readerMock.Object, ds);

            //Act
            var result = await repo.ServicesAsync(fiscalCode, ente);

            //Assert
            Assert.True(result.Any());
            Assert.True(result.Count == 3);
            Assert.True(result.First().Id > 0);
        }

        [Fact]
        public async Task ServicesByLevelAsync_Retrieve_Any()
        {
            var fiscalCode = Guid.NewGuid().ToString();
            var ente = Guid.NewGuid().ToString();

            var readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "UserName", fiscalCode  },
                { "CodEnte", ente },
                { "authenticationType", CommonAuthenticationTypes.OTP },
                { "getProfiliSospesi", 1 },

            });

            var dt = new DataTable();

            dt.Columns.Add("idservizio", typeof(string));
            dt.Columns.Add("codLivelloAutorizzativo", typeof(string));

            for (int i = 0; i < 3; i++)
            {
                var dr = dt.NewRow();
                dr.SetField("idservizio", 4);
                dr.SetField("codLivelloAutorizzativo", CommonAuthenticationTypes.OTP);
                dt.Rows.Add(dr);
            }

            var ds = new DataSet();
            ds.Tables.Add(dt);

            var repo = ServiceUnderTest(readerMock.Object, ds);

            //Act
            var result = await repo.AuthorizedServicesAsync(fiscalCode,
                ente,
                CommonAuthenticationTypes.Undefined,
                true);

            //Assert
            Assert.True(result.Any());
            Assert.True(result.Count == 3);
            Assert.True(result.First().Id > 0);
        }

        [Theory]
        [InlineAutoData("", 3, 0)]
        [InlineAutoData("R", 0, 3)]
        public async Task ConventionsAsync_Retrieve_Any(string filterType, int filterCount, int roleCount)
        {
            var fiscalCode = Guid.NewGuid().ToString();
            var ente = Guid.NewGuid().ToString();
            var conventionedApps = "1,2,3,4,5,6,7,8,9";

            var readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "utente", fiscalCode  },
                { "codiceEnte", ente },
                { "DataScadenza", DateTime.UtcNow.AddDays(1) },
                { "IdServizio", RandomNumberGenerator.GetInt32(9) },
                { "Valore", RandomNumberGenerator.GetInt32(9) },
                { "Ambito", RandomNumberGenerator.GetInt32(9) },
                { "listaGruppoServiziInConvenzione", conventionedApps },
                { "TipoFiltro", filterType },
            }, 2);

            var dt = new DataTable();
            dt.Columns.Add("utente", typeof(string));
            dt.Columns.Add("codiceEnte", typeof(string));
            dt.Columns.Add("listaGruppoServiziInConvenzione", typeof(string));
            dt.Columns.Add("RichiedeConvenzione", typeof(string));
            dt.Columns.Add("idservizio", typeof(string));
            dt.Columns.Add("IdGruppoServizi", typeof(string));
            dt.Columns.Add("codLivelloAutorizzativo", typeof(string));
            for (int i = 0; i < 3; i++)
            {
                var dr = dt.NewRow();
                dr.SetField("utente", ente);
                dr.SetField("codiceEnte", ente);
                dr.SetField("listaGruppoServiziInConvenzione", "1,2,3");
                dr.SetField("RichiedeConvenzione", "1");
                dr.SetField("idservizio", RandomNumberGenerator.GetInt32(9).ToString());
                dr.SetField("IdGruppoServizi", RandomNumberGenerator.GetInt32(9).ToString());
                dr.SetField("codLivelloAutorizzativo", CommonAuthenticationTypes.OTP);
                dt.Rows.Add(dr);
            }

            var ds = new DataSet();
            ds.Tables.Add(dt);

            var repo = ServiceUnderTest(readerMock.Object, ds);

            //Act
            var result = await repo.ConventionsAsync(fiscalCode, ente);

            //Assert
            Assert.NotEmpty(result);
            Assert.True(result.First().IsAvailable);
            Assert.Equal(filterCount, result.First().Filters.Count);
            Assert.Equal(roleCount, result.First().Roles.Count);
        }

        [Theory]
        [InlineAutoData(null)]
        [InlineAutoData("2")]
        public async Task IsGranted_True(string? outParameterValue)
        {
            var fiscalCode = Guid.NewGuid().ToString();
            var serviceId = 1;
            var codEnte = "003";
            var authType = CommonAuthenticationTypes.LOW;

            var readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "UserName", fiscalCode  },
                { "idServizio", serviceId },
                { "codEnte", codEnte },
                { "livelloAutorizzativo", authType },
            });

            readerMock.Setup(reader => reader.GetOrdinal("@RetCode")).Returns(4);
            readerMock.Setup(reader => reader.GetValue(4)).Returns("test");

            var repo = ServiceUnderTest(readerMock.Object, null, outParameterValue);

            //Act
            var result = await repo.IsGrantedAsync(fiscalCode, serviceId, codEnte, authType);

            //Assert
            Assert.False(result);
        }


        [Theory]
        [InlineAutoData]
        [InlineAutoData("abc:cba", "f")]
        [InlineAutoData("id1", "M")]
        public async Task User_Ok(string id, string gender, string name)
        {
            Mock<DbDataReader> readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "nome", name  },
                { "codicefiscale", id },
                { "sesso", gender  },
            });
            IUserRepository repo = ServiceUnderTest(readerMock.Object);
            string expected = id.Split(':').First();

            UserInfo result = await repo.UserAsync(id, CommonAuthenticationTypes.CNS, string.Empty);

            Assert.Equal(expected, result.UserId);
            Assert.Equal(expected, result.FiscalCode);
        }

        [Theory]
        [InlineAutoData("", "fiscalcode")]
        [InlineAutoData(":", "fiscalcode")]
        [InlineAutoData("id1", "")]
        public async Task User_NotFound(string id, string fiscalCode, string name)
        {
            Mock<DbDataReader> readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "nome", name  },
                { "codicefiscale", fiscalCode }
            });
            IUserRepository repo = ServiceUnderTest(readerMock.Object);

            await Assert.ThrowsAsync<NotFoundException>(() => repo.UserAsync(id, CommonAuthenticationTypes.CNS, string.Empty));
        }

        [Theory]
        [InlineAutoData("1", true)]
        [InlineAutoData("0", false)]
        [InlineAutoData(null, false)]
        public async Task IsDelegationAvailable_Ok(string resultString, bool expectedResult, string fiscalCode, string institutionCode)
        {
            Mock<DbDataReader> readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "InstitutionCode", institutionCode  },
                { "CodiceFiscale", fiscalCode },
            });
            IUserRepository repo = ServiceUnderTest(readerMock.Object, null, resultString);

            // Act
            bool result = await repo.IsDelegationAvailableAsync(fiscalCode, institutionCode);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineAutoData("", true)]
        [InlineAutoData("1", false)]
        [InlineAutoData("", false, "")]
        public async Task HasDelegation_Ok(string difference, bool expectedResult, string delegatedUserId, string userId)
        {
            Mock<DbDataReader> readerMock = DbDataReader(new Dictionary<string, object>()
            {
                { "utente", userId }
            });

            DataTable dt = new();
            dt.Columns.Add("CodiceFiscaleDelegato", typeof(string));
            DataRow dr = dt.NewRow();
            dr.SetField("CodiceFiscaleDelegato", delegatedUserId);
            dt.Rows.Add(dr);
            DataSet ds = new();
            ds.Tables.Add(dt);

            IUserRepository repo = ServiceUnderTest(readerMock.Object, ds);

            // Act
            bool result = await repo.HasDelegationAsync(userId, $"{delegatedUserId}{difference}");

            // Assert
            Assert.Equal(expectedResult, result);
        }


        #region private
        private static IUserRepository ServiceUnderTest(DbDataReader dataReader, DataSet? adapter = null, string? parameterValue = null)
        {
            var dataParameterCollection = new Mock<DbParameterCollection>();

            var parameterMock = new Mock<DbParameter>();
            if (!string.IsNullOrWhiteSpace(parameterValue))
                parameterMock.Setup(m => m.Value).Returns(parameterValue);

            Mock<IDbCommand> commandMock = new();
            commandMock.Setup(m => m.CreateParameter()).Returns(parameterMock.Object);
            commandMock.Setup(m => m.Parameters).Returns(dataParameterCollection.Object);
            commandMock.Setup(m => m.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(dataReader);

            var connectionMock = new Mock<IDbConnection>();
            connectionMock.Setup(m => m.CreateCommand()).Returns(commandMock.Object);


            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(x => x.CreateConnection(It.IsAny<string>())).Returns(connectionMock.Object);

            if (adapter != null)
            {
                factory.Setup(x => x.CreateDataset(It.IsAny<IDbCommand>())).Returns(adapter!);
            }

            var adapterMock = new Mock<DbDataAdapter>();
            adapterMock.Setup(x => x.Fill(It.IsAny<DataSet>())).Returns(1);
            factory.Setup(x => x.CreateDataAdapter(It.IsAny<IDbCommand>())).Returns(adapterMock.Object);

            var data = new SqlUserRepository(factory.Object);

            return data;
        }

        private static Mock<DbDataReader> DbDataReader(Dictionary<string, object> data, int cycles = 0)
        {
            var readerMock = new Mock<DbDataReader>();

            var cyc = readerMock.SetupSequence(_ => _.Read())
                .Returns(true);

            for (int i = 0; i < cycles; i++)
            {
                cyc = cyc.Returns(true);
            }

            cyc.Returns(false);

            int count = 0;
            foreach (var element in data)
            {
                readerMock.Setup(reader => reader.GetOrdinal(element.Key)).Returns(count);
                readerMock.Setup(reader => reader.GetValue(count)).Returns(element.Value);
                readerMock.Setup(reader => reader.GetString(count)).Returns(element.Value.ToString() ?? string.Empty);
                count++;
            }

            return readerMock;
        }

        #endregion
    }
}
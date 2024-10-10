using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Store.Sql;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Net;
using AutoFixture;

namespace Passi.Test.Unit.Fixtures
{
    public class PassiFixture : IDisposable
    {
        public static IHttpContextAccessor NullContextAccessor()
        {
            Mock<IServiceProvider> serviceProvider = new();
            serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(null);
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns((HttpContext?)null);
            return mockHttpContextAccessor.Object;
        }

        public static IHttpContextAccessor AccessorUnderTest(string? flag = null)
        {
            Mock<IServiceProvider> serviceProvider = new();
            serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(null);
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext
            {
                RequestServices = serviceProvider.Object,
                Connection =
                {
                    RemoteIpAddress = new IPAddress(16885952)
                },
            };
            context.Request.Scheme = Schema.Https;
            context.Request.Host = new HostString("passiTest.inps.it");
            context.Request.Path = new PathString("/api/pippo");
            context.Request.QueryString = new QueryString("?uri=urltest");
            context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());
            if (!string.IsNullOrWhiteSpace(flag))
            {
                context.Request.Headers.Add(Keys.RequiredUserTypeId, flag);
            }
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
            return mockHttpContextAccessor.Object;
        }

        internal static IDataCypherService DataCypherUnderTest()
        {
            return new SqlDataCypherService(DbConnectionFactory(), new ByteInstanceStore());
        }

        private static IDbConnectionFactory DbConnectionFactory()
        {
            var readerMock = new Mock<DbDataReader>();

            readerMock.SetupSequence(_ => _.Read())
                .Returns(true)
                .Returns(false);

            readerMock.Setup(reader => reader.GetOrdinal("@Valore")).Returns(0);
            readerMock.Setup(reader => reader.GetValue(0)).Returns("NBzwqiW9rpg9oniOlbsdGkoGP90QRul5YWOaR6ce/WUUTwW9RVZV3JzMQrvhPDUx");

            var dataParameterCollection = new Mock<DbParameterCollection>();

            var parameterMock = new Mock<DbParameter>();
            parameterMock.Setup(m => m.Value).Returns("NBzwqiW9rpg9oniOlbsdGkoGP90QRul5YWOaR6ce/WUUTwW9RVZV3JzMQrvhPDUx");

            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(m => m.CreateParameter()).Returns(parameterMock.Object);
            commandMock.Setup(m => m.Parameters).Returns(dataParameterCollection.Object);
            commandMock.Setup(m => m.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(readerMock.Object);

            var connectionMock = new Mock<IDbConnection>();
            connectionMock.Setup(m => m.CreateCommand()).Returns(commandMock.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(x => x.CreateConnection(It.IsAny<string>())).Returns(connectionMock.Object);
            return factory.Object;
        }

        public Mocks Mocks(int serviceId, int? requiredProfileId = null) => new(serviceId, this, requiredProfileId);

        public static ILogger<T> GetLogger<T>()
        {
            var logger = new Mock<ILogger<T>>();

            logger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()))
                .Callback(new InvocationAction(invocation =>
                {
                    var logLevel = (LogLevel)invocation.Arguments[0]; // The first two will always be whatever is specified in the setup above
                    //var eventId = (EventId)invocation.Arguments[1];  // so I'm not sure you would ever want to actually use them
                    var state = invocation.Arguments[2];
                    var exception = (Exception)invocation.Arguments[3];
                    var formatter = invocation.Arguments[4];

                    var invokeMethod = formatter.GetType().GetMethod("Invoke");
                    var logMessage = invokeMethod?.Invoke(formatter, new[] { state, exception });

                    Trace.WriteLine($"{logLevel} - {logMessage}");
                }));

            return logger.Object;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            // do nothing
        }

        public Fixture Fixture { get; private set; }

        public PassiFixture()
        {
            Fixture = new();
            Fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth: 1));
        }
    }

}
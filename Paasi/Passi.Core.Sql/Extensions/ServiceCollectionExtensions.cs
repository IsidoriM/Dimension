using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Store.Sql;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPassiSqlServices(this IServiceCollection services)
        {
            services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
            services.AddSingleton<ILevelsRepository, SqlLevelsRepository>();
            services.AddScoped<IDataCypherService, SqlDataCypherService>();
            services.AddScoped<IUserRepository, SqlUserRepository>();
            services.AddScoped<ICLogRepository, SqlCLogRepository>();
            services.AddSingleton<IInstanceStore<byte[]>, ByteInstanceStore>();
            return services;
        }
    }
}

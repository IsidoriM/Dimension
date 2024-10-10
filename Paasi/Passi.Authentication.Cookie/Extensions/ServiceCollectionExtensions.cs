using Microsoft.Extensions.Configuration;
using Passi.Authentication.Cookie.Providers;
using Passi.Authentication.Cookie.Repository;
using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Entities.Info;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddPassiAuthentication(this IServiceCollection services,
            IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddRegistryKeyConfiguration();

            /// Servizi comuni
            services.AddPassiAllServices(configurationBuilder.Build());

            /// Servizi SQL
            services.AddPassiSqlServices();

            /// Servizi cookie
            services.AddScoped<IInfoRepository<ConventionInfo>, ConventionInfoRepository>();
            services.AddScoped<IInfoRepository<SessionInfo>, SessionInfoRepository>();
            services.AddScoped<IInfoRepository<ProfileInfo>, ProfileInfoRepository>();
            services.AddScoped<IInfoRepository<UserInfo>, UserInfoRepository>();
            services.AddScoped<IInfoRepository<ContactCenterInfo>, ContactCenterInfoRepository>();
            services.AddScoped<IInfoRepository<SessionToken>, SessionTokenRepository>();
            services.AddScoped<IHostingAppManager, CookieHostingAppManager>();

            return services;
        }

        private static IConfigurationBuilder AddRegistryKeyConfiguration(
               this IConfigurationBuilder builder)
        {
            return builder.Add(new RegistryKeyConfigurationSource());
        }
    }
}

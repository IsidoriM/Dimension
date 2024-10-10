using Microsoft.Extensions.Configuration;
using Passi.Authentication.Fake.Repository;
using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Store.Fake.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Metodi di estensione per l'inizializzazione di PASSI.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Metodo di inizializzazione dei servizi di PASSI.
        /// Richiama questo metodo per inizializzare le istanze di IPassiService, IPassiSecureService, IPassiUserContactsService.
        /// Inoltre, permette di iniettare le policy di autenticazione.
        /// </summary>
        /// <param name="services" cref="IServiceCollection">Service collection.</param>
        /// <param name="configurationBuilder" cref="IConfigurationBuilder">Configuration builder.</param>
        /// <returns cref="IServiceCollection">Service collection.</returns>
        public static IServiceCollection AddPassiAuthentication(this IServiceCollection services, IConfigurationBuilder configurationBuilder)
        {
            var configuration = configurationBuilder.Build();

            /// Aggiungi i servizi base
            services.AddPassiAllServices(configuration);

            services.AddPassiFakeServices(configuration);

            /// Aggiungi i servizi che leggono da appsettings
            services.AddSingleton<IInfoRepository<SessionInfo>, SessionInfoRepository>();
            services.AddSingleton<IInfoRepository<UserInfo>, UserInfoRepository>();
            services.AddSingleton<IInfoRepository<ProfileInfo>, ProfileInfoRepository>();
            services.AddSingleton<IInfoRepository<ConventionInfo>, ConventionInfoRepository>();
            services.AddSingleton<IInfoRepository<ContactCenterInfo>, ContactCenterInfoRepository>();
            services.AddScoped<IInfoRepository<SessionToken>, SessionTokenRepository>();
            services.AddScoped<IHostingAppManager, HostingAppManager>();

            var options = configuration.GetSection(UserOptions.SectionName);
            services.Configure<UserOptions>(options);
            return services;
        }
    }
}


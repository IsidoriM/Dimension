using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Passi.Core.Application.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Entities;
using Passi.Core.Store.Fake;
using Passi.Core.Store.Fake.Options;
using Passi.Core.Store.Fake.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPassiFakeServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ILevelsRepository, LevelsRepository>();
            services.AddScoped<IDataCypherService, DataCypherService>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.Configure<UserOptions>(configuration.GetSection(UserOptions.SectionName));

            configuration["LinkChangeContacts"] = UriExtensions.Default.ToString();
            configuration["LinkChangePin"] = UriExtensions.Default.ToString();
            configuration["LinkErrorPage"] = UriExtensions.Default.ToString();
            configuration["LinkLogout"] = UriExtensions.Default.ToString();
            configuration["LinkSwitchProfile"] = UriExtensions.Default.ToString();
            configuration["LinkPassiWeb"] = UriExtensions.Default.ToString();
            configuration["LinkPassiWebC"] = UriExtensions.Default.ToString();
            configuration["LinkPassiWebI"] = UriExtensions.Default.ToString();
            configuration["LinkPassiWebO"] = UriExtensions.Default.ToString();

            services.Configure<ErrorOptions>(configuration.GetSection("Error"));

            services.Decorate<IPassiAuthenticationService, FakeAuthenticationService>();
            services.AddScoped<ICLogRepository, FakeCLogRepository>();


            return services;
        }
    }
}

using Microsoft.Extensions.Configuration;
using Passi.Core.Application.Options;
using Passi.Core.Application.Services;
using Passi.Core.Services;
using Passi.Core.Services.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddPassiAllServices(this IServiceCollection services, IConfiguration configuration)
        {
            /// Aggiunge lo schema di autenticazione
            services
                .AddPassiScheme()
                .AddPassiServices(configuration);

            /// Aggiungere la decorazione all'authentication service
            services.Decorate<IPassiAuthenticationService, ApiAuthenticationService>();
            return services;
        }

        private static IServiceCollection AddPassiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<UrlOptions>(a => ConfigureExtensions.ConfigureUrlOptions(a, configuration));

            services.Configure<ConfigurationOptions>(a => ConfigureExtensions.ConfigureConfigurationOptions(a, configuration));

            services.AddHttpContextAccessor();

            services.AddScoped(typeof(IPassiAuthenticationService), Type.GetType("Passi.Core.Services.AuthenticationService")!);
            services.AddScoped(typeof(IPassiUserContactsService), Type.GetType("Passi.Core.Services.PassiUserContactsService")!);
            services.AddScoped(typeof(IPassiService), Type.GetType("Passi.Core.Services.PassiService")!);
            services.AddScoped(typeof(IPassiSecureService), Type.GetType("Passi.Core.Services.PassiSecureService")!);
            services.AddScoped(typeof(IPassiConventionService), Type.GetType("Passi.Core.Services.PassiConventionService")!);

            services.AddScoped(typeof(ICLogService), Type.GetType("Passi.Core.Services.CLogService")!);

            return services;
        }
    }
}

namespace Passi.Core.Services.Extensions
{
    internal static class ConfigureExtensions
    {
        internal static void ConfigureUrlOptions(UrlOptions options, IConfiguration configuration)
        {
            var changeContacts = configuration["LinkChangeContacts"];
            var changePin = configuration["LinkChangePin"];
            var errorPage = configuration["LinkErrorPage"];
            var logout = configuration["LinkLogout"];
            var switchProfile = configuration["LinkSwitchProfile"];
            var passiWeb = configuration["LinkPassiWeb"];
            var passiWebC = configuration["LinkPassiWebC"];
            var passiWebI = configuration["LinkPassiWebI"];
            var passiWebO = configuration["LinkPassiWebO"];
            options.ChangeContacts = !string.IsNullOrWhiteSpace(changeContacts) ? new Uri(changeContacts!) : UriExtensions.Default;
            options.ChangePin = !string.IsNullOrWhiteSpace(changePin) ? new Uri(changePin!) : UriExtensions.Default;
            options.ErrorPage = !string.IsNullOrWhiteSpace(errorPage) ? new Uri(errorPage!) : UriExtensions.Default;
            options.Logout = !string.IsNullOrWhiteSpace(logout) ? new Uri(logout!) : UriExtensions.Default;
            options.SwitchProfile = !string.IsNullOrWhiteSpace(switchProfile) ? new Uri(switchProfile!) : UriExtensions.Default;
            options.PassiWeb = !string.IsNullOrWhiteSpace(passiWeb) ? new Uri(passiWeb!) : UriExtensions.Default;
            options.PassiWebCns = !string.IsNullOrWhiteSpace(passiWebC) ? new Uri(passiWebC!) : UriExtensions.Default;
            options.PassiWebI = !string.IsNullOrWhiteSpace(passiWebI) ? new Uri(passiWebI!) : UriExtensions.Default;
            options.PassiWebOtp = !string.IsNullOrWhiteSpace(passiWebO) ? new Uri(passiWebO!) : UriExtensions.Default;
        }

        internal static void ConfigureConfigurationOptions(ConfigurationOptions options, IConfiguration configuration)
        {
            var serviceId = configuration["ServiceId"]; //required
            var redirectUrl = configuration["RedirectUrl"];
            var gestioneSessione = configuration["GestioneSessione"] ?? "1";
            var log = configuration["Log"] ?? "0";
            var allowServiceIdEditingProp = configuration["AllowServiceIdEditing"];
            _ = bool.TryParse(allowServiceIdEditingProp, out bool allowServiceIdEditing);

            options.ServiceId = string.IsNullOrWhiteSpace(serviceId) ? 0 : int.Parse(serviceId);
            options.SessionManagementFlag = string.IsNullOrWhiteSpace(gestioneSessione) ? string.Empty : gestioneSessione!;
            options.Log = string.IsNullOrWhiteSpace(log) ? string.Empty : log;
            options.AllowServiceIdEditing = allowServiceIdEditing;
            options.RedirectUrl = redirectUrl;
        }
    }
}
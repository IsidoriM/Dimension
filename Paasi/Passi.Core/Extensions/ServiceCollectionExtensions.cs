using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Passi.Core.Extensions;
using Passi.Core.Handlers;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Metodi di estensione per IServiceCollection. <br/>
    /// Da utilizzare nel Program per inizializzare PASSI.
    /// </summary>
    static class ServiceCollectionExtensions
    {
        [SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "Da capire se riportare tutti i services")]
        public static IServiceCollection AddPassiScheme(this IServiceCollection services)
        {
            services.AddAuthentication(opt => ServiceCollectionExtensionsOptions.AddDefaultScheme(opt))
                .AddScheme<PassiAuthenticationSchemeOption, PassiAuthenticationHandler>(ServiceCollectionExtensionsOptions.SCHEME, null);

            services.AddAuthorizationCore(options => ServiceCollectionExtensionsOptions.AddAuthorizationCoreOptions(options));

            services.AddAuthorizationPolicyEvaluator();

            // controllo disabilitato in quanto per ora non sono richiesti Ruoli
            //services.AddScoped<IAuthorizationHandler, PassiAuthorizationHandler>();

            services.Configure<CookiePolicyOptions>(options => ServiceCollectionExtensionsOptions.ConfigureCookiePolicyOptions(options));

            return services;
        }
    }
}

namespace Passi.Core.Extensions
{
    internal class ServiceCollectionExtensionsOptions
    {
        internal const string SCHEME = "Passi";
        internal const string POLICY = "PassiPolicy";

        internal static void AddDefaultScheme(AuthenticationOptions opt)
        {
            opt.DefaultScheme = SCHEME;
        }

        internal static void AddAuthorizationCoreOptions(AuthorizationOptions options)
        {
            AuthorizationPolicyBuilder authorizationBuilder = new(POLICY)
            {
                AuthenticationSchemes = new[] { SCHEME }
            };
            authorizationBuilder = authorizationBuilder.RequireAuthenticatedUser();

            options.DefaultPolicy = authorizationBuilder.Build();
        }

        internal static void ConfigureCookiePolicyOptions(CookiePolicyOptions options)
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        }
    }
}

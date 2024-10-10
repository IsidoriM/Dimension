using Microsoft.AspNetCore.Authentication;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Metodi di estensione per AppBuilder. <br/>
    /// Da utilizzare nel Program per inizializzare PASSI.
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Questo metodo di estensione deve essere richiamato nel Progeam.cs 
        /// per inizializzare l'uso del middleware di autenticazione PASSI.
        /// Va utilizzato sia per le applicazioni API che per le applicazioni WEB.
        /// </summary>
        /// <param name="app" cref="IApplicationBuilder">Application builder.</param>
        /// <returns cref="IApplicationBuilder">Application builder.</returns>
        public static IApplicationBuilder UsePassiAuthentication(this IApplicationBuilder app)
        {
            app.UseMiddleware<AuthenticationMiddleware>();

            return app;
        }
    }
}

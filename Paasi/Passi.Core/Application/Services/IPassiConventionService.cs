using Passi.Core.Domain.Entities;

namespace Passi.Core.Application.Services
{
    /// <summary>
    /// Questo servizio permette di verificare se sono presenti dei ruoli e filtri sul servizio utilizzato per l'autenticazione. <br/>
    /// Viceversa, il servizio di autenticazione deve richiedere una convenzione affinché abbia dei ruoli e dei filtri associati.
    /// </summary>
    public interface IPassiConventionService
    {
        /// <summary>
        /// Restituisce la lista dei ruoli per il servizio utilizzato per l'autenticazione.
        /// </summary>
        /// <returns cref="Role">Lista dei ruoli per il servizio utilizzato per l'autenticazione.</returns>
        public Task<ICollection<Role>> ConventionRolesAsync();

        /// <summary>
        /// Restituisce se l’utente ha il ruolo indicato per il servizio utilizzato per l’autenticazione. 
        /// </summary>
        /// <param name="role">Ruolo richiesto</param>
        /// <returns>True se il ruolo è presente, altrimenti false.</returns>
        public Task<bool> ConventionHasRoleAsync(string role);

        /// <summary>
        /// Restituisce la lista dei filtri per il servizio utilizzato per l'autenticazione.
        /// </summary>
        /// <returns cref="Filter">Lista dei filtri per il servizio utilizzato per l'autenticazione.</returns>
        public Task<ICollection<Filter>> ConventionFiltersAsync();

        /// <summary>
        /// Restituisce la lista dei filtri per il servizio utilizzato per l'autenticazione e del tipo specificato.
        /// </summary>
        /// <param name="type">Tipo di filtro richiesto.</param>
        /// <returns cref="Filter">Lista dei filtri per il servizio utilizzato per l'autenticazione.</returns>
        public Task<ICollection<Filter>> ConventionFiltersAsync(string type);
    }
}

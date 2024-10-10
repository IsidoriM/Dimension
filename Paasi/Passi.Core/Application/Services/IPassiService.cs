using Passi.Core.Domain.Entities;

namespace Passi.Core.Application.Services
{
    /// <summary>
    /// Questo servizio espone una suite di funzionalità che permettono di restituire le informazioni sull'utenza loggata unite a delle funzionalità di controllo e verifica. <br/>
    /// Attraverso questo quindi, è possibile ottenere i dati dell'utente, del suo profilo, se ha deleghe ed effettuare eventuali check.
    /// </summary>
    public interface IPassiService
    {
        /// <summary>
        /// Recupera i dati anagrafici dell'utente loggato.
        /// </summary>
        /// <returns cref="User">User</returns>
        public Task<User> MeAsync();

        /// <summary>
        /// Ottiene il profilo corrente dell'utente loggato.
        /// </summary>
        /// <returns cref="Profile">Profilo</returns>
        public Task<Profile> ProfileAsync();

        /// <summary>
        /// Recupera le UserContacts dei contatti dell'utente loggato dall'Archivio Unico dei Contatti. <br/>
        /// Le UserContacts sono presentate generalmente come HTML da embeddare in pagina. <br/>
        /// Gli utenti del Contact Center che utilizzano questa funzionalità potranno vedere le UserContacts dei contatti dell'utente.
        /// </summary>
        /// <returns cref="UserContacts">UserContacts</returns>
        public Task<UserContacts> UserContactsAsync();

        /// <summary>
        /// Recupera le UserContacts dei contatti di un utente dall'Archivio Unico dei Contatti. <br/>
        /// Le UserContacts sono presentate generalmente come HTML da embeddare in pagina. <br/>
        /// E' possibile ottenere i contatti di un utente, ma i dati personali (email, telefono...) saranno offuscati.
        /// </summary>
        /// <param name="fiscalCode">L'identificativo univoco dell'utente (il suo codice fiscale)</param>
        /// <returns cref="UserContacts">UserContacts</returns>
        public Task<UserContacts> UserContactsAsync(string fiscalCode);

        /// <summary>
        /// Verifica se il soggetto passato come parametro, ha delegato il patronato su cui è profilato l’utente autenticato.
        /// </summary>
        /// <param name="delegatedFiscalCode">Il codice fiscale del delegato</param>
        /// <returns>True se il soggetto ha delegato il patronato su cui è profilato l’utente autenticato.</returns>
        public Task<bool> HasPatronageDelegationAsync(string delegatedFiscalCode);

        /// <summary>
        /// Verifica se l'utente loggato è abilitato o meno all'utilizzo del servizio specificato (aggiuntivo rispetto a quello usato per l’autenticazione).<br/>
        /// Questo metodo deve essere utilizzato quando, per esempio, deve essere controllata l’autorizzazione a più di un serviceId. 
        /// </summary>
        /// <param name="serviceId">L'id del servizio richiesto</param>
        /// <returns>True se abilitato.</returns>
        public Task<bool> IsAuthorizedAsync(int serviceId);

        /// <summary>
        /// Ottiene la lista dei servizi disponibili all'utente loggato.
        /// </summary>
        /// <returns>La lista degli id dei servizi disponibili.</returns>
        public Task<ICollection<int>> AuthorizedServicesAsync();

        #region Url

        /// <summary>
        /// La url da richiamare per cambiare il proprio profilo.
        /// </summary>
        /// <returns cref="Uri">Url per cambiare il profilo.</returns>
        public Uri SwitchProfileUrl();

        /// <summary>
        /// La url da richiamare per effettuare il logout.
        /// </summary>
        /// <returns cref="Uri">Url di logout.</returns>
        public Uri LogoutUrl();

        #endregion
    }
}

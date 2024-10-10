using Passi.Core.Domain.Entities;

namespace Passi.Core.Application.Services
{
    /// <summary>
    /// Servizio che consente di recuperare le UserContacts dei contatti di un utente dall'Archivio Unico dei Contatti.
    /// </summary>
    internal interface IPassiUserContactsService
    {
        /// <summary>
        /// Recupera la descrizione dell'utente loggato
        /// </summary>
        /// <returns></returns>
        public Task<UserContacts> UserContactsAsync();

        /// <summary>
        /// Recupera la descrizione di un utente in base al suo codice fiscale
        /// Solo particolari profili possono recuperare i dati, e questi verranno comunque offuscati
        /// </summary>
        /// <param name="fiscalCode"></param>
        /// <returns></returns>
        public Task<UserContacts> UserContactsAsync(string fiscalCode);
    }
}

using Passi.Core.Domain.Const;

namespace Passi.Core.Domain.Entities
{
    /// <summary>
    /// Rappresenta i dati anagrafici dell'utente loggato
    /// </summary>
    public class User
    {
        /// <summary>
        /// id utente (per quasi la totalità dei casi è rappresentato dal codice fiscale dell’utente)
        /// </summary>
        public string UserId { get; internal set; } = string.Empty;
        /// <summary>
        /// Nome
        /// </summary>
        public string Name { get; internal set; } = string.Empty;

        /// <summary>
        /// Cognome
        /// </summary>
        public string Surname { get; internal set; } = string.Empty;

        /// <summary>
        /// Genere
        /// </summary>
        public string Gender { get; internal set; } = string.Empty;
        /// <summary>
        /// Città di nascita
        /// </summary>
        public string BirthPlaceCode { get; internal set; } = string.Empty;

        /// <summary>
        /// Provincia di nascita (in formato Codice Catastale)
        /// </summary>
        public string BirthProvince { get; internal set; } = string.Empty;

        /// <summary>
        /// Data di nascita
        /// </summary>
        public string BirthDate { get; internal set; } = string.Empty;

        /// <summary>
        /// Specifica se l'utente ha più profili associati
        /// </summary>
        public bool MultipleProfile { get; internal set; } = false;

        /// <summary>
        /// Metodologia di autenticazione dell'utente
        /// </summary>
        public string AuthenticationType { get; internal set; } = string.Empty;
    }
}

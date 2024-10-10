using Passi.Core.Domain.Const;

namespace Passi.Core.Domain.Entities.Info
{
    /// <summary>
    /// Rappresenta i dati anagrafici dell'utente loggato
    /// </summary>
    [Serializable]
    class UserInfo
    {
        /// <summary>
        /// id utente (per quasi la totalità dei casi è rappresentato dal codice fiscale dell’utente)
        /// </summary>
        public string UserId { get; internal set; } = string.Empty;

        /// <summary>
        /// Codice fiscale
        /// </summary>
        public string FiscalCode { get; internal set; } = string.Empty;

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
        /// Email personale
        /// </summary>
        public string Email { get; internal set; } = string.Empty;

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
        public DateTime BirthDate { get; internal set; } = DateTime.MinValue;

        /// <summary>
        /// Numero di telefono personale (casa)
        /// </summary>
        public string Phone { get; internal set; } = string.Empty;

        /// <summary>
        /// Numero di telefono personale (cellulare)
        /// </summary>
        public string Mobile { get; internal set; } = string.Empty;

        /// <summary>
        /// Casella PEC personale
        /// </summary>
        public string PEC { get; internal set; } = string.Empty;

        /// <summary>
        /// Codice ufficio
        /// </summary>
        public string OfficeCode { get; internal set; } = string.Empty;

        /// <summary>
        /// Specifica se l'utente ha più profili associati
        /// </summary>
        public bool MultipleProfile { get; internal set; } = false;

        /// <summary>
        /// Verifica se i dati dell'utente sono ricavati da una federazione
        /// </summary>
        public bool IsFederated { get; internal set; } = false;
    }
}

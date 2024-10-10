namespace Passi.Core.Domain.Entities
{
    /// <summary>
    /// Definisce il profilo corrente dell'utente loggato.
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// Id del profilo
        /// </summary>
        public int ProfileTypeId { get; internal set; } = 0;

        /// <summary>
        /// Codice dell'ente relativo al profilo selezionato
        /// </summary>
        public string InstitutionCode { get; internal set; } = string.Empty;

        /// <summary>
        /// Codice fiscale dell'ente relativo al profilo selezionato
        /// </summary>
        public string InstitutionFiscalCode { get; internal set; } = string.Empty;

        /// <summary>
        /// Descrizione dell'ente
        /// </summary>
        public string InstitutionDescription { get; internal set; } = string.Empty;

        /// <summary>
        /// Codice ufficio
        /// </summary>
        public string OfficeCode { get; internal set; } = string.Empty;
    }
}

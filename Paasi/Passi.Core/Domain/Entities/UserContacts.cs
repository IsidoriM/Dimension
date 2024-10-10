using Microsoft.AspNetCore.Http;

namespace Passi.Core.Domain.Entities
{
    /// <summary>
    /// Oggetto contenente le informazioni principali di contatto e le descrizioni da mostrare.
    /// </summary>
    [Serializable]
    public class UserContacts
    {
        public UserContacts() { }

        /// <summary>
        /// Titolo da mostrare per il contatto
        /// </summary>
        public string Title { get; internal set; } = string.Empty;
        /// <summary>
        /// Email label
        /// </summary>
        public string EmailLabel { get; internal set; } = "Indirizzo email:";
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; internal set; } = string.Empty;
        /// <summary>
        /// PEC label
        /// </summary>
        public string PecLabel { get; internal set; } = "Indirizzo PEC:";
        /// <summary>
        /// Indirizzo PEC
        /// </summary>
        public string Pec { get; internal set; } = string.Empty;
        /// <summary>
        /// Mobile label
        /// </summary>
        public string MobileLabel { get; internal set; } = "Cellulare:";
        /// <summary>
        /// Numero di telefono personale (cellulare)
        /// </summary>
        public string Mobile { get; internal set; } = string.Empty;
        /// <summary>
        /// Phone label
        /// </summary>
        public string PhoneLabel { get; internal set; } = "Telefono fisso:";
        /// <summary>
        /// Numero di telefono personale (casa)
        /// </summary>
        public string Phone { get; internal set; } = string.Empty;
        /// <summary>
        /// Informazioni aggiuntive del contatto
        /// </summary>
        public string Info { get; internal set; } = string.Empty;

    }
}

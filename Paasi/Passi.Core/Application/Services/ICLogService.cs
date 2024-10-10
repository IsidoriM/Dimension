
using System.Diagnostics.CodeAnalysis;

namespace Passi.Core.Application.Services
{
    /// <summary>
    /// Servizio che consente l’alimentazione del log di sicurezza (CLOG) con gli eventi di tracciatura definiti per l’applicazione.
    /// </summary>
    public interface ICLogService
    {
        /// <summary>
        /// Effettua il log persistente di un evento per l'utente loggato.
        /// </summary>
        /// <param name="eventId">Id dell’evento oggetto di log. È uno degli id rilasciato in fase di censimento dal gruppo sicurezza.clog.</param>
        /// <param name="executionTime">Tempo di esecuzione in millisecondi dell’operazione. Questo campo va valorizzato se si intende monitorare i tempi di risposta dei servizi.</param>
        /// <param name="parameters">Contiene i dati identificativi dell’operazione effettuata, presentati come insieme di elementi chiave/valore.
        /// <param name="returnCode">Codice di ritorno dell’operazione. Il codice dovrà essere 0 per tutte quelle operazioni che hanno avuto un esito positivo. In caso di errori il campo dovrà avere un valore diverso da 0 con una codifica propria per ogni servizio.<br/> In quest’ultimo caso dovrà essere valorizzato obbligatoriamente anche il campo errorMessage. Per i dettagli far riferimento alle specifiche di integrazione con il sistema CLOG presenti sul portale intranet nell’Area Sicurezza dei Sistemi informativi.</param></param>
        /// <param name="errorMessage">(Opzionale) Fornisce una descrizione dell’eventuale errore riscontrato. Se returnCode=0 non deve essere valorizzato.</param>
        /// <returns>Viene lanciata una eccezione di tipo CLogException se il salvataggio non va a buon fine</returns>
        public Task LogAsync(int eventId,
            int executionTime,
            Dictionary<string, string> parameters,
            int returnCode = 0,
            string? errorMessage = null);
    }
}

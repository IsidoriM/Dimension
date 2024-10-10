using System.Diagnostics.CodeAnalysis;

namespace Passi.Core.Application.Repositories
{
    internal interface ICLogRepository
    {
        /// <summary>
        /// Servizio utilizzato per scrivere i CLog sul DB.
        /// </summary>
        /// <param name="userId">Id dell'utente</param>
        /// <param name="eventId">Id dell'evento</param>
        /// <param name="ip">Ip del Client</param>
        /// <param name="executionTime">Tempo di esecuzine</param>
        /// <param name="returnCode">Codice di ritorno</param>
        /// <param name="tipoUtente">Il codice identificativo del tipo di utente, precedentemente valorizzato come idClasseUtente</param>
        /// <param name="institutionCode">Codice dell'ente</param>
        /// <param name="workOfficeCode">Codice dell'ufficio</param>
        /// <param name="parameters">Parametri aggiuntivi</param>
        /// <param name="errorMessage">Messaggio di errore</param>
        /// <returns>Viene lanciata una eccezione di tipo CLogException se il salvataggio del log va in errore</returns>
        /// <throws>CLogException</throws>
        public Task LogAsync(string userId, 
            int eventId, 
            string ip, 
            int executionTime, 
            int returnCode,
            int tipoUtente,
            [AllowNull] string? institutionCode, 
            [AllowNull] string? workOfficeCode, 
            [AllowNull] string? parameters, 
            [AllowNull] string? errorMessage);
    }
}

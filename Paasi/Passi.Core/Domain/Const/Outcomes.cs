namespace Passi.Core.Domain.Const
{
    enum Outcomes
    {
        One = 1,
        Two = 2,
        Success = 0,
        /// <summary>
        /// Non è stato richiamato il metodo checkAuthentication di PASSI
        /// </summary>
        AUC001 = -1,
        /// <summary>
        /// Non è stato possibile recuperare le informazioni dell'utente
        /// </summary>
        AUC002 = -2,
        /// <summary>
        /// Non è stato possibile recuperare le informazioni dell'utente (cookie Contact Center vuoto)
        /// </summary>
        AUC003 = -3,
        /// <summary>
        /// Non è stato possibile recuperare le informazioni dell'utente (cookie Contact Center non presente)
        /// </summary>
        AUC004 = -4,
        /// <summary>
        /// Non è stato possibile recuperare le informazioni dell'utente (errore nel recupero delle informazioni)
        /// </summary>
        AUC005 = -5,
        /// <summary>
        /// Il metodo getUserContacts è stato invocato senza valorizzare i parametri richiesti
        /// </summary>
        AUC006 = -6,
        /// <summary>
        /// Tentativo di invocare il metodo per l’accesso ai contatti da parte di un intermediario con un profilo utente non intermediario
        /// </summary>
        AUC007 = -7
    }
}

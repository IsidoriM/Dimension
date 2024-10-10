namespace Passi.Core.Application.Services
{
    /// <summary>
    /// Con questo servizio è possibile ottenere il necessario per l'integrazione con eventuali API (qualora presenti)
    /// e permette la cifratura di testi da utilizzare in ambiente INPS per lo scambio dati.
    /// </summary>
    public interface IPassiSecureService
    {
        /// <summary>
        /// Converte in formato querystring una serie di dati
        /// e li restituisce cifrati come testo che potrà poi essere letto dal resto della piattaforma.
        /// <br/>
        /// E' l'equivalente del SecureTable.
        /// </summary>
        /// <param name="data">Il dato da cifrare</param>
        /// <returns>Testo cifrato</returns>
        public Task<string> SecureAsync(IDictionary<string, string> data);

        /// <summary>
        /// Decripta un testo cifrato e lo trasforma in un oggetto formato querystring in chiaro.
        /// <br/>
        /// E' l'equivalente del SecureTable.
        /// </summary>
        /// <param name="cryptedText">Dati da decriptare</param>
        /// <returns>Formato querystring chiave-valore decriptato</returns>
        public Task<IDictionary<string, string>> UnsecureAsync(string cryptedText);

        /// <summary>
        /// Recupera un token di sessione che può essere utilizzato per effettuare delle chiamate verso web Api che utilizzano PASSI.
        /// </summary>
        /// <returns>Il token di sessione richiesto.</returns>
        public Task<string> SessionTokenAsync();

        /// <summary>
        /// Effettua una verifica sulla lunghezza massima e sulla presenza di caratteri pericolosi all’interno della stringa passata come parametro al metodo.<br/>
        /// L’utilizzo è indicato per la verifica, per esempio, dei valori delle textbox passati allo strato di business dell’applicazione.<br/>
        /// È esposto anche un overload del metodo che consente di escludere dal controllo particolari caratteri.<br/>
        /// I caratteri verificati sono i seguenti: | &amp; ; $ % @ ' " \\' \\" &lt; &gt; ( ) + \n \r , \
        /// </summary>
        /// <param name="parameter">Parametro da controllare</param>
        /// <param name="maxLength">Lunghezza massima consentita</param>
        /// <param name="exceptionValues">(Opzionale) Specifica eventuali caratteri da consentire forzatamente</param>
        /// <returns>True se il check è passato correttamente, altrimenti False.</returns>
        public Task<bool> CheckParameterAsync(string parameter, int maxLength, string[]? exceptionValues = null);
    }
}

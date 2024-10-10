using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Web;

namespace TestMenuEnteMvc.Class
{
    public class ProfilazioneIam
    {
        /// <summary>
        ///     Determina il tipo di confronto che è possibile eseguire sui ruoli dell'operatore.
        /// </summary>
        public enum ComparisionType
        {
            /// <summary>
            ///     Il confronto viene fatto su tutti i ruoli.
            /// </summary>
            All,

            /// <summary>
            ///     Il confronto è fatto sul possesso di almeno un ruolo tra quelli indicati.
            /// </summary>
            AtLeastOne,
        }

        public static ProfilazioneIam Instance
        {
            get
            {
                return new ProfilazioneIam();
            }
        }

        /// <summary>Esegue un controllo sui ruoli posseduti dall'operatore per verificare
        ///     se possiede uno o tutti dei ruoli indicati in <c>value</c>.
        ///     Il controllo cambia a seconda di <c>type</c>.</summary>
        /// <param name="values">I ruoli da controllare.</param>
        /// <param name="type">Il tipo di confronto eseguito.
        ///     All = Tutti i ruoli presenti in "values" devono essere posseduti dall'operatore
        ///     AtLeastOne = Almeno uno dei ruoli presenti in "values" deve essere posseduto dall'operatore.
        /// </param>
        /// <returns><c>True</c> o <c>False</c> a seconda dell'esito del test.</returns>
        public bool CheckRuoli(IEnumerable<string> values, ComparisionType type)
        {
            string[] ruoli = this.LoadRuoli().ToUpper().Split('|');

            foreach (string value in values)
            {
                if (type == ComparisionType.All)
                {
                    if (!ruoli.Contains(value.ToUpper()))
                    {
                        return false;
                    }
                }
                else
                {
                    if (ruoli.Contains(value.ToUpper()))
                    {
                        return true;
                    }
                }
            }

            return type == ComparisionType.All;
        }

        /// <summary>
        ///     Carica il nome dell'account windows dalla variabile
        ///     d'ambiente HTTP_INPS_ACCOUNT_WINDOWS.
        ///     Per l'esecuzione locale è restituito il valore contenuto nel file di configurazione
        ///     con chiave AmbienteTest.
        /// </summary>
        /// <returns>il nome dell'account windows del client.</returns>
        public string LoadAccountUtente()
        {
            return !ConfigurationManager.AppSettings["AmbienteTest"].Equals("1")
                       ? HttpContext.Current.Request.ServerVariables["HTTP_INPS_ACCOUNT_WINDOWS"].Trim()
                       : ConfigurationManager.AppSettings["UserAccount"];
        }

        public string LoadCodiceFiscaleOperatore()
        {
            return !ConfigurationManager.AppSettings["AmbienteTest"].Equals("1")
                       ? HttpContext.Current.Request.ServerVariables["HTTP_INPS_CODICE_FISCALE"].Trim()
                       : ConfigurationManager.AppSettings["OperatoreCF"];
        }

        /// <summary>
        ///     Carica il codice dell'operatore dalle variabili d'ambiente.
        ///     Il codice operatore è dato dalla variabile "HTTP_INPS_MATRICOLA".
        ///     Localmente il metodo restituisce la chiave CodOperatoreTest impostanta nel WebConfig
        /// </summary>
        /// <returns>Il codice dell'operatore.</returns>
        public string LoadCodiceOperatore()
        {
            try
            {
                if (!ConfigurationManager.AppSettings["AmbienteTest"].Equals("1")
                    && ConfigurationManager.AppSettings["ControlloIAM"].Equals("1"))
                {
                    return HttpContext.Current.Request.ServerVariables["HTTP_INPS_MATRICOLA"].Trim();
                }

                return ConfigurationManager.AppSettings["UserAccount"];
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        /// <summary>
        ///     Carica il codice sede (HTTP_INPS_CODICE_SEDE).
        /// </summary>
        /// <returns>il codice sede dell'operatore.</returns>
        public string LoadCodiceSede()
        {
            return !ConfigurationManager.AppSettings["AmbienteTest"].Equals("1", StringComparison.Ordinal)
                       ? HttpContext.Current.Request.ServerVariables["HTTP_INPS_CODICE_SEDE"].Trim()
                       : ConfigurationManager.AppSettings["UserCodSete"];
        }

        public string LoadCodiceSedeSAP()
        {
            return !ConfigurationManager.AppSettings["AmbienteTest"].Equals("1", StringComparison.Ordinal)
                       ? HttpContext.Current.Request.ServerVariables["HTTP_INPS_CODICE_SEDE_SAP"].Trim()
                       : ConfigurationManager.AppSettings["UserCodSedeSAP"];
        }

        /// <summary>
        ///     Carica i ruoli per l'operatore corrente. I ruoli restituiti sono filtrati
        ///     in modo da escludere quelli non gestiti.
        /// </summary>
        /// <param name="ruoliConsentiti">L'elenco dei ruoli gestiti da considerare separati da pipe.</param>
        /// <returns>Restituisce una stringa con l'elenco dei ruoli dell'operatore separati da pipe.</returns>
        public string LoadRuoli(string ruoliConsentiti)
        {
            // SV: semplice filtraggio dei ruoli.
            // Utile per gli operatori che hanno migliaia di ruoli.
            string ruoli = this.LoadRuoli();
            string[] ruoliFiltrati = (from ruolo in ruoli.Split('|')
                                      from ruoloGestito in ruoliConsentiti.Split('|')
                                      where ruolo.Equals(ruoloGestito, StringComparison.OrdinalIgnoreCase)
                                      select ruolo).ToArray();

            return string.Join("|", ruoliFiltrati);
        }

        /// <summary>
        ///     Carica tutti i ruoli disponibili per l'operatore corrente.
        ///     Sono restituiti come stringa unica separata da '|'.
        ///     Per ruolo si intende la coppia di valori codiceApplicazione:ruolo come ad esempio
        ///     "AssegnazionePin:Operatore" oppure "A1850:P2715". Per quei valori che hanno anche il
        ///     codice sede "A1850:P2715:003900" questo è escluso.
        /// </summary>
        /// <returns>Una stringa con i ruoli posseduti dall'operatore loggato.</returns>
        public string LoadRuoli()
        {
            StringBuilder returnString = new StringBuilder();
            try
            {


                string userRoles = ConfigurationManager.AppSettings["AmbienteTest"].Equals(
                    "1",
                    StringComparison.Ordinal)
                                       ? ConfigurationManager.AppSettings["UserRoles"]
                                       : HttpContext.Current.Request.ServerVariables["HTTP_INPS_RUOLI"];

                foreach (string role in from userRole in userRoles.Split('|')
                                        from cnUserRole in userRole.Split(',')
                                        where cnUserRole.ToUpperInvariant().Contains("CN=")
                                        select cnUserRole.Split(':')
                                            into tokens
                                            select tokens[0] + ':' + tokens[1])
                {
                    returnString.Append(role.Trim(new[] { 'c', 'n', 'C', 'N', '=' }) + "|");
                }

                if (returnString.Length > 1)
                {
                    returnString.Remove(returnString.Length - 1, 1);
                }

                return returnString.ToString();
            }
            catch (Exception ex)
            {
                //throw new PinProvisioningException("ProfilazioneIAM", 15001, 23, ex.InnerException);
                return returnString.ToString();
            }
        }
    }
}
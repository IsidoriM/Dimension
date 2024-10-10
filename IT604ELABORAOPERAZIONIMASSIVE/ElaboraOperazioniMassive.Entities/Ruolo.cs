using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElaboraOperazioniMassive.Entities
{
    [Serializable]
    public class Ruolo
    {
        /// <summary>
        /// Identificativo del superuser.
        /// </summary>
        public const string OperatoreSuperUser = "A1850:P9132";

        /// <summary>
        /// Identificativo dell'amministratore.
        /// </summary>
        public const string OperatoreAmministratore = "A1850:P2715";

        /// <summary>
        /// Identificativo di AssegnazionePin:Operatore
        /// </summary>
        public const string OperatoreAssegnazionePin = "AssegnazionePin:Operatore";

        /// <summary>
        /// Identificativo dell'operatore abilitato alla visualizzazione e download dei documenti.
        /// </summary>
        public const string OperatoreGai = "A1850:P9045";

        /// <summary>
        /// Identificativi separati da '|' degli operatori abilitati alla conversione del pin.
        /// </summary>
        public const string OperatoreCpd = "A1850:P2716|A1850:P2715|A1850:P9045";

        /// <summary>
        /// Elenco degli operatori abilitati ad assegnare l'OTP.
        /// </summary>
        public const string OperatoreOtp = "A1850:P2715|A1850:P9132";

        public int IdRuolo { get; set; }

        public string Descrizione { get; set; }

        public bool Attivo { get; set; }

        public string Codice { get; set; }
    }

}

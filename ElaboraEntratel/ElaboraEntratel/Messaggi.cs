using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElaboraEntratel
{
    class Messaggi
    {

        public static string PARSED_CERT_FILE = "Lista di certificati contenuti nel file: {0}";
        public static string PARSED_CRL_FILE = "CRL contenuta nel file: {0}";
        
        public static string ADDCERT_MESSAGE = "In data {0:d} alle ore {0:t} sono stati inseriti {1} certificati su {2}  certificati complessivi";

        public static string ERROR_MESSAGE = "La procedura nella fase {0} ha generato il seguente errore:{1}";

        public static string ERROR_MESSAGE_PARAM = "La procedura nella fase {0} ({1}) ha generato il seguente errore:{2}";
    }
}

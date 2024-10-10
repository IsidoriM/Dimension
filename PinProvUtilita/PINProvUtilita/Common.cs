using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PINProvUtilita
{
    public class Common
    {


        public static bool IsForeignFormat(string code)
        {
            // SV
            // Il codice ente non è sempre disponibile. Non controllo che sia valido.
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    throw new ArgumentException("Il codice per utenti esteri non può essere nullo.");
                }

                if (code.Length <= 6)
                {
                    return false;
                }

                string head = code.Substring(0, 2).ToUpperInvariant(); // prendo i primi due caratteri..
                string tail = code.Substring(code.Length - 5); // ..e gli ultimi 4 caratteri.
                int num;

                return head.Equals("EE", StringComparison.Ordinal) && int.TryParse(tail, out num);
            }
            catch
            {
                return false;
            }
        }
    }
}
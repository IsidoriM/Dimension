namespace PINProvUtilita.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CFUtility
    {
        #region Static Fields

        private static readonly Dictionary<char, int> ControlloOmocodice = new Dictionary<char, int>
                                                                               {
                                                                                   { 'L', 0 }, 
                                                                                   { 'M', 1 }, 
                                                                                   { 'N', 2 }, 
                                                                                   { 'P', 3 }, 
                                                                                   { 'Q', 4 }, 
                                                                                   { 'R', 5 }, 
                                                                                   { 'S', 6 }, 
                                                                                   { 'T', 7 }, 
                                                                                   { 'U', 8 }, 
                                                                                   { 'V', 9 }
                                                                               };

        private static readonly char[] ListaCaratteriDispari =
            {
                'B', 'A', 'K', 'P', 'L', 'C', 'Q', 'D', 'R', 'E', 'V', 
                'O', 'S', 'F', 'T', 'G', 'U', 'H', 'M', 'I', 'N', 'J', 
                'W', 'Z', 'Y', 'X'
            };

        private static readonly char[] ListaCaratteriPari =
            {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 
                'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 
                'Y', 'Z'
            };

        private static readonly char[] ListaCodiciCatastali =
            {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'L', 'M', 
                'Z'
            };

        private static readonly char[] ListaMese =
            {
                ' ', // per far si che i mesi abbiano un indice a base 1
                'A', // gennaio
                'B', // febbraio
                'C', // marzo
                'D', // aprile
                'E', // maggio
                'H', // giugno
                'L', // luglio
                'M', // agosto
                'P', // settembre
                'R', // ottobre
                'S', // novembre
                'T' // dicembre
            };

        private static readonly char[] ListaNumeriDispari =
            {
                '1', '0', ' ', ' ', ' ', '2', ' ', '3', ' ', '4', ' ', ' ', 
                ' ', '5', ' ', '6', ' ', '7', ' ', '8', ' ', '9'
            };

        private static readonly char[] ListaNumeriPari = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        #endregion

        #region Public Methods and Operators

        public static bool ControllaCheckDigit(string codiceFiscale)
        {
            int tempVal = 0;

            // Ciclo di conteggio dei valori sui primi 15 caratteri del codice fiscale
            for (int i = 0; i < 15; i++)
            {
                char ch = codiceFiscale[i];

                if (i % 2 == 0)
                {
                    // se pari lista dispari...
                    tempVal += char.IsDigit(ch)
                                   ? Array.IndexOf(ListaNumeriDispari, ch)
                                   : Array.IndexOf(ListaCaratteriDispari, ch);
                }
                else
                {
                    // se dispari lista pari...
                    tempVal += char.IsDigit(ch)
                                   ? Array.IndexOf(ListaNumeriPari, ch)
                                   : Array.IndexOf(ListaCaratteriPari, ch);
                }
            }

            // Estraggo il carattere di controllo
            char ceckdigit = codiceFiscale[15];
            return (tempVal % 26) == Array.IndexOf(ListaCaratteriPari, ceckdigit);
        }

        /// <summary>
        /// Controlla la correttezza del codice fiscale.
        /// </summary>
        /// <param name="codiceFiscale">Il codice fiscale da testare.</param>
        /// <returns><c>True</c> se il codice fiscale passato è valido; altrimenti <c>False</c>.</returns>
        public static bool ControllaCorrettezza(string codiceFiscale)
        {
            try
            {
                bool isLettera = false;

                codiceFiscale = codiceFiscale.ToUpperInvariant();

                if (codiceFiscale.Length < 16)
                {
                    return false;
                }

                // controllo dei primi 6 caratteri alfabetici
                for (int i = 0; i < 6; i++)
                {
                    if (!char.IsLetter(codiceFiscale[i]))
                    {
                        return false;
                    }
                }

                // TODO: SV - modifica temporanea per risolvere l'eccezzione
                // sollevata da ARCA.cs riga 87 nel caso in cui alle posizioni 6 e 7 invece di
                // valori numerici trova delle lettere.
                if (codiceFiscale.Substring(6, 2).Any(code => code < '0' || code > '9'))
                {
                    return false;
                }

                // controllo dell'anno
                for (int i = 6; i < 8; i++)
                {
                    if (!char.IsDigit(codiceFiscale[i]) && !ControlloOmocodice.ContainsKey(codiceFiscale[i]))
                    {
                        return false;
                    }
                }

                // controllo del mese
                if (!ListaMese.Contains(codiceFiscale[8]))
                {
                    return false;
                }

                // controllo del giorno
                for (int i = 9; i < 11; i++)
                {
                    if (!char.IsDigit(codiceFiscale[i]) && !ControlloOmocodice.ContainsKey(codiceFiscale[i]))
                    {
                        return false;
                    }
                }

                // controllo formale del giorno
                int giorno = TrasformaGiorno(codiceFiscale, 9);

                if (giorno > 31)
                {
                    giorno -= 40;
                }

                int mese = Array.IndexOf(ListaMese, codiceFiscale[8]);

                int anno = TrasformaGiorno(codiceFiscale, 6);

                // controllo dell'intera data
                if (!ControllaData(giorno, mese, anno))
                {
                    return false;
                }

                // controllo del 1° carattere del codice catastale
                if (!ListaCodiciCatastali.Contains(codiceFiscale[11]))
                {
                    return false;
                }

                for (int i = 12; i < 15; i++)
                {
                    if (!char.IsDigit(codiceFiscale[i]))
                    {
                        isLettera = true;

                        // controllo del codice catastale se è un omocode.
                        if (!ControlloOmocodice.ContainsKey(codiceFiscale[i]))
                        {
                            return false;
                        }
                    }
                }

                if (isLettera == false)
                {
                    int numeroCodCat = int.Parse(codiceFiscale.Substring(12, 3));

                    if (numeroCodCat == 000)
                    {
                        return false;
                    }

                    // se lettera M e le 3 cifre del cod. cat. non > di 399
                    if ((codiceFiscale[11] == 'M') && (numeroCodCat > 399))
                    {
                        return false;
                    }
                }

                if (ControllaCheckDigit(codiceFiscale))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool ControllaData(int giorno, int mese, int anno)
        {
            // controllo l'anno dopo averlo estrapolato dalla stringa
            try
            {
                // controlli di ammissibilità sul giorno e sul mese
                if ((mese > 12) || (giorno > 31) || (mese < 1) || (giorno < 1))
                {
                    return false;
                }

                // controllo mese
                switch (mese)
                {

                    case 2:
                        // febbraio
                        bool isBisestile = ControllaSeBisestile(anno);

                        if ((isBisestile && (giorno > 29)) || (!isBisestile && (giorno > 28)))
                        {
                            return false;
                        }

                        break;

                    case 4:
                    // aprile
                    case 6:
                    // giugno
                    case 9:
                    // settembre
                    case 11:
                        // novembre
                        if (giorno > 30)
                        {
                            return false;
                        }

                        break;
                }

                // se arrivo a questo punto vuol dire che la data è corretta
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Controlla se l'anno indicato è bisestile oppure no.
        /// Si può indicare anche un anno di due sole cifre ma in quel caso il controllo
        /// non è perfetto poiché non posso applicare la regola sugli anni secolari.
        /// </summary>
        /// <param name="anno">Anno di 4 o 2 cifre.</param>
        /// <returns>
        ///     <c>True</c> se l'anno è bisestile; <c>False</c> altrimenti.
        ///     <b>Attenzione che indicando un anno di due cifre per i secolari (1800, 1900..)
        ///     ottendo sempre <c>True</c>.</b>
        /// </returns>
        private static bool ControllaSeBisestile(int anno)
        {
            // Per un anno di due cifre non posso applicare la regola standard
            // del calendario gregoriano sugli anni secolari (1800, 1900, 2000 ...)
            // quindi per questi anni il risultato sarà sempre true.
            if (anno >= 0 && anno < 99)
            {
                return anno % 4 == 0;
            }

            // Se l'anno è di 4 cifre applico l'algoritmo standard del calendario gregoriano.
            return DateTime.IsLeapYear(anno);
        }

        /// <summary>
        /// </summary>
        /// <param name="c">
        ///     c=byte campo substring
        ///     9 giorno
        ///     6 anno
        /// </param>
        /// <returns></returns>
        private static int TrasformaGiorno(string codiceFiscale, int c)
        {
            string temp = string.Empty;

            for (int i = c; i < c + 2; i++)
            {
                if (char.IsDigit(codiceFiscale[i]))
                {
                    temp += codiceFiscale[i];
                }
                else
                {
                    temp += ControlloOmocodice[codiceFiscale[i]];
                }
            }

            return int.Parse(temp);
        }

        #endregion
    }

    public class ControlloCodiceFiscale
    {
        #region Public Methods and Operators

        //public static bool CheckCodiceFiscale(string codiceFiscale)
        //{
        //    ////bool result = false;
        //    if (string.IsNullOrEmpty(codiceFiscale))
        //    {
        //        return false;
        //    }

        //    if (codiceFiscale.Length != 16)
        //    {
        //        return false;
        //    }

        //    // TODO: SV - modifica temporanea per risolvere l'eccezzione
        //    // sollevata da ARCA.cs riga 87 nel caso in cui alle posizioni 6 e 7 invece di
        //    // valori numerici trova delle lettere.
        //    if (codiceFiscale.Substring(6, 2).Any(code => code < '0' || code > '9'))
        //    {
        //        return false;
        //    }

        //    try
        //    {
        //        // stringa per controllo e calcolo omocodia
        //        const string Omocodici = "LMNPQRSTUV";

        //        // per il calcolo del check digit e la conversione in numero
        //        const string ListaControllo = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        //        int[] listaPari =
        //            {
        //                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 
        //                24, 25
        //            };
        //        int[] listaDispari =
        //            {
        //                1, 0, 5, 7, 9, 13, 15, 17, 19, 21, 2, 4, 18, 20, 11, 3, 6, 8, 12, 14, 16, 10, 22, 
        //                25, 24, 23
        //            };

        //        codiceFiscale = codiceFiscale.ToUpper();
        //        char[] codiceArray = codiceFiscale.ToCharArray();

        //        // check della correttezza formale del codice fiscale
        //        // elimino dalla stringa gli eventuali caratteri utilizzati negli
        //        // spazi riservati ai 7 che sono diventati carattere in caso di omocodia
        //        for (int k = 6; k < 15; k++)
        //        {
        //            if ((k == 8) || (k == 11))
        //            {
        //                continue;
        //            }

        //            int x = Omocodici.IndexOf(codiceArray[k]);

        //            if (x != -1)
        //            {
        //                codiceArray[k] = x.ToString(CultureInfo.InvariantCulture).ToCharArray()[0];
        //            }
        //        }

        //        ////Regex(@"^[A-Z]{6}[0-9]{2}[A-Z][0-9]{2}[A-Z][0-9]{3}[A-Z]$")
        //        ////Regex rgx = new Regex(@"^[A-Z]{6}[]{2}[A-Z][]{2}[A-Z][]{3}[A-Z]$");
        //        ////Match m = rgx.Match(new string(cCodice));
        //        ////result = m.Success;
        //        ////result = true;

        //        // da una verifica ho trovato 3 risultati errati su più di 4000  codici fiscali
        //        // ho temporaneamente rimosso il test con le Regular fino a quando non riuscirò a capire perchè in alcuni casi sbaglia

        //        // normalizzato il codice fiscale se la regular non ha buon
        //        // fine è inutile continuare
        //        ////if (result)
        //        ////{
        //        int somma = 0;

        //        // ripristino il codice fiscale originario
        //        // grazie a Lino Barreca che mi ha segnalato l'errore
        //        codiceArray = codiceFiscale.ToCharArray();

        //        for (int i = 0; i < 15; i++)
        //        {
        //            char c = codiceArray[i];
        //            int x = "0123456789".IndexOf(c);

        //            if (x != -1)
        //            {
        //                c = ListaControllo.Substring(x, 1).ToCharArray()[0];
        //            }

        //            x = ListaControllo.IndexOf(c);

        //            // i modulo 2 = 0 è dispari perchè iniziamo da 0
        //            // controllo sulla presenza di caratteri non validi (. + - ecc..)
        //            if (x != -1)
        //            {
        //                x = (i % 2) == 0 ? listaDispari[x] : listaPari[x];
        //                somma += x;
        //            }
        //        }

        //        ////result = ListaControllo.Substring(somma % 26, 1) == codiceFiscale.Substring(15, 1);
        //        ////}
        //        ////return result;
        //        return ListaControllo.Substring(somma % 26, 1) == codiceFiscale.Substring(15, 1);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("[Class: ControlloCodiceFiscale; Cod: 3001] " + ex.Message, ex);
        //    }
        //}

        public static string MeseCodiceFiscale(string lettera)
        {
            string mese = string.Empty;

            switch (lettera.ToLower())
            {
                case "a":
                    mese = "01";
                    break;
                case "b":
                    mese = "02";
                    break;
                case "c":
                    mese = "03";
                    break;
                case "d":
                    mese = "04";
                    break;
                case "e":
                    mese = "05";
                    break;
                case "h":
                    mese = "06";
                    break;
                case "l":
                    mese = "07";
                    break;
                case "m":
                    mese = "08";
                    break;
                case "p":
                    mese = "09";
                    break;
                case "r":
                    mese = "10";
                    break;
                case "s":
                    mese = "11";
                    break;
                case "t":
                    mese = "12";
                    break;
            }

            return mese;
        }

        public static Dictionary<string, object> VerificaSemanticaCF(Dictionary<string, object> args)
        {
            ////string codiceFiscale, string cognome, string nome, string dataNascita, string sesso, string comuneNascita)
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["error"] = false;
            result["nome"] = true;
            result["cognome"] = true;
            result["datanascita"] = true;
            result["comune"] = true;

            try
            {
                // maiuscolo
                args["codicefiscale"] = args["codicefiscale"].ToString().ToUpper();
                args["cognome"] = args["cognome"].ToString().ToUpper();
                args["nome"] = args["nome"].ToString().ToUpper();
                args["sesso"] = args["sesso"].ToString().ToUpper();
                args["comune"] = args["comune"].ToString().ToUpper();

                // match cognome
                if (
                    !args["codicefiscale"].ToString()
                         .Substring(0, 3)
                         .Equals(CalcolaCognomeCF(args["cognome"].ToString())))
                {
                    result["cognome"] = false;
                }

                // match nome
                if (!args["codicefiscale"].ToString().Substring(3, 3).Equals(CalcolaNomeCF(args["nome"].ToString())))
                {
                    result["nome"] = false;
                }

                // match anno nascita
                if (
                    !args["codicefiscale"].ToString()
                         .Substring(6, 2)
                         .Equals(args["datanascita"].ToString().Substring(8, 2)))
                {
                    result["datanascita"] = false;
                }

                // match mese di nascita
                if (
                    !MeseCodiceFiscale(args["codicefiscale"].ToString().Substring(8, 1))
                         .Equals(args["datanascita"].ToString().Substring(3, 2)))
                {
                    result["datanascita"] = false;
                }

                // match giorno di nascita
                if (
                    !args["codicefiscale"].ToString()
                         .Substring(9, 2)
                         .Equals(args["datanascita"].ToString().Substring(0, 2))
                    && !args["codicefiscale"].ToString()
                            .Substring(9, 2)
                            .Equals((int.Parse(args["datanascita"].ToString().Substring(0, 2)) + 40).ToString()))
                {
                    result["datanascita"] = false;
                }

                // match comune di nascita
                // la prima condizione se non presenta 3 numeri vuol dire che è un caso di omocodia e quindi by-passo il controllo del comune
                int numero = 0;
                if (int.TryParse(args["codicefiscale"].ToString().Substring(12, 3), out numero) == true
                    && !args["codicefiscale"].ToString().Substring(11, 4).Equals(args["comune"].ToString()))
                {
                    result["comune"] = false;
                }
            }
            catch
            {
                result["error"] = true;
                return result;
            }

            return result;
        }

        #endregion

        #region Methods

        private static string CalcolaCognomeCF(string cognome)
        {
            int i = 0;
            string stringa;

            for (stringa = string.Empty; stringa.Length < 3 && i + 1 <= cognome.Length; i++)
            {
                if ("BCDFGHJKLMNPQRSTVWXYZ".Contains(cognome.ToUpper().Substring(i, 1)))
                {
                    stringa += cognome.ToUpper().Substring(i, 1);
                }
            }

            if (stringa.Length > 3)
            {
                stringa = stringa.Substring(0, 3);
            }

            for (i = 0; stringa.Length < 3 && i + 1 <= cognome.Length; i++)
            {
                if ("AEIOU".Contains(cognome.ToUpper().Substring(i, 1)))
                {
                    stringa += cognome.ToUpper().Substring(i, 1);
                }
            }

            if (stringa.Length < 3)
            {
                for (i = stringa.Length; i < 3; i++)
                {
                    stringa += "X";
                }
            }

            return stringa;
        }

        private static string CalcolaNomeCF(string nome)
        {
            int i = 0;
            string stringa = string.Empty;
            string cons;

            for (cons = string.Empty; cons.Length < 4 && i + 1 <= nome.Length; i++)
            {
                if ("BCDFGHJKLMNPQRSTVWXYZ".Contains(nome.ToUpper().Substring(i, 1)))
                {
                    cons += nome.ToUpper().Substring(i, 1);
                }
            }

            if (cons.Length > 3)
            {
                stringa = cons.Substring(0, 1) + cons.Substring(2, 2);
            }
            else
            {
                stringa = cons;
            }

            for (i = 0; stringa.Length < 3 && i + 1 <= nome.Length; i++)
            {
                if ("AEIOU".Contains(nome.ToUpper().Substring(i, 1)))
                {
                    stringa += nome.ToUpper().Substring(i, 1);
                }
            }

            if (stringa.Length < 3)
            {
                for (i = stringa.Length; i < 3; i++)
                {
                    stringa += "X";
                }
            }

            return stringa;
        }

        #endregion
    }
}

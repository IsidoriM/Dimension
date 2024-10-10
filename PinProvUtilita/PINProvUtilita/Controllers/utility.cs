using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MenuPinProvisioning;
using System.Text.RegularExpressions;

namespace PINProvUtilita.Controllers
{
    public class utility
    {
        private static int[] ListaPari = { 0, 2, 4, 6, 8, 1, 3, 5, 7, 9 };


        public static bool ControllaPartitaIva(string PartitaIva)
        {

            // normalizziamo la cifra

            //if (PartitaIva.Length < 11)

            //    PartitaIva = PartitaIva.PadLeft(11, '0');

            // lunghezza errata non fa neanche il controllo

            if (PartitaIva.Length != 11)

                return false;

            int Somma = 0;

            for (int k = 0; k < 11; k++)
            {

                string s = PartitaIva.Substring(k, 1);

                // otteniamo contemporaneamente

                // il valore, la posizione e testiamo se ci sono

                // caratteri non numerici

                int i = "0123456789".IndexOf(s);

                if (i == -1)

                    return false;

                int x = int.Parse(s);

                if (k % 2 == 1) // Pari perchè iniziamo da zero

                    x = ListaPari[i];

                Somma += x;

            }

            return ((Somma % 10 == 0) && (Somma != 0));

        }

        public static bool ControllaCertificatiEntratel(string Tipologia)
        {
            bool certificatoOK = false;
            string CertificatiSelezionati = System.Configuration.ConfigurationManager.AppSettings["Certificati"];

            if (CertificatiSelezionati.Contains(Tipologia))
                certificatoOK = true;


            return certificatoOK;
        }

        public static string GetCodiciOperatore()
        {
            try
            {
                TestMenuEnteMvc.Class.ProfilazioneIam P = new TestMenuEnteMvc.Class.ProfilazioneIam();
                string ruoliAttivi = string.Empty;
                string matricolaOperatore = P.LoadCodiceOperatore();

                if (string.IsNullOrEmpty(ruoliAttivi))
                {
                    TestMenuEnteMvc.Class.ControllerBase C = new TestMenuEnteMvc.Class.ControllerBase();
                    ruoliAttivi = C.LoadRuoliAttiviString();
                }

                string ruoli = matricolaOperatore.Contains("CRM:") ? "CRM:CCI" : P.LoadRuoli(ruoliAttivi);

                return ruoli;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string CaricaMenu(string matricolaoperatore, String idFunzionalita)
        {

            string ruoli = GetCodiciOperatore();
            TestMenuEnteMvc.Class.ProfilazioneIam P = new TestMenuEnteMvc.Class.ProfilazioneIam();
            string accountWindows = P.LoadAccountUtente();

            MainManager menu = new MainManager();

            string menu2;

            menu2 = menu.getMenu(ruoli, matricolaoperatore, accountWindows, idFunzionalita);

            return menu2;

        }


        public string CheckCodiceUtente(string codiceUtente)
        {
            string errorMsg = string.Empty;

            if (string.IsNullOrEmpty(codiceUtente))
            {
                errorMsg = "Il campo è obbligatorio";
            }

            else if (!CFUtility.ControllaCorrettezza(codiceUtente))
            {
                if (!Common.IsForeignFormat(codiceUtente))
                {
                    errorMsg = "Il valore inserito non è valido";
                }
                else
                {
                    //this.IsCodiceUtenteEstero = true;
                }
            }
            return errorMsg;
        }
        public static string CaricaMenu(string matricolaoperatore)
        {

            string ruoli = GetCodiciOperatore();
            TestMenuEnteMvc.Class.ProfilazioneIam P = new TestMenuEnteMvc.Class.ProfilazioneIam();
            string accountWindows = P.LoadAccountUtente();

            MainManager menu = new MainManager();

            string menu2;

            menu2 = menu.getMenu(ruoli, matricolaoperatore, accountWindows, String.Empty);

            return menu2;

        }

        /// <summary>
        /// ggrassi04 decripta il cf
        /// </summary>
        /// <param name="cf"></param>
        /// dato un cf criptato di 20 caratteri
        /// lo decripta restituendo un cf di 16
        /// <returns> cf decriptato </returns>
        public static string DecifraCodiceFiscale(string cf)
        {
            /// array di caratteri da ripristinare
            string[] origin = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            /// array di caratteri criptati
            string[] cript = { "V", "8", "N", "D", "T", "H", "Q", "B", "J", "M", "3", "2", "0", "6", "C", "P", "7", "1", "K", "Y", "I", "R", "A", "S", "U", "E", "Z", "X", "O", "4", "W", "G", "5", "F", "9", "L" };
            /// array di caratteri cf
            char[] arrcf;
            string oricf = "";
            string element = "";
            int index = 0;
            try
            {
                arrcf = cf.ToCharArray();
                if (string.IsNullOrEmpty(cf)
                    || (cf.Length != 20))
                {
                    return "ERRORE";
                }
                else
                {
                    for (int i = 0; i < arrcf.Length; i++)
                    {
                        if (i != 1 && i != 3 && i != 6 && i != 14)
                        {
                            element = arrcf[i].ToString();
                            for (int t = 0; t < cript.Length; t++)
                            {
                                if (cript[t] == element.ToUpper())
                                {
                                    index = t;
                                    oricf += origin[index].ToString();
                                }
                            }

                        }
                    }
                }

                return oricf;
            }
            catch (Exception ex)
            {
                return "ERRORE";
            }
            finally
            {
            }
        }

        public static bool IsNumeric(string str)
        {
            float f;
            return float.TryParse(str, out f);
        }

        public static bool ValidateCellNumber(string value)
        {

            const string RegExpCell = @"^[\+]?\d{8,20}$";                                  //espressione regolare apavan01
            //const string RegExpCell = "^([0]{2}[1-9]{1,4}|[3]{1})([0-9]{8,15})";            //espressione regolare Pellegrini 2
            //const string RegExpCell = "^(0{2}[1-9]{1,4}|3){1}[0-9]{6,15}$";               espressione regolare ggrassi 2
            //const string RegExpCell = "^(\\[1-9]{1,4}|0{2}[1-9]{1,4}|3{1}[0-9]{6,15}$";   espressione regolare ggrassi 1
            //const string RegExpCell = "^(\\0{2}[1-9]{1,4}|3){1}[0-9]{6,15}$";             espressione regolare ggrassi 0
            //const string RegExpCell = "^(\\+[1-9]{1,4}|0{2}[1-9]{1,4}|3){1}[0-9]{6,15}$"; espressione regolare concessa da Moio Tommaso
            try
            {

                return IsNumeric(value) ? Regex.IsMatch(value, RegExpCell) : IsNumeric(value);
                // return Regex.IsMatch(value, RegExpCell);

            }
            catch (Exception ex)
            {
                // SV: traccio l'errore ma non blocco l'esecuzione.
                //this.Logger.TraceError(
                //    LogEvents.Errore,
                //    1051,
                //    ex.Message,
                //    new Dictionary<string, object> { { "CF", this.IdUtente } });
                if (ex.ToString() != "")
                {
                    return false;
                }
                return false;
            }

        }

        public static bool ValidateEmail(string value)
        {
            //const string RegExpEmail = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            const string RegExpEmail = "^[a-zA-Z0-9_-]+(?:\\.[a-zA-Z0-9_-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$";


            try
            {
                return Regex.IsMatch(value, RegExpEmail);
            }
            catch (Exception ex)
            {
                // SV: traccio l'errore ma non blocco l'esecuzione.
                //this.Logger.TraceError(
                //    LogEvents.Errore,
                //    1052,
                //    ex.Message,
                //    new Dictionary<string, object> { { "CF", this.IdUtente } });
                if (ex.ToString() != "")
                {
                    return false;
                }
                return false;
            }
        }

        public static Boolean stringFormatQuery(string[] arrCodice, string Verifica)
        {
            Boolean codice = false;

            if (arrCodice.Length > 0)
            {
                for (int i = 0; i < arrCodice.Length; i++)
                {
                    if (Verifica.Contains(arrCodice[i].ToString()))
                        return true;

                }
                //  codiceSede = codiceSede.Remove(codiceSede.Length - 1, 1);
            }
            return codice;


        }

    }
}
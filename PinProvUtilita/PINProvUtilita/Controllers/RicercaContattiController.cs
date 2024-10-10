using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PINProvUtilita.Models;
using System.Configuration;

namespace PINProvUtilita.Controllers
{
    public class RicercaContattiController : Controller
    {
        //
        // GET: /RicercaContatti/
        public ActionResult Index()
        {
            ViewData["listContatti"] = null;
            ViewData["totali"] = null;
            TestMenuEnteMvc.Class.ProfilazioneIam P = new TestMenuEnteMvc.Class.ProfilazioneIam();

            string matricolaoperatore;
            matricolaoperatore = P.LoadCodiceOperatore();


            //  String AttivaTabEccezione = ConfigurationManager.AppSettings["AttivaTabEccezione"];
            //  string Ruoli = P.LoadRuoli();
            //  string[] ArrayCodice = Ruoli.Split('|');
            //  Boolean visualizzaTabEccezione = PINProvUtilita.Controllers.utility.stringFormatQuery(ArrayCodice, AttivaTabEccezione);
            //  Session["visualizzaTabEccezione"] = visualizzaTabEccezione;

            bool caricaMenu = true;

            if (Session["MatricolaOperatore"] != null)
            {
                if (Session["MatricolaOperatore"].ToString().Trim().Equals(matricolaoperatore))
                    caricaMenu = true;
                else
                    Session["MatricolaOperatore"] = matricolaoperatore;
            }
            else
                Session["MatricolaOperatore"] = matricolaoperatore;


            string paramM = ConfigurationManager.AppSettings["ParamRicercaContatti"];

            if (caricaMenu)
                Session["listaCertificati"] = PINProvUtilita.Controllers.utility.CaricaMenu(matricolaoperatore, paramM);

            Session["radioButton"] = "EMAIL";
            String Eccezione = (String)Session["eccezione"];
            if (String.IsNullOrEmpty(Eccezione)) ;
            Session["eccezione"] = "RICERCA";

            return View();
        }

        public ActionResult RicercaContatti()
        {
            if (Session["MatricolaOperatore"] != null)
            {

            }
            else
            {

                bool caricaMenu = true;
                TestMenuEnteMvc.Class.ProfilazioneIam P = new TestMenuEnteMvc.Class.ProfilazioneIam();
                string matricolaoperatore;
                matricolaoperatore = P.LoadCodiceOperatore();
                Session["MatricolaOperatore"] = matricolaoperatore;


                string paramM = ConfigurationManager.AppSettings["ParamRicercaContatti"];

                if (caricaMenu)
                    Session["listaCertificati"] = PINProvUtilita.Controllers.utility.CaricaMenu(matricolaoperatore, paramM);

                Session["radioButton"] = "EMAIL";
                Session["eccezione"] = "RICERCA";

            }

            return View("Index");
        }


        [HttpPost]
        public ActionResult RicercaContatti(RicercaContatti ricCont)
        {
            ViewData["tabAttivo"] = "Ricerca_Contatti";
            string operatore = (String)Session["MatricolaOperatore"];
            List<ListaContatti> listContatti = new List<ListaContatti>();
            ViewData["listContatti"] = null;
            ViewData["totali"] = null;
            Boolean controllo = false;
            ViewData["alertMessage"] = null;

            try
            {
                Session["radioButton"] = ricCont.SelectContatto;

                if (ricCont.SelectContatto.Equals("EMAIL"))
                {
                    controllo = utility.ValidateEmail(ricCont.Email);
                }
                else if (ricCont.SelectContatto.Equals("PEC"))
                {
                    controllo = utility.ValidateEmail(ricCont.Pec);
                }
                else
                {
                    controllo = utility.ValidateCellNumber(ricCont.Cellulare);
                }
                if (controllo)
                {
                    RicercaContattiDB ric = new RicercaContattiDB();
                    listContatti = ric.RicercaContatti(ricCont.Email, ricCont.Pec, ricCont.Cellulare, ricCont.Certificato, operatore, this.Request.UserHostAddress);
                    ViewData["listContatti"] = listContatti;
                    String totali = totaliLista(listContatti, ricCont.SelectContatto);
                    ViewData["totali"] = totali;
                }
                else
                {
                    if (ricCont.SelectContatto.Equals("EMAIL"))
                    {
                        //  TempData["alertMessage"] = "alert('Il formato dell’indirizzo email non è corretto.');";
                        TempData["alertMessage"] = "Il formato dell’indirizzo email non è corretto.";
                    }
                    else if (ricCont.SelectContatto.Equals("PEC"))
                    {
                        //TempData["alertMessage"] = "alert('Il formato dell’indirizzo PEC non è corretto.');";
                        TempData["alertMessage"] = "Il formato dell’indirizzo PEC non è corretto.";
                    }
                    else
                    {
                        // TempData["alertMessage"] = "alert('Il formato del numero di Cellulare non è corretto.');";
                        TempData["alertMessage"] = "Il formato del numero di Cellulare non è corretto.";
                    }


                    if (TempData["alertMessage"] != null)
                    {
                        chiamataPopup("RicercaContatti");
                        ViewData["alertMessage"] = "Entro";
                        ViewBag.Message = TempData["alertMessage"].ToString();
                    }
                }

            }
            catch (Exception e)
            {

            }
            finally
            {

            }
            return View("Index");
        }

        public ActionResult InserisciEccezione()
        {
            ViewData["tabAttivo"] = "Gestione_Eccezioni";
            Session["eccezione"] = "INSERIMENTO";
            return View("Index");
        }


        public ActionResult AnnullaEccezione()
        {
            ViewData["tabAttivo"] = "Gestione_Eccezioni";
            Session["eccezione"] = "RICERCA";
            Session["ContattoNonCensito"] = null;
            return View("Index");
        }

        public ActionResult RicercaEccezione()
        {
            ViewData["tabAttivo"] = "Gestione_Eccezioni";
            Session["eccezione"] = "RICERCA";
            return View("Index");
        }


        [HttpPost]
        public ActionResult InserimentoEccezione(WhiteListNumContatto ricEccezone)
        {
            ViewData["tabAttivo"] = "Gestione_Eccezioni";
            int result = 0;
            Boolean verificaContatto = false;
            Boolean verificaCongruenzaTipoContatto = false;
            WhiteListNumContatto gestioneEccezione = new WhiteListNumContatto();
            Session["ContattoNonCensito"] = ricEccezone.Contatto;

            if (utility.ValidateEmail(ricEccezone.Contatto))
            {
                verificaContatto = true;
                if (ricEccezone.TipoContatto.Substring(0, 1).Equals("E") || ricEccezone.TipoContatto.Substring(0, 1).Equals("P"))
                    verificaCongruenzaTipoContatto = true;
            }
            else if (utility.ValidateCellNumber(ricEccezone.Contatto))
            {
                verificaContatto = true;
                if (ricEccezone.TipoContatto.Substring(0, 1).Equals("C"))
                    verificaCongruenzaTipoContatto = true;
            }
            else
            {
                TempData["alertMessage"] = "Il contatto digitato non è formalmente corretto";
            }

            if (verificaContatto)
            {
                if (!String.IsNullOrEmpty(ricEccezone.TipoContatto))
                {

                    if (verificaCongruenzaTipoContatto)
                    {
                        verificaContatto = true;
                    }
                    else
                    {
                        TempData["alertMessage"] = "Il tipo contatto selezionato non è congruente con il contatto indicato.";
                        verificaContatto = false;
                    }
                }
                else
                {
                    TempData["alertMessage"] = "E' obbligatorio indicare un valore per Tipo Contatto";
                    verificaContatto = false;
                }

            }
            if (verificaContatto)
            {
                if (ricEccezone.Limite > 0)
                {
                    verificaContatto = true;
                }
                else
                {
                    TempData["alertMessage"] = "E' obbligatorio indicare un valore per Limite";
                    verificaContatto = false;
                }

            }
            if (verificaContatto)
            {
                if (String.IsNullOrEmpty(ricEccezone.CodiceFiscaleSegnalazione))
                {
                    Session["ContattoNonCensito"] = null;
                    Session["eccezione"] = "RICERCA";
                    ViewData["gestioneEccezione"] = gestioneEccezione;
                    chiamataPopup("RicercaContatti");
                    GestioneEccezioneDB gestEcc = new GestioneEccezioneDB();
                    gestioneEccezione = gestEcc.RicercaEccezione(ricEccezone.Contatto);
                    if (String.IsNullOrEmpty(gestioneEccezione.Contatto))
                    {
                        result = gestEcc.SaveEccezione(ricEccezone);
                        if (result > 0)
                        {
                            TempData["alertMessage"] = "Inserimento effettuato.";
                        }
                        else
                        {
                            TempData["alertMessage"] = "Inserimento non effettuato.";
                        }

                    }
                    else
                    {
                        TempData["alertMessage"] = "Il contatto indicato è già presente.";
                    }
                }
                else
                {
                    if (CFUtility.ControllaCorrettezza(ricEccezone.CodiceFiscaleSegnalazione.Trim()))
                    {
                        Session["eccezione"] = "RICERCA";
                        ViewData["gestioneEccezione"] = gestioneEccezione;
                        chiamataPopup("RicercaContatti");
                        GestioneEccezioneDB gestEcc = new GestioneEccezioneDB();
                        gestioneEccezione = gestEcc.RicercaEccezione(ricEccezone.Contatto);
                        if (String.IsNullOrEmpty(gestioneEccezione.Contatto))
                        {
                            result = gestEcc.SaveEccezione(ricEccezone);
                            if (result > 0)
                            {
                                TempData["alertMessage"] = "Inserimento effettuato.";
                            }
                            else
                            {
                                TempData["alertMessage"] = "Inserimento non effettuato.";
                            }

                        }
                        else
                        {
                            TempData["alertMessage"] = "Il contatto indicato è già presente.";
                        }
                    }
                    else
                    {
                        TempData["alertMessage"] = "Il codice fiscale del segnalatore non è formalmente corretto.";
                    }
                }
            }
            chiamataPopup("RicercaContatti");
            ViewData["alertMessage"] = "Entro";
            ViewBag.Message = TempData["alertMessage"].ToString();

            return View("Index");
        }


        [HttpPost]
        public ActionResult RicercaEccezione(WhiteListNumContatto ricEccezone)
        {
            ViewData["tabAttivo"] = "Gestione_Eccezioni";
            Session["vecchiaEccezione"] = null;
            Session["ContattoNonCensito"] = null;
            Boolean verificaContatto = false;
            WhiteListNumContatto gestioneEccezione = new WhiteListNumContatto();

            if (utility.ValidateEmail(ricEccezone.Contatto))
            {
                verificaContatto = true;
            }
            else if (utility.ValidateCellNumber(ricEccezone.Contatto))
            {
                verificaContatto = true;
            }

            if (verificaContatto)
            {
                GestioneEccezioneDB gestEcc = new GestioneEccezioneDB();
                gestioneEccezione = gestEcc.RicercaEccezione(ricEccezone.Contatto);
                if (!String.IsNullOrEmpty(gestioneEccezione.Contatto))
                {
                    Session["eccezione"] = "VISUALIZZA";
                    ViewData["gestioneEccezione"] = gestioneEccezione;
                }
                else
                {
                    Session["ContattoNonCensito"] = ricEccezone.Contatto;
                    TempData["alertMessage"] = "Il contatto non è censito.";
                    chiamataPopup("RicercaContatti");
                    ViewData["alertMessage"] = "Entro";
                    ViewBag.Message = TempData["alertMessage"].ToString();
                }
            }
            else
            {
                TempData["alertMessage"] = "Il contatto digitato non è formalmente corretto.";
                chiamataPopup("RicercaContatti");
                ViewData["alertMessage"] = "Entro";
                ViewBag.Message = TempData["alertMessage"].ToString();
            }

            return View("Index");
        }

        [HttpGet]
        public ActionResult EliminaEccezione(bool confirm, String contatto)
        {
            ViewData["tabAttivo"] = "Gestione_Eccezioni";
            if (confirm)
            {
                int result = 0;
                GestioneEccezioneDB gestEcc = new GestioneEccezioneDB();
                result = gestEcc.EliminaEccezione(contatto);

                Session["eccezione"] = "RICERCA";
                TempData["alertMessage"] = "Cancellazione effettuata.";
                chiamataPopup("RicercaContatti");
                ViewData["alertMessage"] = "Entro";
                ViewBag.Message = TempData["alertMessage"].ToString();
            }
            return View("Index");
        }


        [HttpPost]
        public ActionResult ModificaEccezione(WhiteListNumContatto ricEccezone)
        {
            ViewData["tabAttivo"] = "Gestione_Eccezioni";
            Session["eccezione"] = "MODIFICA";
            ViewData["gestioneEccezione"] = ricEccezone;
            var model = new WhiteListNumContatto();
            model = ricEccezone;

            return View("Index");
        }


        [HttpPost]
        public ActionResult InserimentoModificaEccezione(WhiteListNumContatto modEccezone)
        {
            ViewData["tabAttivo"] = "Gestione_Eccezioni";
            Boolean verificaContatto = false;
            Boolean verificaCongruenzaTipoContatto = false;
            int result = 0;
            WhiteListNumContatto gestioneEccezione = new WhiteListNumContatto();
            gestioneEccezione = (WhiteListNumContatto)Session["vecchiaEccezione"];
            modEccezone.DataInserimento = gestioneEccezione.DataInserimento;
            modEccezone.Contatto = gestioneEccezione.Contatto;
            if (utility.ValidateEmail(modEccezone.Contatto))
            {
                verificaContatto = true;
                if (modEccezone.TipoContatto.Substring(0, 1).Equals("E") || modEccezone.TipoContatto.Substring(0, 1).Equals("P"))
                    verificaCongruenzaTipoContatto = true;
            }
            else if (utility.ValidateCellNumber(modEccezone.Contatto))
            {
                verificaContatto = true;
                if (modEccezone.TipoContatto.Substring(0, 1).Equals("C"))
                    verificaCongruenzaTipoContatto = true;

            }
            else
            {
                TempData["alertMessage"] = "Il contatto digitato non è formalmente corretto";
            }

            if (verificaContatto)
            {
                if (!String.IsNullOrEmpty(modEccezone.TipoContatto))
                {
                    if (verificaCongruenzaTipoContatto)
                    {
                        verificaContatto = true;
                    }
                    else
                    {
                        TempData["alertMessage"] = "Il tipo contatto selezionato non è congruente con il contatto indicato.";
                        verificaContatto = false;
                    }
                }
                else
                {
                    TempData["alertMessage"] = "E' obbligatorio indicare un valore per Tipo Contatto";
                    verificaContatto = false;
                }

            }
            if (verificaContatto)
            {
                if (modEccezone.Limite > 0)
                {
                    verificaContatto = true;
                }
                else
                {
                    TempData["alertMessage"] = "E' obbligatorio indicare un valore per Limite";
                    verificaContatto = false;
                }

            }

            if (verificaContatto)
            {
                if (String.IsNullOrEmpty(modEccezone.CodiceFiscaleSegnalazione))
                {
                    GestioneEccezioneDB gestEcc = new GestioneEccezioneDB();
                    Session["eccezione"] = "RICERCA";
                    if (modEccezone.Limite == gestioneEccezione.Limite && modEccezone.TipoContatto == gestioneEccezione.TipoContatto)
                    {
                        result = gestEcc.UpdateEccezione(modEccezone, gestioneEccezione.Contatto);
                    }
                    else
                    {
                        result = gestEcc.UpdateInsertEccezione(modEccezone, gestioneEccezione.Contatto);
                    }

                    if (result > 0)
                    {
                        TempData["alertMessage"] = "Aggiornamento effettuato";
                    }
                    else
                    {
                        TempData["alertMessage"] = "Aggiornamento non effettuato";
                    }
                }
                else
                {
                    if (CFUtility.ControllaCorrettezza(modEccezone.CodiceFiscaleSegnalazione.Trim()))
                    {
                        GestioneEccezioneDB gestEcc = new GestioneEccezioneDB();
                        Session["eccezione"] = "RICERCA";
                        if (modEccezone.Limite == gestioneEccezione.Limite && modEccezone.TipoContatto == gestioneEccezione.TipoContatto)
                        {
                            result = gestEcc.UpdateEccezione(modEccezone, gestioneEccezione.Contatto);
                        }
                        else
                        {
                            result = gestEcc.UpdateInsertEccezione(modEccezone, gestioneEccezione.Contatto);
                        }

                        if (result > 0)
                        {
                            TempData["alertMessage"] = "Aggiornamento effettuato";
                        }
                        else
                        {
                            TempData["alertMessage"] = "Aggiornamento non effettuato";
                        }


                    }
                    else
                    {
                        ViewData["gestioneEccezione"] = modEccezone;
                        TempData["alertMessage"] = "Il codice fiscale del segnalatore non è formalmente corretto.";
                    }
                }
            }
            else
            {
                ViewData["gestioneEccezione"] = modEccezone;
            }

            chiamataPopup("RicercaContatti");
            ViewData["alertMessage"] = "Entro";
            ViewBag.Message = TempData["alertMessage"].ToString();

            return View("Index");
        }


        private String totaliLista(List<ListaContatti> listContatti, String tipoContatto)
        {
            int totaleinfoprivacy = 0;
            int totalenoinfoprivacy = 0;
            int totalecertificato = 0;
            int totalecertificatoNoinfoprivacy = 0;
            for (int i = 0; i < listContatti.Count; i++)
            {
                if (!String.IsNullOrEmpty(listContatti[i].Infoprivacy))
                {
                    switch (tipoContatto)
                    {
                        case "EMAIL":
                            if (!listContatti[i].Statoverificaemail.Equals(""))
                            {
                                totalecertificato++;
                            }
                            else
                            {
                                totaleinfoprivacy++;
                            }

                            break;
                        case "PEC":
                            if (!listContatti[i].Statoverificapec.Equals(""))
                            {
                                totalecertificato++;
                            }
                            else
                            {
                                totaleinfoprivacy++;
                            }
                            break;
                        default:
                            if (!listContatti[i].Statoverificacellulare.Equals(""))
                            {
                                totalecertificato++;
                            }
                            else
                            {
                                totaleinfoprivacy++;
                            }
                            break;
                    }

                }
                else
                {
                    switch (tipoContatto)
                    {
                        case "EMAIL":
                            if (!listContatti[i].Statoverificaemail.Equals(""))
                            {
                                totalecertificatoNoinfoprivacy++; 
                            }
                            else
                            {
                                totalenoinfoprivacy++;
                            }

                            break;
                        case "PEC":
                            if (!listContatti[i].Statoverificapec.Equals(""))
                            {
                                totalecertificatoNoinfoprivacy++;
                            }
                            else
                            {
                                totalenoinfoprivacy++;
                            }
                            break;
                        default:
                            if (!listContatti[i].Statoverificacellulare.Equals(""))
                            {
                                totalecertificatoNoinfoprivacy++;
                            }
                            else
                            {
                                totalenoinfoprivacy++;
                            }
                            break;
                    }
                }
            }
            int totaleNonCertificati = totaleinfoprivacy + totalenoinfoprivacy;
            int totaleCertificati = totalecertificatoNoinfoprivacy + totalecertificato;
            String totali = "Totale:" + listContatti.Count.ToString() + "     di cui Certificati: " + totaleCertificati.ToString() + "     (con consenso:" + totalecertificato.ToString() + " - senza consenso:" + totalecertificatoNoinfoprivacy.ToString() + ")  e Non certificati:" + totaleNonCertificati.ToString() + "     (con consenso:" + totaleinfoprivacy.ToString() + "     - senza consenso:" + totalenoinfoprivacy.ToString() + " )    ";
            return totali;
        }

        public ActionResult Link(String id)
        {
            string link = ConfigurationManager.AppSettings["LINKGC"];
            link = link + "?CF=" + id;
            return Redirect(link);
        }

        private void chiamataPopup(String funzioneChiamante)
        {
            LogDelegati Log = new LogDelegati();
            String strUrl = "";
            int num = 0;

            try
            {
                String strPathAndQuery = HttpContext.Request.Url.AbsoluteUri;
                num = strPathAndQuery.IndexOf(funzioneChiamante);
                strUrl = strPathAndQuery.Substring(0, num) + "Home/AlertPopup";
                //Log.SaveLogPinProvisioning("chiamataPopup", funzioneChiamante.ToString(), (Int16)LogEvents.Errore, strUrl, Convert.ToInt16(num), null, this.Request.UserHostAddress);

                ViewBag.JavaScriptFunction = string.Format("ShowNewPage('" + strUrl + "');");
            }
            catch (Exception ex)
            {
                Log.SaveLogPinProvisioning("chiamataPopup", funzioneChiamante, (Int16)LogEvents.Errore, strUrl, Convert.ToInt16(num), ex.ToString(), this.Request.UserHostAddress);
            }
        }

    }
}

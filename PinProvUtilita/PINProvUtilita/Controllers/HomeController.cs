using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MenuPinProvisioning;
using PINProvUtilita.Models;
using TestMenuEnteMvc.Class;
using System.Diagnostics;
using System.Threading;

namespace PINProvUtilita.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        Boolean primaVolta;

        public PartialViewResult AlertPopup()
        {

            messaggi msg = new messaggi();
            if (TempData["PassaggioErrore"] != null)
            {
                msg.MsgErrore = TempData["PassaggioErrore"].ToString();
            }

            //msg.MsgErrore = "È obbligatorio inserire una Partita Iva.";
            return PartialView(msg);
        }

        public ActionResult GestioneDelegatiEntratel()
        {
            primaVolta = false;
            Session["primaVolta"] = primaVolta;

            if (TempData["alertMessage"] != null)
            {
                chiamataPopup("Home");
                ViewData["alertMessage"] = "Entro";
                ViewBag.Message = TempData["alertMessage"].ToString();
            }

            if (TempData["RicercaCodici"] != null)
            {
                ViewData["RicercaCodici"] = "Entro";
                ViewData["cod11"] = true;
                ViewData["partitaIva"] = (String)Session["CodiceFiscale"];
            }

            if (TempData["codiceerrato"] != null)
                ViewData["partitaIva"] = (String)Session["CodiceFiscale"];



            TestMenuEnteMvc.Class.ProfilazioneIam P = new TestMenuEnteMvc.Class.ProfilazioneIam();

            string matricolaoperatore;
            matricolaoperatore = P.LoadCodiceOperatore();

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

            if (caricaMenu)
                Session["listaCertificati"] = PINProvUtilita.Controllers.utility.CaricaMenu(matricolaoperatore);

            return View();
        }


        [HttpPost]
        public ActionResult GestioneDelegatiEntratel(Certificati codice)
        {
            string operatore = (String)Session["MatricolaOperatore"];
            string function = "GestioneDelegatiEntratel";
            try
            {

                if (codice.CodiceFiscale != null)
                {
                    // TempData["msg"] = "<script>alert('Change succesfully');</script>";
                    bool caricadati = false;
                    bool cod11 = false;
                    ViewData["Messaggio"] = "Non sono presenti certificati Entratel per il codice fiscale/p. iva indicato";
                    Session["alertMessage"] = null;
                    Session["CodiceFiscale"] = codice.CodiceFiscale.Trim();
                    if (codice.CodiceFiscale.Trim().Length == 16 || codice.CodiceFiscale.Trim().Length == 11)
                    {


                        if (codice.CodiceFiscale.Trim().Length == 16)
                        {

                            if (CFUtility.ControllaCorrettezza(codice.CodiceFiscale.Trim()))
                            {
                                caricadati = true;
                            }
                            else
                            {
                                TempData["alertMessage"] = "Il Codice Fiscale non è formalmente corretto";
                                //TempData["alertMessage"] = "alert('Il Codice Fiscale non è formalmente corretto.');";
                                TempData["codiceerrato"] = "codice";
                            }




                        }

                        if (codice.CodiceFiscale.Trim().Length == 11)
                        {
                            if (utility.ControllaPartitaIva(codice.CodiceFiscale.Trim()))
                            {
                                caricadati = true;
                                cod11 = true;
                            }
                            else
                            {
                                TempData["alertMessage"] = "La Partita Iva non è formalmente corretta";
                                //TempData["alertMessage"] = "alert('La Partita Iva non è formalmente corretta.');";
                                TempData["codiceerrato"] = "codice";
                            }

                        }

                    }
                    else
                    {
                        TempData["alertMessage"] = "Il codice fiscale non è formalmente corretto.";
                        //TempData["alertMessage"] = "alert('Il codice fiscale non è formalmente corretto.');";
                        TempData["codiceerrato"] = "codice";
                    }


                    if (caricadati)
                    {

                        List<Certificati> listaGetDatiGrid = new List<Certificati>();
                        FunctionDB func = new FunctionDB();

                        List<Lista_Delegati> listaGetDelegati = new List<Lista_Delegati>();


                        Session["CodiceIniziale"] = codice;

                        if (codice.CodiceFiscale.Trim().Length == 11)
                        {
                            ViewData["Messaggio"] = "Non sono presenti certificati Entratel per la p. iva indicata";

                            ViewData["SecondaTabella"] = "Non sono presenti deleghe";
                            function = "RicercaDelegaPIva";
                            listaGetDatiGrid = func.RicercaDelegaPIva(codice.CodiceFiscale.Trim(), operatore, this.Request.UserHostAddress);

                            // Session["listaCertificati"] = listaGetDatiGrid;
                        }
                        else
                        {

                            listaGetDatiGrid = func.RicercaEntratel(codice.CodiceFiscale.Trim(), operatore, this.Request.UserHostAddress);


                            ViewData["Messaggio"] = "Non sono presenti certificati Entratel per il codice fiscale indicato";

                            ViewData["SecondaTabella"] = "Non sono presenti deleghe per il soggetto";
                            function = "RicercaDelegaCF";
                            listaGetDelegati = func.RicercaDelegaCF(codice.CodiceFiscale.Trim(), operatore, this.Request.UserHostAddress);

                            // Session["listaGetDatiGridCF"] = listaGetDatiGrid;

                        }

                        Session["listaCertificatiCF"] = listaGetDatiGrid;
                        ViewData["listaGetDelegati"] = listaGetDelegati;
                        Session["listaGetDatiGrid"] = listaGetDatiGrid;
                        ViewData["RicercaCodici"] = "Entro";
                        ViewData["cod11"] = cod11;
                        return View();
                    }
                    else
                    {

                        return Redirect("GestioneDelegatiEntratel");
                    }
                }
                else
                {
                    TempData["alertMessage"] = "È obbligatorio inserire un codice fiscale di persona fisica o giuridica.";
                    //TempData["alertMessage"] = "alert('È obbligatorio inserire un codice fiscale di persona fisica o giuridica.');";
                    return Redirect("GestioneDelegatiEntratel");
                }
            }

            catch (Exception e)
            {
                string message = "Errore nella funzione " + function + " codicefiscale : " + codice.CodiceFiscale.Trim();
                string messageErrore = "Descrizione errore : " + e.ToString();
                LogDelegati Log = new LogDelegati();
                Log.SaveLogPinProvisioning(codice.CodiceFiscale.Trim(), operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, this.Request.UserHostAddress);

                TempData["codiceerrato"] = "codice";
                TempData["alertMessage"] = "Servizio momentaneamente non disponibile";
                //TempData["alertMessage"] = "alert('Servizio momentaneamente non disponibile');";

                return Redirect("GestioneDelegatiEntratel");

            }
            finally
            {

            }
        }

        [HttpGet]
        public ActionResult DeleteAction(bool confirm, String serialnumber, String codiceFiscale, String commonname)
        {
            Stopwatch min = new Stopwatch();
            min.Start();
            bool cancellazione = false;
            string operatore = (String)Session["MatricolaOperatore"];
            string partitaIva = (String)Session["CodiceFiscale"];
            try
            {
                if (confirm)
                {
                    FunctionDB func = new FunctionDB();
                    cancellazione = func.DeleteCertificatoDelegato(serialnumber, codiceFiscale, operatore, this.Request.UserHostAddress);
                    if (cancellazione)
                    {

                        string message = partitaIva.Trim().ToUpper() + " - SN: " + serialnumber + " - CN: " + commonname;
                        LogDelegati Log = new LogDelegati();
                        Log.SaveLogPinProvisioning(codiceFiscale.Trim().ToUpper(), operatore, (Int16)LogEvents.CancellazionePinProvisioning, message, 0, null, this.Request.UserHostAddress);
                        message = "CF=" + codiceFiscale.Trim().ToUpper() + ";SN=" + serialnumber + ";CN=" + commonname + ";CF_DELEGANTE=" + partitaIva;
                        Clog logger = new Clog();
                        min.Stop();
                        logger.SaveLogPinProvisioning(operatore, 101, (Int16)LogEvents.CancellazioneClog, message, min.ElapsedMilliseconds, null, this.Request.UserHostAddress);


                        //  TempData["alertMessage"] = "alert('Cancellazione effettuata con successo.');";
                        TempData["alertMessage"] = "Cancellazione effettuata con successo.";
                    }
                    else
                    {
                        //TempData["alertMessage"] = "alert('Servizio momentaneamente non disponibile.');";
                        TempData["alertMessage"] = "Servizio momentaneamente non disponibile.";
                    }
                    Certificati codice = new Certificati();

                    codice = (Certificati)Session["CodiceIniziale"];

                    List<Certificati> listaGetDatiGrid = new List<Certificati>();
                    listaGetDatiGrid = func.RicercaDelegaPIva(codice.CodiceFiscale.Trim(), operatore, this.Request.UserHostAddress);
                    Session["listaGetDatiGrid"] = listaGetDatiGrid;
                    TempData["RicercaCodici"] = "Entro";

                    return RedirectToAction("GestioneDelegatiEntratel");

                }
                else
                {
                    return View();
                }
            }

            catch (Exception e)
            {

                string message = "Errore nella funzione DeleteAction SerialNum : " + serialnumber + " - codicefiscale : " + codiceFiscale + " - CN= " + commonname;
                string messageErrore = "Descrizione errore : " + e.ToString();
                LogDelegati Log = new LogDelegati();
                Log.SaveLogPinProvisioning(codiceFiscale, operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, this.Request.UserHostAddress);

                //TempData["alertMessage"] = "alert('Servizio momentaneamente non disponibile');";
                TempData["alertMessage"] = "Servizio momentaneamente non disponibile";

                return Redirect("GestioneDelegatiEntratel");

            }
            finally
            {

            }

        }


        public ActionResult InsertEntratel()
        {


            if (TempData["alertMessage"] != null)
            {
                chiamataPopup("Home");
                ViewData["alertMessage"] = "Entro";
                ViewBag.Message = TempData["alertMessage"].ToString();
            }

            if (TempData["Codicefisc"] != null)
            {
                ViewData["Codicefisc"] = "Entro";
            }

            return View();
        }

        [HttpPost]
        public ActionResult InsertEntratel(Certificati codice)
        {


            Certificati CertificatoSelezionato = new Certificati();

            CertificatoSelezionato = codice;
            Session["CertificatoSelezionato"] = CertificatoSelezionato;



            return View();
        }



        public ActionResult InsertCodiceFiscale()
        {


            if (TempData["alertMessage"] != null)
            {
                chiamataPopup("Home");
                ViewData["alertMessage"] = "Entro";
                ViewBag.Message = TempData["alertMessage"].ToString();
                ViewData["listaGetCertificatiPIVA"] = Session["listaGetCertificatiPIVA"];
                ViewData["RicercaCodici"] = "Entro";
            }

            if (TempData["partitaIva"] != null)
            {
                ViewData["partitaIva"] = "Entro";

            }

            return View();
        }


        [HttpPost]
        public ActionResult InsertCodiceFiscale(Certificati codice)
        {

            string operatore = (String)Session["MatricolaOperatore"];

            try
            {

                if (codice.CodiceFiscale == null)
                {
                    primaVolta = (Boolean)Session["primaVolta"];
                    List<Certificati> listaGetDatiGrid = new List<Certificati>();

                    listaGetDatiGrid = (List<Certificati>)Session["listaCertificatiCF"];
                    Session["listaGetDatiGrid"] = listaGetDatiGrid;
                    if (primaVolta)
                    {
                        ViewData["alertMessage"] = "Entro";
                        // ViewBag.Message = "alert('È obbligatorio inserire una Partita Iva.');";
                        ViewBag.Message = "È obbligatorio inserire una Partita Iva.";
                        chiamataPopup("Home");
                    }
                    else
                    {
                        primaVolta = true;
                        Session["primaVolta"] = primaVolta;

                    }
                }
                else
                {
                    if (utility.ControllaPartitaIva(codice.CodiceFiscale.Trim()))
                    {
                        ViewData["RicercaCodici"] = "Entro";
                        List<Certificati> listaGetCertificatiPIVA = new List<Certificati>();
                        FunctionDB func = new FunctionDB();
                        listaGetCertificatiPIVA = func.RicercaEntratel(codice.CodiceFiscale, operatore, this.Request.UserHostAddress);
                        ViewData["listaGetCertificatiPIVA"] = listaGetCertificatiPIVA;
                        Session["listaGetCertificatiPIVA"] = listaGetCertificatiPIVA;
                        Session["partitaIva"] = codice.CodiceFiscale;
                    }
                    else
                    {
                        ViewData["alertMessage"] = "Entro";
                        //ViewBag.Message = "alert('La Partita Iva non è formalmente corretta.');";
                        ViewBag.Message = "La Partita Iva non è formalmente corretta.";
                        chiamataPopup("Home");
                        Session["partitaIva"] = codice.CodiceFiscale;
                        ViewData["partitaIva"] = "Entro";
                    }
                }
            }
            catch (Exception e)
            {

                if (codice.Codicefisc != null)
                {
                    Session["Codicefisc"] = codice.Codicefisc;
                    TempData["Codicefisc"] = "entro";
                }

                string message = "Errore nella funzione InsertCodiceFiscale codicefiscale : " + codice.Codicefisc;
                string messageErrore = "Descrizione errore : " + e.ToString();
                LogDelegati Log = new LogDelegati();
                Log.SaveLogPinProvisioning(codice.Codicefisc, operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, this.Request.UserHostAddress);
                //TempData["alertMessage"] = "alert('Servizio momentaneamente non disponibile.');";
                TempData["alertMessage"] = "Servizio momentaneamente non disponibile.";

                return Redirect("InsertEntratel");



            }
            finally
            {

            }




            return View();
        }


        [HttpGet]
        public PartialViewResult ViewPopup()
        {
            return PartialView("ViewPopup");
        }
        [HttpPost]
        public ActionResult ViewPopup(Certificati model)
        {
            return Redirect("GestioneDelegatiEntratel");
        }


        [HttpPost]
        public ActionResult SalvaCertificato(Certificati CD, string SelectCertificato)
        {

            Stopwatch min = new Stopwatch();
            min.Start();
            int reurncode;
            string CodiceFiscale = (String)Session["CodiceFiscale"];
            string operatore = (String)Session["MatricolaOperatore"];

            try
            {

                if (SelectCertificato != null)
                {
                    FunctionDB func = new FunctionDB();

                    String[] arr = SelectCertificato.Split(';');
                    CD.Serialnumber = arr[0].ToString();
                    CD.Commonname = arr[1].ToString();
                    CD.CodiceFiscale = arr[2].ToString();

                    reurncode = func.SaveCertificatoDelegato(CD.Serialnumber, CodiceFiscale, operatore, this.Request.UserHostAddress);

                    if (reurncode == 0)
                    {

                        string message = CD.CodiceFiscale + " - SN: " + CD.Serialnumber + " - CN: " + CD.Commonname;
                        LogDelegati Log = new LogDelegati();
                        Log.SaveLogPinProvisioning(CodiceFiscale.Trim().ToUpper(), operatore, (Int16)LogEvents.InserimentoPinProvisioning, message, 0, null, this.Request.UserHostAddress);
                        message = "CF=" + CodiceFiscale.Trim().ToUpper() + ";SN=" + CD.Serialnumber + ";CN=" + CD.Commonname + ";CF_DELEGANTE=" + CD.CodiceFiscale;
                        Clog logger = new Clog();
                        min.Stop();
                        logger.SaveLogPinProvisioning(operatore, 101, (Int16)LogEvents.InserimentoClog, message, min.ElapsedMilliseconds, null, this.Request.UserHostAddress);


                        //TempData["alertMessage"] = "alert('Salvataggio eseguito con successo.');";
                        TempData["alertMessage"] = "Salvataggio eseguito con successo.";
                        return Redirect("GestioneDelegatiEntratel");
                    }
                    else if (reurncode == 2)
                    {
                        // TempData["alertMessage"] = "alert('Il soggetto è già delegato per il certificato selezionato.');";
                        TempData["alertMessage"] = "Il soggetto è già delegato per il certificato selezionato.";
                        TempData["partitaIva"] = "entro";
                        return Redirect("InsertCodiceFiscale");
                    }
                    else
                    {
                        // TempData["alertMessage"] = "alert('Servizio momentaneamente non disponibile.');";
                        TempData["alertMessage"] = "Servizio momentaneamente non disponibile.";
                        TempData["partitaIva"] = "entro";
                        return Redirect("InsertCodiceFiscale");
                    }
                }
                else
                {
                    // TempData["alertMessage"] = "alert('Selezionare un certificato dall’elenco.');";
                    TempData["alertMessage"] = "Selezionare un certificato dall’elenco.";
                    TempData["partitaIva"] = "entro";
                    return Redirect("InsertCodiceFiscale");
                }

            }
            catch (Exception e)
            {

                string message = "Errore nella funzione SalvaCertificato codicefiscale : " + CodiceFiscale;
                string messageErrore = "Descrizione errore : " + e.ToString();
                LogDelegati Log = new LogDelegati();
                Log.SaveLogPinProvisioning(CodiceFiscale, operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, this.Request.UserHostAddress);



                //TempData["alertMessage"] = "alert('Servizio momentaneamente non disponibile.');";
                TempData["alertMessage"] = "Servizio momentaneamente non disponibile.";

                return Redirect("GestioneDelegatiEntratel");
            }
            finally
            {

            }
        }
        [HttpPost]
        public ActionResult SaveEntratel(Certificati codice)
        {
            Stopwatch min = new Stopwatch();
            min.Start();
            int reurncode;
            string operatore = (String)Session["MatricolaOperatore"];
            Certificati CertificatoSelezionato = new Certificati();
            CertificatoSelezionato = (Certificati)Session["CertificatoSelezionato"];
            string partitaIva = (String)Session["CodiceFiscale"];

            try
            {

                if (codice.Codicefisc != null)
                {

                    if (codice.Codicefisc.Trim().Length == 16 && CFUtility.ControllaCorrettezza(codice.Codicefisc.Trim()))
                    {

                        Session["Codicefisc"] = codice.Codicefisc;
                        FunctionDB func = new FunctionDB();

                        reurncode = func.SaveCertificatoDelegato(CertificatoSelezionato.Serialnumber, codice.Codicefisc, operatore, this.Request.UserHostAddress);

                        if (reurncode == 0)
                        {

                            string message = partitaIva + " - SN: " + CertificatoSelezionato.Serialnumber + " - CN: " + CertificatoSelezionato.Commonname;
                            LogDelegati Log = new LogDelegati();
                            Log.SaveLogPinProvisioning(codice.Codicefisc.Trim().ToUpper(), operatore, (Int16)LogEvents.InserimentoPinProvisioning, message, 0, null, this.Request.UserHostAddress);
                            message = "CF=" + codice.Codicefisc.Trim().ToUpper() + ";SN=" + CertificatoSelezionato.Serialnumber + ";CN=" + CertificatoSelezionato.Commonname + ";CF_DELEGANTE=" + partitaIva;
                            Clog logger = new Clog();
                            min.Stop();
                            logger.SaveLogPinProvisioning(operatore, 101, (Int16)LogEvents.InserimentoClog, message, min.ElapsedMilliseconds, null, this.Request.UserHostAddress);


                            //TempData["alertMessage"] = "alert('Salvataggio eseguito con successo.');";
                            TempData["alertMessage"] = "Salvataggio eseguito con successo.";
                            return Redirect("GestioneDelegatiEntratel");
                        }
                        else if (reurncode == 2)
                        {
                            //TempData["alertMessage"] = "alert('Il soggetto è già delegato per il certificato selezionato.');";
                            TempData["alertMessage"] = "Il soggetto è già delegato per il certificato selezionato.";
                            TempData["Codicefisc"] = "entro";
                            return Redirect("InsertEntratel");
                        }
                        else
                        {
                            //TempData["alertMessage"] = "alert('Servizio momentaneamente non disponibile.');";
                            TempData["alertMessage"] = "Servizio momentaneamente non disponibile.";
                            TempData["Codicefisc"] = "entro";
                            return Redirect("InsertEntratel");
                        }



                    }
                    else
                    {
                        //TempData["alertMessage"] = "alert('Codice Fiscale non corretto.');";
                        TempData["alertMessage"] = "Codice Fiscale non corretto.";
                        Session["Codicefisc"] = codice.Codicefisc;
                        TempData["Codicefisc"] = "entro";
                        ViewBag.Message = TempData["alertMessage"].ToString();
                        return Redirect("InsertEntratel");
                    }

                }
                else
                {
                    //TempData["alertMessage"] = "alert('È obbligatorio inserire un codice Fiscale.');";
                    TempData["alertMessage"] = "È obbligatorio inserire un codice Fiscale.";
                    return Redirect("InsertEntratel");
                }


            }
            catch (Exception e)
            {

                if (codice.Codicefisc != null)
                {
                    Session["Codicefisc"] = codice.Codicefisc;
                    TempData["Codicefisc"] = "entro";
                }

                string message = "Errore nella funzione SaveEntratel SerialNum : " + CertificatoSelezionato.Serialnumber + " - codicefiscale : " + codice.Codicefisc + " - CN: " + CertificatoSelezionato.Commonname;
                string messageErrore = "Descrizione errore : " + e.ToString();
                LogDelegati Log = new LogDelegati();
                Log.SaveLogPinProvisioning(codice.Codicefisc, operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, this.Request.UserHostAddress);
                //TempData["alertMessage"] = "alert('Servizio momentaneamente non disponibile.');";
                TempData["alertMessage"] = "Servizio momentaneamente non disponibile.";

                return Redirect("InsertEntratel");

            }
            finally
            {

            }


        }



        public string GetEncCookieContent(string cookieName)
        {
            try
            {
                string secretKey = ConfigurationManager.AppSettings["SecretKey"];
                HttpCookie cookie = this.Request.Cookies[cookieName];

                return cookie == null ? string.Empty : TestMenuEnteMvc.Class.CryptDecrypt.DecryptStringAes(cookie.Value, secretKey);
            }
            catch (Exception ex)
            {

                throw new Exception("GetEncCookieContent: " + ex.ToString());
            }
        }

        public void SetEncCookieContent(string cookieName, string cookieContent)
        {
            string secretKey = ConfigurationManager.AppSettings["SecretKey"];
            string cryptContent = CryptDecrypt.EncryptStringAes(cookieContent, secretKey);

            this.Response.Cookies.Add(new HttpCookie(cookieName, cryptContent));
        }

        //Deve essere nserito in tutti i controller che chiamano il popup
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

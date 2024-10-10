using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PINProvUtilita.Controllers
{
    public class DecifraCodiceFiscaleController : Controller
    {
        //
        // GET: /DecifraCodiceFiscale/

        public ActionResult Index()
        {
            ViewData["alertMessage"] = null;
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


            string paramM = System.Configuration.ConfigurationManager.AppSettings["ParamDecifraCodiceFiscale"];

            if (caricaMenu)
                Session["listaCertificati"] = PINProvUtilita.Controllers.utility.CaricaMenu(matricolaoperatore, paramM);

            return View();
        }
        [HttpPost]
        public ActionResult Index(String codicefiscale)
        {
            string result = "";
            ViewData["alertMessage"] = null;
            result = utility.DecifraCodiceFiscale(codicefiscale).ToString();
            if (result == "ERRORE")
            {
                chiamataPopup("DecifraCodiceFiscale");
                ViewData["alertMessage"] = "Entro";
                ViewBag.Message = "Non è stato possibile decifrare il codice digitato";
            }
            else
            {
                Session["Codicefisc"] = utility.DecifraCodiceFiscale(codicefiscale).ToString();
            }
            return View();
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

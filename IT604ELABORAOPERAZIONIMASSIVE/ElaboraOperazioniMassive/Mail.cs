using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using ElaboraOperazioniMassive.Servizi;
using ElaboraOperazioniMassive.DAL;

namespace ElaboraOperazioniMassive
{
    class MAIL
    {
        private string _codiceFiscale;
        internal MAIL() { }

        internal MAIL(string CodiceFiscale)
        {
            _codiceFiscale = CodiceFiscale;
        }

        internal bool InvioMail(StringBuilder TestoMail, bool Errore)
        {
            bool ritorno = false;
            try
            {
                CredenzialiServizi.MAIL credenziali = CredenzialiServizi.CredenzialiMAIL();
                StringBuilder testoMessaggio = new StringBuilder();
                GestoreMAIL mail = new GestoreMAIL();
                mail.URLWS = ConfigurationManager.AppSettings["WSICONAMAIL"];
                mail.From = ConfigurationManager.AppSettings["WSICONAMAILMittente"];
                if (Errore)
                {
                    mail.Subject = "Errore nell'esecuzione del batch ElaboraOperazioniMassive.exe";
                    mail.To = ConfigurationManager.AppSettings["DestinatariMailErrore"];
                }
                else
                {
                    mail.Subject = "Esito elaborazione batch ElaboraOperazioniMassive.exe";
                    mail.To = ConfigurationManager.AppSettings["DestinatariMailElaborazione"];
                }

                testoMessaggio.Append(TestoMail);
                mail.Body = testoMessaggio.ToString();
                mail.PEC = false;
                
                EsitoMail esitoMail = new EsitoMail();
                esitoMail = mail.Invio();
                AssegnazionePinDAL log = new AssegnazionePinDAL();

                if (esitoMail.Esito == "ND")
                {
                    log.insertLogDeceduti("ElaboraOperazioniMassive", 0, null, "Errore ElaboraConvenzioni.GestoreMAIL.InvioMail: " + " Manca Destinatario: " + mail.To + esitoMail.ErrDescription);
                    ritorno =  false;
                }
                if (esitoMail.Esito == "ER")
                ///// IM: in coda per Invio Email - ER: errore (comunicazione non inserita) - PK: KeyGest Duplicata
                {
                    log.insertLogDeceduti("ElaboraOperazioniMassive", 0, null, "Errore ElaboraConvenzioni.GestoreMAIL.InvioMail: " + " Errore di connessione: " + mail.To + esitoMail.ErrDescription);
                    ritorno =  false;
                }
                //throw new GestioneContattiException(esito.Esito + " " + esito.ErrDescription);
                if (esitoMail.Esito == "IM")
                {
                    log.insertLogDeceduti("ElaboraOperazioniMassive", 0, null, "ElaboraConvenzioni.GestoreMAIL.InvioMail: " + " Chiave Gestionale: " + esitoMail.Chiave + "--" + mail.To);
                    //Log.InsLog(dataElaborazione, "E", "Errore ElaboraConvenzioni.GestoreMAIL.InvioMail: " + " Chiave Gestionale: " + esitoMail.Chiave + "--" + mail.To);
                    ritorno = true;
                }
            }
            catch (Exception ex)
            {
                StringBuilder StringErrore = new StringBuilder();
                StringErrore.Append(ex.Message.ToString());
                throw new Exception(StringErrore.ToString());
            }

            return ritorno;
        }

    }
}

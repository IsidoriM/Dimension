using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElaboraEntratel.WSIcona2;
using System.Collections;
using System.Configuration;
using mycertificatebatch;

namespace ElaboraEntratel
{
    class ClientIcona
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
           (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public EsitoMail  inviaEmail(string codapp,
                                      string huname,
                                      string hpswd,  
                                      string[] destinatariTo,
                                      string[] destinatariCC,
                                      string mittente,
                                      string subject,
                                      string body
                                      
                                       ){

            GestoreMAIL mail = new GestoreMAIL();
            mail.From = ConfigurationManager.AppSettings["WSICONAMAILMittente"];
            mail.Subject = subject;
            string destinatario = string.Empty;
            for (int i = 0; i < destinatariTo.Length; i++)
            {
                
                destinatario = destinatario + destinatariTo[i];
                if(i < destinatariTo.Length - 1) { destinatario = destinatario + ";"; };
             }
            mail.To = destinatario;
            mail.Body = body;
            mail.PEC = false;
            //dataElaborazione = DateTime.Now;
            EsitoMail esitoMail = new EsitoMail();
            Certificato Log = new Certificato();
            Log.Esito = 2;
            esitoMail = mail.Invio();
            return esitoMail;
          

            
            /*
            InvioSoapClient client = new InvioSoapClient();
            LoginHeader header = new LoginHeader();
            InvioMail_INPUT_01 input = new InvioMail_INPUT_01();
            input.codiceApplicazione = codapp;
            header.UserName = huname;
            header.Password = hpswd;
            //header.AnyAttr="";
            List<Destinatari_01> destinatari = new List<Destinatari_01>();
            if (destinatariTo != null && destinatariTo.Length>0)
            {
                foreach (string destinatario in destinatariTo)
                {
                    Destinatari_01 dest1 = new Destinatari_01();
                    dest1.tipo = tipoDestinatari_01.to;
                    dest1.destinatario = destinatario;
                    destinatari.Add(dest1);
                }
            }

            if (destinatariCC != null && destinatariCC.Length > 0)
            {
                foreach (string destinatario in destinatariCC)
                {
                    Destinatari_01 dest1 = new Destinatari_01();
                    dest1.tipo = tipoDestinatari_01.to;
                    dest1.destinatario = destinatario;
                    destinatari.Add(dest1);
                }
            }

            */

            /*
            

            input.mittente=mittente;
            input.destinatari = destinatari.ToArray < Destinatari_01>();
            input.subject = subject;
            input.body = body;
            input.keygest = "ElaboraEntratel"+DateTime.Now;
            InvioMail_OUTPUT output=  client.InvioEmail_01(header, input);
            
            log.Info("########Esito Invio Email:"+output.CD_RC);
            log.Info("########Esito Invio Email:" + output.errorDescription);
            log.Info("########Esito Invio Email:" + output.dataInserimento);
            log.Info("########Esito Invio Email:" + output.protocollo);

            */


        }

        private static void insertLogEvent(Certificato log, object conn)
        {
            throw new NotImplementedException();
        }
    }
}

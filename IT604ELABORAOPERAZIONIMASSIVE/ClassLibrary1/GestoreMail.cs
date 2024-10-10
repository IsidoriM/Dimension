using System.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WSIcona2 = ElaboraOperazioniMassive.Servizi.WSIcona2;


namespace ElaboraOperazioniMassive.Servizi
{
    public class EsitoMail  // Ver 1.3
    {
        public String Chiave { get; set; }
        public String Esito { get; set; }
        public String ErrDescription { get; set; }
        public DateTime DataInvio { get; set; }
    }

    public class GestoreMAIL : Common, IDisposable
    {
        #region Attributi Globali

        //private Invio _InvioService = null;
        //private InvioMail_INPUT_01 _InputMail = null;
        //private Allegati[] _Allegati = null;

        private Boolean disposed = false;
        private String _From = String.Empty;

        private String _From_Alias = ConfigurationManager.AppSettings["WSICONAMAILAlias"];
        //private String _From_Alias;


        private String _To = String.Empty;
        private String _Cc = String.Empty;
        private String _Bcc = String.Empty;
        private String _Subject = String.Empty;
        private String _Body = String.Empty;
        private string chiaveGestionale = string.Empty;
        //public List<(string NomeAllegato, byte[] byteAllegato)> Allegato;
        public List<string> Allegato;
        byte[] byteAllegato;

        #endregion

        #region Propriet 

        public String From
        {
            get
            {
                return _From;
            }
            set
            {
                _From = value;
            }
        }
        public String From_Alias
        {
            get
            {
                return _From_Alias;
            }
            set
            {
                _From_Alias = value;
            }
        }

        public String To
        {
            get
            {
                return indirizzoMailComunicazioniTestColl(_To);
            }
            set
            {
                _To = value;
            }
        }

        public String Cc
        {
            get
            {
                return indirizzoMailComunicazioniTestColl(_Cc);
            }
            set
            {
                _Cc = value;
            }
        }

        public String Bcc
        {
            get
            {
                return indirizzoMailComunicazioniTestColl(_Bcc);
            }
            set
            {
                _Bcc = value;
            }
        }

        public String Subject
        {
            get
            {
                return _Subject;
            }
            set
            {
                _Subject = value;
            }
        }

        public String Body
        {
            get
            {
                return _Body;
            }
            set
            {
                _Body = value;
            }
        }

        public String Key
        {
            get
            {
                return chiaveGestionale;
            }
            set
            {
                chiaveGestionale = value;
            }
        }
        public bool PEC { get; set; }

        #endregion

        #region Costruttore

        private string indirizzoMailComunicazioniTestColl(string mail)
        {
            return mail;
        }

        public GestoreMAIL()
        {

        }

        public GestoreMAIL(string URL, String CodiceApplicazione, String UserName, String Password)
        {

        }

        #endregion

        #region Metodi Pubblici

       public EsitoMail Invio()
        {
            EsitoMail Esito = new EsitoMail();

            ///// Ver 2.4 WSIcona2 Invio Mail

            WSIcona2.WSIcona20Client wsIcona20Client = new WSIcona2.WSIcona20Client();
            WSIcona2.PUC_InvioComunicazioneRequest invioEmailExtRequest = new WSIcona2.PUC_InvioComunicazioneRequest(); // .InvioEmailExtRequest();

            SetChiaveGestionale();
            invioEmailExtRequest.chiaveGestionale = chiaveGestionale; //"Test.ABDCEFXXX1234567803";
            invioEmailExtRequest.mittente = GetFrom(); // _From;
            invioEmailExtRequest.subject = _Subject;
            invioEmailExtRequest.body = _Body;
            invioEmailExtRequest.sede = ConfigurationManager.AppSettings["WSICONAMAILMittenteSede"];
            //invioEmailExtRequest.checkEsitoInvio = false;
            invioEmailExtRequest.isPEC = this.PEC;
            WSIcona2.PUC_TipoComunicazione tipoComunicazione = new WSIcona2.PUC_TipoComunicazione();

            tipoComunicazione.lavorazione = ConfigurationManager.AppSettings["WSICONAMAILTipoComunicazioneLavorazione"];
            invioEmailExtRequest.tipoComunicazione = tipoComunicazione;

            //Destinatari
            invioEmailExtRequest.listaDestinatari = GetDestinatariMail(invioEmailExtRequest);
            if (invioEmailExtRequest.listaDestinatari != null && invioEmailExtRequest.listaDestinatari.Length > 0)
            {

                //Allegati
                invioEmailExtRequest.allegatiPresenti = false;
                /*
                invioEmailExtRequest.listaAllegati = GetAllegatiMail(invioEmailExtRequest);
                if (invioEmailExtRequest.listaAllegati != null && invioEmailExtRequest.listaAllegati.Length > 0)
                    invioEmailExtRequest.allegatiPresenti = true;
                    
                else
                    invioEmailExtRequest.allegatiPresenti = false;
                */
                WSIcona2.PUC_InvioComunicazioneResponse invioEmailExtResponse = wsIcona20Client.PUC_InvioComunicazione(invioEmailExtRequest);
                Esito.Chiave = chiaveGestionale;
                Esito.Esito = invioEmailExtResponse.cdEsito.ToString();
                // Esito.DataInvio = output.dataInserimento;
                Esito.DataInvio = invioEmailExtResponse.dataInserimento;
                Esito.ErrDescription = invioEmailExtResponse.dsErorre;
            }
            else
            {
                ////// Caso in cui non viene inviata l'email perch  manca il destinatario.
                Esito.Chiave = chiaveGestionale;
                Esito.Esito = "ND";
                // Esito.DataInvio = output.dataInserimento;
                Esito.DataInvio = DateTime.Now;
                Esito.ErrDescription = "Error: Destinatari assenti";
            }

            return Esito;

        }


        private void SetChiaveGestionale()
        {
            Guid g = Guid.NewGuid();
            chiaveGestionale = g.ToString();
        }




        private WSIcona2.PUC_MittenteComunicazione GetFrom()
        {
            WSIcona2.PUC_MittenteComunicazione f = new WSIcona2.PUC_MittenteComunicazione();

            f.address = _From;
            if (!string.IsNullOrEmpty(_From_Alias))
            {
                f.descrizione = _From_Alias;
            }

            return f;
        }

        private WSIcona2.PUC_AllegatoInvio[] GetAllegatiMail(WSIcona2.PUC_InvioComunicazioneRequest imr)
        {
            byte[] byteAllegato = null;
            if (Allegato != null)
            {
                imr.listaAllegati = new WSIcona2.PUC_AllegatoInvio[Allegato.Count];

                int i = 0;
                foreach (var item in Allegato)
                {
                    if (byteAllegato.Length > 0)
                    {
                        WSIcona2.PUC_AllegatoInvio allegato = new WSIcona2.PUC_AllegatoInvio();
                        //allegato.nomeFile = item. .NomeAllegato;
                        allegato.nomeFile = "";
                        allegato.allegato = byteAllegato;
                        imr.listaAllegati[i] = allegato;
                        i = i + 1;
                    }
                }

            }

            return imr.listaAllegati;
        }

        private WSIcona2.PUC_DestinatarioInvioComunicazione[] GetDestinatariMail(WSIcona2.PUC_InvioComunicazioneRequest imr)
        {
            int nDest = 0;

            string[] aTo = string.IsNullOrEmpty(_To) ? null : _To.Replace(",", ";").Split(';');
            string[] aCc = string.IsNullOrEmpty(_Cc) ? null : _Cc.Replace(",", ";").Split(';');
            string[] aBcc = string.IsNullOrEmpty(_Bcc) ? null : _Bcc.Replace(",", ";").Split(';');

            //    nDest = (aTo is null ? 0 : aTo.Length) + (aCc is null ? 0 : aCc.Length) + (aBcc is null ? 0 : aBcc.Length);
            nDest = 1;
            imr.listaDestinatari = new WSIcona2.PUC_DestinatarioInvioComunicazione[nDest];
            int i = 0;

            if (aTo != null)
            {
                foreach (string item in aTo)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        WSIcona2.PUC_DestinatarioInvioComunicazione destinatario = new WSIcona2.PUC_DestinatarioInvioComunicazione();
                        destinatario.tipoSoggetto = WSIcona2.PUC_EnTipoSoggetto.NON_DISPONIBILE;
                        destinatario.tipo = WSIcona2.EnTipoDestinatario.TO;
                        destinatario.indirizzoEmail = item.Trim();
                        imr.listaDestinatari[i] = destinatario;
                        i = i + 1;
                    }
                }
            }
            if (aCc != null)
            {
                foreach (string item in aCc)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        WSIcona2.PUC_DestinatarioInvioComunicazione destinatario = new WSIcona2.PUC_DestinatarioInvioComunicazione();
                        destinatario.tipoSoggetto = WSIcona2.PUC_EnTipoSoggetto.NON_DISPONIBILE;
                        destinatario.tipo = WSIcona2.EnTipoDestinatario.CC;
                        destinatario.indirizzoEmail = item.Trim();
                        imr.listaDestinatari[i] = destinatario;
                        i = i + 1;
                    }

                }
            }
            if (aBcc != null)
            {
                foreach (string item in aBcc)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        WSIcona2.PUC_DestinatarioInvioComunicazione destinatario = new WSIcona2.PUC_DestinatarioInvioComunicazione();
                        destinatario.tipoSoggetto = WSIcona2.PUC_EnTipoSoggetto.NON_DISPONIBILE;
                        destinatario.tipo = WSIcona2.EnTipoDestinatario.BCC;
                        destinatario.indirizzoEmail = item.Trim();
                        imr.listaDestinatari[i] = destinatario;
                        i = i + 1;
                    }
                }
            }

            return imr.listaDestinatari;
        }

        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (!this.disposed)
            {

            }
            disposed = true;
        }
        #endregion
    }
}


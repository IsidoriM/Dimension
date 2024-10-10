using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PINProvUtilita.Models
{
    [Serializable]
    public class WhiteListNumContatto
    {
        String contatto;

        [Display(Name = "Contatto: ")]
        public String Contatto
        {
            get { return contatto; }
            set { contatto = value; }
        }

        String tipoContatto;

        [Display(Name = "Tipo: ")]
        public String TipoContatto
        {
            get { return tipoContatto; }
            set { tipoContatto = value; }
        }

        int limite;

        [Display(Name = "Limite: ")]
        public int Limite
        {
            get { return limite; }
            set { limite = value; }
        }

        String codiceFiscaleSegnalazione;

        [Display(Name = "Segnalato da: ")]
        public String CodiceFiscaleSegnalazione
        {
            get { return codiceFiscaleSegnalazione; }
            set { codiceFiscaleSegnalazione = value; }
        }


        String note;

        [Display(Name = "Note: ")]
        public String Note
        {
            get { return note; }
            set { note = value; }
        }

        String dataInserimento;

        [Display(Name = "Inserito il: ")]
        public String DataInserimento
        {
            get { return dataInserimento; }
            set { dataInserimento = value; }
        }


    }
}
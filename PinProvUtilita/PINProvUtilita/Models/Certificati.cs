using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PINProvUtilita.Models
{
    [Serializable]
    public class Certificati
    {
        private String serialnumber;

        public String Serialnumber
        {
            get { return serialnumber; }
            set { serialnumber = value; }
        }



        String codiceFiscale;

        [Display(Name = "Codice Fiscale persona fisica o giuridica: ")]      
        public String CodiceFiscale
        {
            get { return codiceFiscale; }
            set { codiceFiscale = value; }
        }

        String commonname;

        public String Commonname
        {
            get { return commonname; }
            set { commonname = value; }
        }


        private String tipo;

        public String Tipo
        {
            get { return tipo; }
            set { tipo = value; }
        }

        private String dataemissione;

        public String Dataemissione
        {
            get { return dataemissione; }
            set { dataemissione = value; }
        }

        private String datascadenza;

        public String Datascadenza
        {
            get { return datascadenza; }
            set { datascadenza = value; }
        }

        public string SelectedAnswer { set; get; }

        private String codicefisc;

        [Display(Name = "Codice Fiscale")]
        public String Codicefisc
        {
            get { return codicefisc; }
            set { codicefisc = value; }
        }
    }
}
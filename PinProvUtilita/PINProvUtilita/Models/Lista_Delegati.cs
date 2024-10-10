using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;




namespace PINProvUtilita.Models
{
    [Serializable]
    public class Lista_Delegati
    {
        String codiceFiscale;


        public String CodiceFiscale
        {
            get { return codiceFiscale; }
            set { codiceFiscale = value; }
        }

        String tipo;

        public String Tipo
        {
            get { return tipo; }
            set { tipo = value; }
        }

        String commonname;

        public String Commonname
        {
            get { return commonname; }
            set { commonname = value; }
        }

        String dataemissione;

        public String Dataemissione
        {
            get { return dataemissione; }
            set { dataemissione = value; }
        }

        String datascadenza;

        public String Datascadenza
        {
            get { return datascadenza; }
            set { datascadenza = value; }
        }



    }
}
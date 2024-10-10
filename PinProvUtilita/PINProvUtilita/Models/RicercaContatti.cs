using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PINProvUtilita.Models
{
    [Serializable]
    public class RicercaContatti
    {

        String selectContatto;

        public String SelectContatto
        {
            get { return selectContatto; }
            set { selectContatto = value; }
        }




        String email;

        [Display(Name = "Indirizzo email: ")]
        public String Email
        {
            get { return email; }
            set { email = value; }
        }

        String pec;

        [Display(Name = "Indirizzo PEC: ")]
        public String Pec
        {
            get { return pec; }
            set { pec = value; }
        }

        String cellulare;

        [Display(Name = "Cellulare: ")]
        public String Cellulare
        {
            get { return cellulare; }
            set { cellulare = value; }
        }

        string certificato;

        [Display(Name = "Certificato: ")]
        public string Certificato
        {
            get { return certificato; }
            set { certificato = value; }
        }

    }
}
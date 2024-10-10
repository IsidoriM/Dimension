using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PINProvUtilita.Models
{
    public class DecifraCodiceFiscale
    {

        String codiceFiscale;

        [Display(Name = "Codice Fiscale da decifrare: ")]
        public String CodiceFiscale
        {
            get { return codiceFiscale; }
            set { codiceFiscale = value; }
        }

        String codiceFiscaleDecifrato;

        [Display(Name = "")]
        public String CodiceFiscaleDecifrato
        {
            get { return codiceFiscaleDecifrato; }
            set { codiceFiscaleDecifrato = value; }
        }
    }
}

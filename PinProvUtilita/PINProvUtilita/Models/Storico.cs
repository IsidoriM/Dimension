using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PINProvUtilita.Models
{
    [Serializable]
    public class Storico
    {

        String codicefiscale;

        [Display(Name = "Codice Fiscale: ")]
        public String CodiceFiscale
        {
            get { return codicefiscale; }
            set { codicefiscale = value; }
        }


    }
}
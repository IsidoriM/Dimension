using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestMenuEnteMvc.Class
{
    [Serializable]
    public class Regioni
    {
        private string codice;

        public string Codice
        {
            get { return codice; }
            set { codice = value; }
        }


        private string regione;

        public string Regione
        {
            get { return regione; }
            set { regione = value; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PINProvUtilita.Models
{
    public class messaggi
    {
        String msgAvviso;

        public String MsgAvviso
        {
            get { return msgAvviso; }
            set { msgAvviso = value; }
        }

        String msgErrore;

        public String MsgErrore
        {
            get { return msgErrore; }
            set { msgErrore = value; }
        }

    }
}
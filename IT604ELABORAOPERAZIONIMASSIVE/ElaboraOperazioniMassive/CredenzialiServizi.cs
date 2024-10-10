using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ElaboraOperazioniMassive
{
    class CredenzialiServizi
    {
        internal struct MAIL
        {
            public string CodiceApplicazione;
            public string UserName;
            public string Password;

        }

        internal static MAIL CredenzialiMAIL()
        {
            MAIL ritorno = new MAIL();

            ritorno.CodiceApplicazione = ConfigurationManager.AppSettings["WSICONAMAILCodiceApplicazione"];
            ritorno.UserName = ConfigurationManager.AppSettings["WSICONAMAILUserName"];
            ritorno.Password = ConfigurationManager.AppSettings["WSICONAMAILPassword"];

            return ritorno;
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PINProvUtilita.Controllers
{
    public enum LogEvents
    {
        //CLOG
        InserimentoClog = 13019,            //: Inserimento delega su certificato Entratel
        CancellazioneClog = 13022,          //: Cancellazione delega su certificato Entratel

        //PIN Provisioning
        InserimentoPinProvisioning = 48,   //: Inserimento delega su certificato Entratel
        CancellazionePinProvisioning = 49, //: Cancellazione delega su certificato Entratel
        Errore = 22
    }
}
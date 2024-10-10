using PinProvEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtenteDal;

namespace PinProvBLL
{
    public class UtenteContattiBLL
    {
        public UtenteStorico getUtenteContattiCont(String codiceFiscale)
        {
            UtenteStorico utente = new UtenteStorico();
            UtenteContattiDal  uDal = new UtenteContattiDal();
            utente = uDal.getUtenteContatti(codiceFiscale);
            return utente;
        }

        public List<UtenteContatti> getUtenteContattiStorico(String codiceFiscale)
        {
            List<UtenteContatti> utente = new List<UtenteContatti>();
            UtenteContattiDal uDal = new UtenteContattiDal();
            utente = uDal.getUtenteContattiStorico(codiceFiscale);
            return utente;
        }

        
    }
}

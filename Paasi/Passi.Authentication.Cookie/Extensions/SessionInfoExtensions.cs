using Passi.Core.Domain.Const;
using Passi.Core.Extensions;

namespace Passi.Core.Domain.Entities.Info
{
    static class SessionInfoExtensions
    {
        public static string Serialize(this SessionInfo item)
        {
            var profileData = new List<string>
            {
                item.UserId,                    //0: id
                item.FiscalCode,                //1: codice fiscale
                item.InstitutionCode,           //2: codice dell'ente (es. 009)
                item.Name,                      //3: nome
                item.Surname,                   //4: cognome
                item.Gender,                    //5: sesso
                item.Email,                     //6: email
                string.Empty,                   //7: fax
                item.Phone,                     //8: telefono
                item.InstitutionDescription,    //9: descrizione dell'ente (es. E.p.a.c.a)
                item.ProfileTypeId.ToString(),  //10: id del tipo di profilo utente (es. 3 = Cittadino)
                item.OfficeCode,                //11: codice dell'ufficio (es. LP01) 
                item.UserClass,                 //12: classe utente
                item.Number,                    //13: matricola
                item.InstitutionFiscalCode,     //14: codice fiscale o partita iva dell'ente
                item.AuthenticationType,        //15: tipo di autenticazione dell'utente
                item.LastAccess.ToString(),     //16: ultimo accesso dell'utente (in stringa)
                item.LastUpdated.ToMilliseconds().ToString(),           //17: ultimo aggiornamento della sessione (in milisecondi)
                item.SessionTimeout.TotalSeconds.ToString(),            //18: durata della sessione (in secondi)
                string.Empty,                                           //19: codice del profilo necessario all'accesso (srcPortal), Non mappato
                item.LoggedIn.ToMilliseconds().ToString(),              //20: ora di accesso (in ms)
                item.SessionMaximumTime.TotalSeconds.ToString(),        //21: tempo massimo di sessione (in s)
                item.LastPinUpdate.ToMilliseconds().ToString(),         //22: data di ultimo aggiornamento del pin (in ms)
                item.IsPinUnified.ToStringLower(),                      //23: unificazione pin (bool)
                item.InformationCampaign.ToStringLower(),               //24: campagna informativa (bool)
                item.SessionId,                                         //25: id di sessione
                item.LastSessionUpdate.ToMilliseconds().ToString(),     //26: data di ultimo aggiornamento della sessione (in ms)
                item.SessionMaximumCachingTime.TotalSeconds.ToString(), //27: tempo massimo di chaching della sessione (in s)
                item.HasSessionFlag.ToStringLower(),                    //28: session flag message (bool)
                item.MultipleProfile.ToInt().ToString(),                //29: se esiste un profilo multiplo (bool, ma con 0 e 1)
                item.AnonymousId,                                       //30: id anonimo
                item.IsInfoPrivacyAccepted.ToStringLower(),             //31: accettazione info (bool)
                item.PEC,                                               //32: pec di session
                item.PECVerificationStatus.ToVerificationString(),      //33: stato della pec (enum)
                item.Mobile,                                            //34: cellulare di sessione
                item.BirthDate.ToString("dd/MM/yyyy"),                  //35: data di nascita (dd/MM/yyyy)
                item.BirthProvince,                                     //36: provincia
                item.BirthPlaceCode,                                    //37: città di nascita
                item.DelegateUserId,                                    //38: id del delegato
                item.IsFromLogin.ToStringLower(),                       //39: se proviene dalla login (bool)
                string.Empty,                                           //40: ignoto
            };

            return string.Join(Keys.Separator, profileData);
        }
    }
}

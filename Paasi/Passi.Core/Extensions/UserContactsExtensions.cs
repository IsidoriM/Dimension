using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;

namespace Passi.Core.Extensions
{
    static class UserContactsExtensions
    {
        public static UserContacts With(this UserContacts userContacts, UserInfo userInfo, SessionInfo sessionInfo, bool isObfuscated)
        {
            userContacts.Email = isObfuscated ? userInfo.Email.ObfuscateEmail() : userInfo.Email;
            userContacts.Pec = isObfuscated ? userInfo.PEC.ObfuscateEmail() : userInfo.PEC;
            userContacts.Mobile = isObfuscated ? userInfo.Mobile.ObfuscatePhoneNumber() : userInfo.Mobile;
            userContacts.Phone = isObfuscated ? userInfo.Phone.ObfuscatePhoneNumber() : userInfo.Phone;

            string html_footer = "I contatti personali possono essere aggiornati in ogni momento dall'utente da \"Entra in MyINPS > Anagrafica\"";
            string html_title = $"Contatti personali di {userInfo.UserId}";

            bool requestingLoggedUserContacts = userInfo.UserId == sessionInfo.FiscalCode;
            //Se sto prendendo i miei contatti personali
            if (requestingLoggedUserContacts)
            {
                html_title = "I tuoi contatti personali";
                html_footer = "Puoi aggiornare in ogni momento i tuoi contatti personali dal percorso \"Entra in MyINPS > Anagrafica\"";
                userContacts.Email = userInfo.Email;
                userContacts.Pec = userInfo.PEC;
                userContacts.Mobile = userInfo.Mobile;
                userContacts.Phone = userInfo.Phone;
            }

            userContacts.Title = html_title;
            userContacts.Info = html_footer;

            if (string.IsNullOrWhiteSpace(userInfo.FiscalCode))
            {
                throw new ContactsException(Outcomes.Two,
                        userInfo.FiscalCode,
                        "L'utente non ha contatti personali registrati.");
            }

            if (!sessionInfo.IsInfoPrivacyAccepted)
            {
                throw new ContactsException(Outcomes.One,
                        userInfo.FiscalCode,
                        "L'utente non ha preso visione dell'informativa privacy INPS sulle modalità di utilizzo dei contatti.");
            }

            return userContacts;
        }
    }
}

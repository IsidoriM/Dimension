using Microsoft.AspNetCore.Http;
using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Extensions;
using System.Globalization;
using System.Text;
using static Passi.Authentication.Cookie.Const.Cookies;
using static Passi.Authentication.Cookie.Const.Positions;
using static Passi.Authentication.Cookie.Const.VsuCookieProperties;

namespace Passi.Authentication.Cookie.Repository
{
    internal class ContactCenterInfoRepository : IInfoRepository<ContactCenterInfo>
    {
        private readonly IHttpContextAccessor accessor;

        public ContactCenterInfoRepository(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public Task<ContactCenterInfo> RetrieveAsync()
        {
            var CCI = new ContactCenterInfo();
            var context = accessor.HttpContext;
            var cookieSCC = context?.Request.Cookies[ContactCenterSCC];
            var cookieVSU = context?.Request.Cookies[ContactCenterVSU];

            if (cookieSCC == null && context != null)
            {
                string textCookies = context.Request.Headers["Cookie"].ToString();
                if (!string.IsNullOrWhiteSpace(textCookies))
                {
                    string[] d = textCookies.Replace(";", ",").Split(",");
                    // vs: questo branch non è testabile perché l'anomalia che ha portato alla sua creazione capita solo in ambiente remoto
                    cookieSCC = d.FirstOrDefault(f => f.TrimStart().StartsWith(ContactCenterSCC))?.Split("=")?.LastOrDefault();
                }
            }

            if (cookieSCC == null)
            {
                throw new NotFoundException($"Cookie {ContactCenterSCC} not found");
            }
            if (cookieVSU == null)
            {
                throw new NotFoundException($"Cookie {ContactCenterVSU} not found");
            }

            cookieSCC = cookieSCC.Trim();
            var pieces = cookieSCC.Split(Keys.OptionalSeparator);

            Dictionary<string, string> dictionary = cookieVSU.Trim()
                .Split(Keys.OptionalSeparator)
                .Select(part => part.Split('='))
                .Where(part => part.Length == 2)
                .ToDictionary(sp => sp[0], sp => sp[1]);

            var dataNascitaStr = dictionary[BirthDate].GetStringNoSpecialChars();
            if (!DateTime.TryParse(dataNascitaStr, new CultureInfo("it"), DateTimeStyles.None, out DateTime dataNascita))
            {
                DateTime.TryParse(dataNascitaStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out dataNascita);
            }

            CCI.OperatorId = pieces.GetString(UTENZA_OPERATORE_CC);
            CCI.OperatorUserClass = pieces.GetString(CLASSE_UTENTE_OPERATORE_CC);
            CCI.UserId = (!string.IsNullOrWhiteSpace(pieces.GetString(ID_UTENTE_CC))) ? pieces.GetString(ID_UTENTE_CC) : dictionary[FiscalCode].GetStringNoSpecialChars();
            CCI.FiscalCode = CCI.UserId;
            CCI.BirthDate = dataNascita;
            CCI.BirthPlaceCode = dictionary.ContainsKey(BirthPlaceCode) ? dictionary[BirthPlaceCode].GetStringNoSpecialChars() : string.Empty;
            CCI.BirthProvince = dictionary.ContainsKey(BirthProvince) ? dictionary[BirthProvince].GetStringNoSpecialChars() : string.Empty;
            CCI.Email = dictionary[Email].GetStringNoSpecialChars();
            CCI.Phone = dictionary.ContainsKey(Phone) ? dictionary[Phone].GetStringNoSpecialChars() : string.Empty;
            CCI.Mobile = dictionary.ContainsKey(Mobile) ? dictionary[Mobile].GetStringNoSpecialChars() : string.Empty;
            CCI.Name = dictionary[Name].GetStringNoSpecialChars();
            CCI.Surname = dictionary[Surname].GetStringNoSpecialChars();
            CCI.PEC = dictionary.ContainsKey(PEC) ? dictionary[PEC].GetStringNoSpecialChars() : string.Empty;
            CCI.Gender = dictionary[Const.VsuCookieProperties.Gender].GetStringNoSpecialChars();

            return Task.FromResult(CCI);
        }

        public Task<ContactCenterInfo> UpdateAsync(ContactCenterInfo item)
        {
            throw new NotImplementedException();
        }
    }
}

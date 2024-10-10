using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;

namespace Passi.Authentication.Cookie.Extensions
{
    static class ContactCenterExtensions
    {
        public static (string Scc, string Vsu) Serialize(this ContactCenterInfo contactCenterInfo)
        {
            var sccList = new List<string>
            {
                contactCenterInfo.OperatorId,
                contactCenterInfo.OperatorUserClass,
                contactCenterInfo.UserId,
                $"{contactCenterInfo.Name} {contactCenterInfo.Surname}".Trim()
            };

            var vsuDic = new Dictionary<string, string>
            {
                {Const.VsuCookieProperties.Name, contactCenterInfo.Name },
                {Const.VsuCookieProperties.Surname, contactCenterInfo.Surname },
                {Const.VsuCookieProperties.BirthDate, contactCenterInfo.BirthDate.ToString("dd/MM/yyyy") },
                {Const.VsuCookieProperties.BirthPlaceCode, contactCenterInfo.BirthPlaceCode },
                {Const.VsuCookieProperties.BirthProvince, contactCenterInfo.BirthProvince },
                {Const.VsuCookieProperties.Email, contactCenterInfo.Email },
                {Const.VsuCookieProperties.PEC, contactCenterInfo.PEC },
                {Const.VsuCookieProperties.FiscalCode, contactCenterInfo.FiscalCode },
                {Const.VsuCookieProperties.Mobile, contactCenterInfo.Mobile },
                {Const.VsuCookieProperties.Phone, contactCenterInfo.Phone },
                {Const.VsuCookieProperties.PIN, contactCenterInfo.Pin },
                {Const.VsuCookieProperties.Gender, contactCenterInfo.Gender },
            };

            string scc = string.Join(Keys.OptionalSeparator, sccList);
            string vsu = string.Join(Keys.OptionalSeparator, vsuDic.Select(s => $"{s.Key}={s.Value}"));
            return (scc, vsu);
        }
    }
}

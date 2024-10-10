using Microsoft.AspNetCore.Http;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Extensions;
using System.Net;
using System.Text;
using static Passi.Authentication.Cookie.Const.Cookies;
using static Passi.Authentication.Cookie.Const.Positions;

namespace Passi.Authentication.Cookie.Repository
{
    class SessionInfoRepository : IInfoRepository<SessionInfo>
    {
        private readonly IHttpContextAccessor accessor;
        private readonly IDataCypherService dataCypher;
        private readonly IInfoRepository<UserInfo> userInfoRepository;

        public SessionInfoRepository(IHttpContextAccessor accessor,
            IDataCypherService dataCypher,
            IInfoRepository<UserInfo> userInfoRepository)
        {
            this.accessor = accessor;
            this.dataCypher = dataCypher;
            this.userInfoRepository = userInfoRepository;
        }

        public async Task<SessionInfo> RetrieveAsync()
        {
            SessionInfo info = new();
            var context = accessor.HttpContext;
            var cookie = context?.Request.Cookies[Session];
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                cookie = dataCypher.Decrypt(cookie, Crypto.KCA);
                var pieces = cookie.Split(Keys.Separator);

                if (pieces.Length >= MIN_LENGTH)
                {
                    // viene impostato per il log l'utente di sessione, il relativo profilo e l'ufficio
                    var userId = pieces.GetString(ID_UTENTE);
                    UserInfo userInfo = await userInfoRepository.RetrieveAsync();
                    info.UserId = userInfo.UserId;
                    info.FiscalCode = userInfo.FiscalCode;
                    info.Name = userInfo.Name;
                    info.Surname = userInfo.Surname;
                    info.Gender = userInfo.Gender;
                    info.Email = userInfo.Email;
                    info.Phone = userInfo.Phone;
                    info.BirthPlaceCode = userInfo.BirthPlaceCode;
                    info.BirthProvince = userInfo.BirthProvince;
                    info.BirthDate = userInfo.BirthDate;
                    info.Mobile = userInfo.Mobile;
                    info.PEC = userInfo.PEC;
                    info.OfficeCode = userInfo.OfficeCode;

                    if (!userId.Contains(':'))
                    {
                        // Utente non Federato
                        info.InstitutionCode = pieces.GetString(COD_ENTE);
                        info.InstitutionDescription = pieces.GetString(DESC_ENTE);
                        info.ProfileTypeId = pieces.GetInt(TIPO_UTENTE);
                        info.UserClass = pieces.GetString(ID_CLASSE_UTENTE);
                        info.Number = pieces.GetString(MATRICOLA);
                        info.InstitutionFiscalCode = pieces.GetString(COD_FIS_ENTE);
                        info.AuthenticationType = pieces.GetString(AUTHENTICATION_TYPE);
                        info.LastAccess = pieces.GetString(LAST_ACCESS).ToDatetime();
                        info.LastUpdated = pieces.GetString(SESSION_LAST_UPDATE).ToDatetime();
                        info.SessionTimeout = pieces.GetString(SESSION_TIMEOUT).ToTimespan();
                        info.LoggedIn = pieces.GetString(LOGIN_TIME).ToDatetime();
                        info.SessionMaximumTime = pieces.GetString(MAX_SESSION_TIME).ToTimespan();
                        info.LastPinUpdate = pieces.GetString(SESSION_PINLAST_UPDATE).ToDatetime();
                        info.IsPinUnified = pieces.GetBool(UNIFICAZIONE_PIN);
                        info.InformationCampaign = pieces.GetBool(CAMPAGNA_INFORMATIVA);
                        info.SessionId = pieces.GetString(SESSION_ID);
                        info.LastSessionUpdate = pieces.GetString(SESSION_ID_UPDATE).ToDatetime();
                        info.SessionMaximumCachingTime = pieces.GetString(SESSION_ID_CACHINGTIME).ToTimespan();
                        info.HasSessionFlag = pieces.GetBool(SESSION_FLAGMSG);
                        info.MultipleProfile = pieces.GetBool(SESSION_PROFILOMULTIPLO);
                        info.AnonymousId = pieces.GetString(SESSION_ID_ANONIMO);
                        info.DelegateUserId = pieces.GetString(SESSION_DELEGATO);
                        info.IsFromLogin = pieces.GetBool(SESSION_DALOGIN);
                        info.IsInfoPrivacyAccepted = pieces.GetBool(SESSION_INFOPRIVACY);
                        info.PECVerificationStatus = pieces.GetString(SESSION_STATOPEC).ToVerificationStatuses();
                    }

                    return info;
                }
                else
                {
                    throw new InvalidDataException("cookie sessione non coerente (lunghezza)");
                }
            }

            throw new NotFoundException();
        }

        public Task<SessionInfo> UpdateAsync(SessionInfo item)
        {
            item.LastUpdated = DateTime.UtcNow;

            var context = accessor.HttpContext;
            var cookie = context?.Request.Cookies[Session];
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                cookie = dataCypher.Decrypt(cookie, Crypto.KCA);
                
                var pieces = cookie.Split(Keys.Separator);
                pieces[SESSION_LAST_UPDATE] = item.LastUpdated.ToMilliseconds().ToString();

                cookie = string.Join(Keys.Separator, pieces);
                accessor.HttpContext?.AddCypheredCookie(Session, cookie, dataCypher);
            }
            return Task.FromResult(item);
        }

        
    }
}

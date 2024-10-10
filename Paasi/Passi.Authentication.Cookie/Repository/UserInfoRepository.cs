using Microsoft.AspNetCore.Http;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Extensions;
using System.Globalization;
using System.Text;
using static Passi.Authentication.Cookie.Const.Cookies;
using static Passi.Authentication.Cookie.Const.Positions;

namespace Passi.Authentication.Cookie.Repository
{
    class UserInfoRepository : IInfoRepository<UserInfo>
    {
        private readonly IHttpContextAccessor accessor;
        private readonly IDataCypherService dataCypher;

        public UserInfoRepository(IHttpContextAccessor accessor, IDataCypherService dataCypher)
        {
            this.accessor = accessor;
            this.dataCypher = dataCypher;
        }

        public Task<UserInfo> RetrieveAsync()
        {
            UserInfo ui = new();

            var context = accessor.HttpContext;
            var cookie = context?.Request.Cookies[Session];
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                cookie = dataCypher.Decrypt(cookie, Crypto.KCA);
                var sessionData = cookie.Split(Keys.Separator);

                if (sessionData.Length >= MIN_LENGTH)
                {
                    // viene impostato per il log l'utente di sessione, il relativo profilo e l'ufficio
                    var userId = sessionData.GetString(ID_UTENTE);

                    if (userId.Contains(':'))
                    {
                        // Utente Federato
                        userId = userId.Split(':')[1];
                        ui.UserId = userId;
                        ui.FiscalCode = userId;
                        ui.IsFederated = true;
                    }
                    else
                    {
                        var dataNascitaStr = sessionData[SESSION_DATANASC];
                        if (!DateTime.TryParse(dataNascitaStr, new CultureInfo("it"), DateTimeStyles.None, out DateTime dataNascita))
                        {
                            DateTime.TryParse(dataNascitaStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out dataNascita);
                        }

                        // Utente non Federato
                        ui.UserId = userId;
                        ui.FiscalCode = sessionData.GetString(COD_FIS);
                        ui.Name = sessionData.GetString(NOME);
                        ui.Surname = sessionData.GetString(COGNOME);
                        ui.Gender = sessionData.GetString(SESSO);
                        ui.BirthPlaceCode = sessionData.GetString(SESSION_COMUNENASC);
                        ui.BirthProvince = sessionData.GetString(SESSION_PROVNASC);
                        ui.BirthDate = dataNascita;
                        ui.OfficeCode = sessionData.GetString(COD_UFFICIO);
                        ui.IsFederated = false;

                        var isInfoPrivacyAccepted = sessionData.GetBool(SESSION_INFOPRIVACY);
                        if (!isInfoPrivacyAccepted)
                        {
                            ui.Email = string.Empty;
                            ui.PEC = string.Empty;
                            ui.Phone = string.Empty;
                            ui.Mobile = string.Empty;
                        }
                        else
                        {
                            ui.Email = sessionData.GetString(E_MAIL);
                            var pecVerificationStatus = sessionData.GetString(SESSION_STATOPEC).ToVerificationStatuses();
                            if (pecVerificationStatus == PecVerificationStatuses.ConfirmOrUpdate || pecVerificationStatus == PecVerificationStatuses.Validated)
                            {
                                ui.PEC = sessionData.GetString(SESSION_PEC);
                            }
                            ui.Phone = sessionData.GetString(TELEFONO);
                            ui.Mobile = sessionData.GetString(SESSION_CELLULARE);
                        }
                    }

                    return Task.FromResult(ui);
                }
                else
                {
                    throw new InvalidDataException("cookie sessione non coerente (lunghezza)");
                }
            }

            throw new NotFoundException();
        }

        public Task<UserInfo> UpdateAsync(UserInfo item)
        {
            throw new NotImplementedException();
        }
    }
}

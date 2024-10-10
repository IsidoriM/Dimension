using Microsoft.AspNetCore.Http;
using Passi.Authentication.Cookie.Const;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Extensions;
using System.Globalization;
using System.Text;
using static Passi.Authentication.Cookie.Const.Positions;


namespace Passi.Authentication.Cookie.Repository
{
    class ProfileInfoRepository : IInfoRepository<ProfileInfo>
    {
        private readonly IHttpContextAccessor accessor;
        private readonly IDataCypherService dataCypher;
        private readonly IUserRepository userRepository;

        public ProfileInfoRepository(IHttpContextAccessor accessor,
            IDataCypherService dataCypher,
            IUserRepository userRepository)
        {
            this.accessor = accessor;
            this.dataCypher = dataCypher;
            this.userRepository = userRepository;
        }

        public Task<ProfileInfo> RetrieveAsync()
        {
            var info = new ProfileInfo();

            var context = accessor.HttpContext;
            var cookie = context?.Request.Cookies[Cookies.Profile];
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                cookie = dataCypher.Decrypt(cookie, Crypto.KCA);
                var pieces = cookie.Split(Keys.Separator);

                if (pieces.Length >= 8)
                {
                    info.Id = pieces.GetString(PRF_COD_FIS);

                    if (info.Id.Contains(':'))
                    {
                        info.Id = info.Id.Split(':').FirstOrDefault() ?? string.Empty;
                    }

                    info.FiscalCode = pieces.GetString(PRF_COD_FIS);
                    info.InstitutionCode = pieces.GetString(PRF_COD_ENTE);
                    info.ProfileTypeId = pieces.GetInt(PRF_TIPO_UTENTE);
                    info.OfficeCode = pieces.GetString(PRF_COD_UFFICIO);
                    info.LastUpdate = pieces.GetString(PRF_PROFILE_LAST_UPDATE).ToDatetime();
                    info.Timeout = pieces.GetString(PRF_PROFILE_TIMEOUT).ToTimespan();

                    var from = pieces.GetString(PRF_INIZIO_FASCIA_ORARIA);
                    if (DateTime.TryParse(from, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime opening))
                    {
                        info.Opening = opening.ToUniversalTime();
                    }

                    var to = pieces.GetString(PRF_FINE_FASCIA_ORARIA);
                    if (DateTime.TryParse(to, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime closing))
                    {
                        info.Closing = closing.ToUniversalTime();
                    }

                    // Creare le applicazioni
                    // Salto i pimi 8 pezzi, che sono il numero di variabili lette in precedenza e sono le info di profilo
                    // Quelle successive sono i servizi abilitati
                    foreach (var item in pieces.Skip(8))
                    {
                        if (int.TryParse(item[1..^1], out int _id))
                        {
                            info.Services.Add(new Service()
                            {
                                RequiredAuthenticationType = item.ShortDescribe(),
                                HasConvention = item.Last() == '1',
                                Id = _id
                            });
                        }
                    }

                }
            }

            return Task.FromResult(info);

        }

        public async Task<ProfileInfo> UpdateAsync(ProfileInfo item)
        {
            if (item.HasProfile && !item.IsStillValid)
            {
                item.Services = await userRepository.ServicesAsync(item.Id, item.InstitutionCode);
                /// Generiamo la stringona del cookie
                /// Mantenere l'ordine!!!
                item.LastUpdate = DateTime.UtcNow;
                var cookie = item.Serialize();

                accessor.HttpContext?.AddCypheredCookie(Cookies.Profile, cookie, dataCypher);
            }
            return item;
        }
    }

}

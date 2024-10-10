using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Passi.Core.Application.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Extensions;
using System.Collections.Specialized;
using System.Web;

namespace Passi.Core.Services
{
    internal class PassiSecureService : IPassiSecureService
    {
        private readonly IDataCypherService dataCypherService;
        private readonly ConfigurationOptions configOptions;
        private readonly IHttpContextAccessor accessor;
        private readonly IInfoRepository<SessionInfo> sessionInfoRepo;
        private readonly IInfoRepository<ContactCenterInfo> contactCenterInfoRepo;

        public PassiSecureService(IDataCypherService dataCypherService,
            IInfoRepository<SessionInfo> sessionInfoRepo,
            IInfoRepository<ContactCenterInfo> contactCenterInfoRepo,
            IOptions<ConfigurationOptions> configOptions,
            IHttpContextAccessor accessor)
        {
            this.dataCypherService = dataCypherService;
            this.configOptions = configOptions.Value;
            this.accessor = accessor;
            this.sessionInfoRepo = sessionInfoRepo;
            this.contactCenterInfoRepo = contactCenterInfoRepo;
        }

        public Task<string> SecureAsync(string text)
        {
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(text);
            return SecureAsync(queryString);
        }

        public Task<string> SecureAsync(IDictionary<string, string> data)
        {
            var nameValueCollection = new NameValueCollection();

            foreach (var kvp in data)
            {
                if (kvp.Value != null)
                {
                    nameValueCollection.Add(kvp.Key.ToString(), kvp.Value.ToString());
                }
            }

            return SecureAsync(nameValueCollection);
        }

        private Task<string> SecureAsync(NameValueCollection data)
        {
            return Task.FromResult(dataCypherService.Secure(data));
        }

        public Task<IDictionary<string, string>> UnsecureAsync(string cryptedText)
        {
            IDictionary<string, string> result = new Dictionary<string, string>();
            var nameValues = dataCypherService.Unsecure(cryptedText);
            foreach (var name in nameValues.AllKeys)
            {
                result.Add(name!, nameValues[name]!);
            }
            return Task.FromResult(result);
        }

        public async Task<string> SessionTokenAsync()
        {
            if (accessor.HttpContext?.Request.Path.ToString().IsApi() == true)
            {
                return string.Empty;
            }

            try
            {
                var session = await sessionInfoRepo.RetrieveAsync();

                try
                {
                    //Devo verificare che non ci sia un Operatore CC che stia lavorando per un utente,
                    //in tal caso devo prendere le generalità dell'utente e non quelle dell'Operatore
                    var contactCenterInfo = await contactCenterInfoRepo.RetrieveAsync();
                    session.CheckContactCenterInfo(contactCenterInfo);
                }
                catch (NotFoundException)
                {
                    //Do nothing
                }

                var _request = accessor.HttpContext?.Request;

                /// Per le applicazioni standard, la returnUrl del servizio è esattamente la url che viene chiamata dall'utente
                /// Per le applicazioni API, la returnUrl è la url iniziale del servizio. Viene mantenuta nel SessionToken
                string path = $"{_request?.PathBase.ToString().ToLower()}/{_request?.Path.ToString().ToLower()}".Trim('/');

                var uribuilder = new UriBuilder()
                {
                    Scheme = Schema.Https,
                    Host = _request?.Host.Host,
                    Path = path,
                    Query = _request?.QueryString.ToString(),
                };

                var returnUrl = uribuilder.ToString();
                if (!string.IsNullOrWhiteSpace(configOptions.RedirectUrl))
                {
                    returnUrl = configOptions.RedirectUrl;
                }

                var token = new SessionToken
                {
                    SessionId = session.SessionId,
                    ServiceId = configOptions.ServiceId,
                    InstitutionCode = session.InstitutionCode,
                    UserId = session.UserId,
                    UserTypeId = session.ProfileTypeId,
                    OfficeCode = session.OfficeCode,
                    LoggedIn = session.LoggedIn,
                    ServiceUri = new Uri(returnUrl)
                };

                var data = token.Serialize();
                var cypheredData = dataCypherService.Crypt(data);
                return cypheredData;
            }
            catch (NotFoundException)
            {
                return string.Empty;
            }
            catch (InvalidDataException)
            {
                return string.Empty;
            }

        }

        public Task<bool> CheckParameterAsync(string parameter, int maxLength, string[]? exceptionValues = null)
        {
            List<string> blackList = new()
            {
                "|",
                "&",
                ";",
                "$",
                "%",
                "@",
                "'",
                "\"",
                @"\'",
                "\\\"",
                "<",
                ">",
                "(",
                ")",
                "+",
                "\n",
                "\r",
                ",",
                @"\",
            };

            if ((maxLength > 0) && (parameter.Length > maxLength))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(!blackList.Any(blackStr =>
                (
                    (exceptionValues != null && !exceptionValues.Contains(blackStr)) ||
                    exceptionValues == null)
                &&
                (
                    parameter.Contains(blackStr, StringComparison.InvariantCultureIgnoreCase) ||
                    parameter.Contains(HttpUtility.UrlEncode(blackStr), StringComparison.InvariantCultureIgnoreCase)
                )));
        }

    }
}

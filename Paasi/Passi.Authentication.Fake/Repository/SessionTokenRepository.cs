using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Passi.Core.Application.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Extensions;
using System.Text;


namespace Passi.Authentication.Fake.Repository
{
    class SessionTokenRepository : IInfoRepository<SessionToken>
    {
        private readonly IHttpContextAccessor accessor;
        private readonly IDataCypherService cypherService;
        private readonly ConfigurationOptions options;

        public SessionTokenRepository(IHttpContextAccessor accessor,
            IDataCypherService cypherService,
            IOptions<ConfigurationOptions> options)
        {
            this.accessor = accessor;
            this.cypherService = cypherService;
            this.options = options.Value;
        }

        public Task<SessionToken> RetrieveAsync()
        {
            var token = new SessionToken
            {
                ServiceId = options.ServiceId
            };

            var context = accessor.HttpContext;
            var data = context?.Request.GetString(Keys.SessionToken);
            if (!string.IsNullOrWhiteSpace(data))
            {
                data = cypherService.Decrypt(data);
                var pieces = data.Split(Keys.Separator);
                if (pieces.Length >= 9)
                {
                    var url = pieces.ElementAtOrDefault(8);
                    token.SessionId = pieces.ElementAtOrDefault(1) ?? string.Empty;
                    token.UserId = pieces.ElementAtOrDefault(2) ?? string.Empty;
                    token.LoggedIn = pieces.ElementAtOrDefault(3)?.ToDatetime() ?? DateTime.Now;
                    token.UserTypeId = int.Parse(pieces.ElementAtOrDefault(4) ?? "0");
                    token.InstitutionCode = pieces.ElementAtOrDefault(5) ?? string.Empty;
                    token.OfficeCode = pieces.ElementAtOrDefault(6) ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(url))
                        token.ServiceUri = new Uri(url);
                }
            }

            return Task.FromResult(token);

        }

        public Task<SessionToken> UpdateAsync(SessionToken item)
        {
            throw new NotImplementedException();
        }
    }

}

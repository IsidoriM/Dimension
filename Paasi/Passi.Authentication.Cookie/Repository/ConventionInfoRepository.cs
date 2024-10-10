using Microsoft.AspNetCore.Http;
using Passi.Authentication.Cookie.Const;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Extensions;
using System.Text;

namespace Passi.Authentication.Cookie.Repository
{
    class ConventionInfoRepository : IInfoRepository<ConventionInfo>
    {
        private readonly IHttpContextAccessor accessor;
        private readonly IDataCypherService dataCypher;

        public ConventionInfoRepository(IHttpContextAccessor accessor, IDataCypherService dataCypher)
        {
            this.accessor = accessor;
            this.dataCypher = dataCypher;
        }

        public ConventionInfo Compress(ConventionInfo item)
        {
            throw new NotImplementedException();
        }

        public Task<ConventionInfo> RetrieveAsync()
        {
            ConventionInfo result = new();

            var context = accessor.HttpContext;
            var cookie = context?.Request.Cookies[Cookies.Convention];
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                cookie = dataCypher.Decrypt(cookie, Crypto.KCA);
                var data = cookie.Split(Keys.Separator);

                if (data.Length >= 3)
                {
                    result.UserId = data.GetString(0);
                    result.UserTypeId = data.GetInt(1);
                    result.WorkOfficeCode = data.GetString(2);
                    foreach (var item in data.Skip(3))
                    {
                        string[] conventionPieces = item.Split('#');

                        var isAvailable = conventionPieces.GetString(0) != "0";
                        var conventionServiceId = conventionPieces.GetInt(1);

                        var convention = new Convention()
                        {
                            IsAvailable = isAvailable,
                            ServiceId = conventionServiceId,
                        };

                        foreach (string datum in conventionPieces.Skip(2))
                        {
                            var firstChar = datum[..1];
                            if (firstChar != "*")
                            {
                                // Se la prima lettera è R, allora è un filtro
                                if (firstChar != "R")
                                {
                                    // //viene creato il Filter
                                    var filter = new Filter
                                    {
                                        Type = firstChar,
                                        Scope = datum.Substring(1, 1),
                                        Value = datum[2..],
                                    };
                                    // viene aggiunto alla collection di filtri
                                    convention.Filters.Add(filter);
                                }
                                // se si tratta di un ruolo
                                else
                                {
                                    var role = new Role
                                    {
                                        Value = datum[2..]
                                    };
                                    convention.Roles.Add(role);
                                }
                            }
                        }

                        result.Conventions.Add(convention);
                    }
                }
            }
            return Task.FromResult(result);

        }

        public Task<ConventionInfo> UpdateAsync(ConventionInfo item)
        {
            if (item.HasConvention)
            {
                var cookie = item.Serialize();
                accessor.HttpContext?.AddCypheredCookie(Cookies.Convention, cookie, dataCypher);
            }
            return Task.FromResult(item);
        }

        internal Task UpdateAsync(string userId)
        {
            throw new NotImplementedException();
        }
    }
}

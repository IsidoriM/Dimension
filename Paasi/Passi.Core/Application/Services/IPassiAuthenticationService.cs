using Microsoft.AspNetCore.Http;
using Passi.Core.Domain.Entities.Info;

namespace Passi.Core.Application.Services
{
    internal interface IPassiAuthenticationService
    {
        public Task<SessionInfo> IsAuthorizedAsync(int serviceId, string returnUrl = "");
    }
}

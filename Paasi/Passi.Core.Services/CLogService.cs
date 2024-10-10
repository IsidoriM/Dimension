using Microsoft.AspNetCore.Http;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Extensions;

namespace Passi.Core.Services
{
    internal class CLogService : ICLogService
    {
        private readonly ICLogRepository cLogRepository;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IInfoRepository<SessionInfo> sessionInfoRepository;
        private readonly IInfoRepository<ContactCenterInfo> contactCenterInfoRepository;

        public CLogService(ICLogRepository cLogRepository, IHttpContextAccessor contextAccessor, IInfoRepository<SessionInfo> sessionInfoRepository, IInfoRepository<ContactCenterInfo> contactCenterInfoRepository)
        {
            this.cLogRepository = cLogRepository;
            this.contextAccessor = contextAccessor;
            this.sessionInfoRepository = sessionInfoRepository;
            this.contactCenterInfoRepository = contactCenterInfoRepository;
        }

        public async Task LogAsync(int eventId, int executionTime, Dictionary<string, string> parameters, int returnCode = 0, string? errorMessage = null)
        {
            SessionInfo sessionInfo;
            try
            {
                sessionInfo = await sessionInfoRepository.RetrieveAsync();
                try
                {
                    //Devo verificare che non ci sia un Operatore CC che stia lavorando per un utente,
                    //in tal caso devo prendere le generalità dell'utente e non quelle dell'Operatore
                    var contactCenterInfo = await contactCenterInfoRepository.RetrieveAsync();
                    sessionInfo.CheckContactCenterInfo(contactCenterInfo);
                }
                catch (NotFoundException)
                {
                    //Do nothing
                }
            }
            catch
            {
                throw new CLogException("Utente non autorizzato al salvataggio su CLog");
            }

            var ip = contextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip))
                ip = string.Empty;

            var parameterz = new List<string>();
            foreach (var parameter in parameters)
            {
                parameterz.Add(parameter.Key + "=" + parameter.Value);
            }

            var tipoUtente = sessionInfo.ProfileTypeId;

            await cLogRepository.LogAsync(
                !string.IsNullOrWhiteSpace(sessionInfo.DelegateUserId) ? sessionInfo.DelegateUserId : sessionInfo.UserId,
                eventId,
                ip,
                executionTime,
                returnCode,
                tipoUtente,
                !string.IsNullOrWhiteSpace(sessionInfo.InstitutionCode) ? sessionInfo.InstitutionCode : "N.A.",
                !string.IsNullOrWhiteSpace(sessionInfo.OfficeCode) ? sessionInfo.OfficeCode : "N.A.",
                string.Join(';', parameterz),
                errorMessage);
        }
    }
}

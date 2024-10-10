using Bogus;
using Bogus.Extensions.Italy;
using Microsoft.Extensions.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Store.Fake.Options;
using System.Text;

namespace Passi.Authentication.Fake.Repository
{
    class SessionInfoRepository : IInfoRepository<SessionInfo>
    {
        private readonly UserOptions options;
        private readonly IInfoRepository<UserInfo> userInfoRepository;

        private static SessionInfo? SessionInfo;

        public SessionInfoRepository(IOptionsMonitor<UserOptions> options, IInfoRepository<UserInfo> userInfoRepository)
        {
            this.options = options.CurrentValue;
            this.userInfoRepository = userInfoRepository;
        }

        public async Task<SessionInfo> RetrieveAsync()
        {
            if (SessionInfo == null)
            {
                UserInfo userInfo = await userInfoRepository.RetrieveAsync();
                Faker<SessionInfo> faker = new Faker<SessionInfo>();

                faker.RuleFor(p => p.InstitutionCode, f => options.Profile.InstitutionCode ?? "CITTADINO");
                faker.RuleFor(p => p.InstitutionDescription, f => options.Profile.InstitutionDescription ?? "");
                faker.RuleFor(p => p.ProfileTypeId, f => (options.Profile.ProfileTypeId != 0) ? options.Profile.ProfileTypeId : 3);
                faker.RuleFor(p => p.OfficeCode, f => options.Profile.OfficeCode ?? "");
                faker.RuleFor(p => p.InstitutionFiscalCode, f => f.Person.CodiceFiscale());
                faker.RuleFor(p => p.AuthenticationType, f => "2SPI");
                faker.RuleFor(p => p.UserClass, f => f.Random.Int(1, 3).ToString());
                faker.RuleFor(p => p.Number, f => string.Empty);
                faker.RuleFor(p => p.LastAccess, f => DateTime.UtcNow.AddDays(-1));
                faker.RuleFor(p => p.LastUpdated, f => DateTime.UtcNow.AddMinutes(-1));
                faker.RuleFor(p => p.SessionTimeout, f => new TimeSpan(0, 30, 0));
                faker.RuleFor(p => p.LoggedIn, f => DateTime.UtcNow.AddDays(-1));
                faker.RuleFor(p => p.LastPinUpdate, f => DateTime.MinValue);
                faker.RuleFor(p => p.IsPinUnified, f => false);
                faker.RuleFor(p => p.InformationCampaign, f => false);
                faker.RuleFor(p => p.IsInfoPrivacyAccepted, f => true);
                faker.RuleFor(p => p.SessionId, f =>
                {
                    byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes(f.Person.FirstName);
                    return Convert.ToBase64String(toEncodeAsBytes);
                });
                faker.RuleFor(p => p.LastSessionUpdate, f => DateTime.UtcNow.AddHours(-1));
                faker.RuleFor(p => p.SessionMaximumTime, f => new TimeSpan(72, 0, 0));
                faker.RuleFor(p => p.SessionMaximumCachingTime, f => new TimeSpan(0, 2, 0));
                faker.RuleFor(p => p.HasSessionFlag, f => false);
                faker.RuleFor(p => p.MultipleProfile, f => true);
                faker.RuleFor(p => p.AnonymousId, f => "poste_id_OLD2");
                faker.RuleFor(p => p.DelegateUserId, f => userInfo.FiscalCode);
                faker.RuleFor(p => p.IsFromLogin, f => false);

                SetSessionInfo(faker.Generate(), userInfo);
            }

            return SessionInfo!;
        }

        public Task<SessionInfo> UpdateAsync(SessionInfo item)
        {
            return Task.FromResult(item);
        }

        private static void SetSessionInfo(SessionInfo sessionInfo, UserInfo userInfo)
        {
            SessionInfo = sessionInfo;
            SessionInfo.UserId = userInfo.UserId;
            SessionInfo.FiscalCode = userInfo.FiscalCode;
            SessionInfo.Name = userInfo.Name;
            SessionInfo.Surname = userInfo.Surname;
            SessionInfo.Gender = userInfo.Gender;
            SessionInfo.Email = userInfo.Email;
            SessionInfo.Phone = userInfo.Phone;
            SessionInfo.BirthPlaceCode = userInfo.BirthPlaceCode;
            SessionInfo.BirthProvince = userInfo.BirthProvince;
            SessionInfo.BirthDate = userInfo.BirthDate;
            SessionInfo.Mobile = userInfo.Mobile;
            SessionInfo.PEC = userInfo.PEC;
        }
    }
}

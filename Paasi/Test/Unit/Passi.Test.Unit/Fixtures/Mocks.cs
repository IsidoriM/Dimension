using Bogus.Extensions.Italy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Passi.Core.Application.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Services;
using System.Security.Cryptography;

namespace Passi.Test.Unit.Fixtures
{
    public class Mocks
    {
        private readonly int? requiredUserTypeId;
        public const string InstitutionCode = "009";

        public Mocks(int serviceId, PassiFixture _, int? requiredUserTypeId)
        {
            SessionInfo = TestSessionInfo();
            SessionRepo = new Mock<IInfoRepository<SessionInfo>>();
            SessionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(SessionInfo);
            SessionRepo.Setup(x => x.UpdateAsync(It.IsAny<SessionInfo>())).ReturnsAsync(SessionInfo);

            UserInfo = TestSessionInfo();
            UserInfoRepo = new Mock<IInfoRepository<UserInfo>>();
            UserInfoRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(UserInfo);
            UserInfoRepo.Setup(x => x.UpdateAsync(It.IsAny<UserInfo>())).ReturnsAsync(UserInfo);

            ProfileInfo = TestProfileInfo(serviceId, SessionInfo);
            ProfileRepo = new Mock<IInfoRepository<ProfileInfo>>();
            ProfileRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(ProfileInfo);
            ProfileRepo.Setup(x => x.UpdateAsync(It.IsAny<ProfileInfo>())).ReturnsAsync(ProfileInfo);

            ConventionInfo = TestConventionInfo(serviceId);
            ConventionRepo = new Mock<IInfoRepository<ConventionInfo>>();
            ConventionRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(ConventionInfo);
            ConventionRepo.Setup(x => x.UpdateAsync(It.IsAny<ConventionInfo>())).ReturnsAsync(ConventionInfo);

            SessionToken = TestSessionToken(serviceId);
            SessionTokenRepo = new Mock<IInfoRepository<SessionToken>>();
            SessionTokenRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(SessionToken);
            SessionTokenRepo.Setup(x => x.UpdateAsync(It.IsAny<SessionToken>())).ReturnsAsync(SessionToken);

            ContactCenterInfo = TestContactCenterInfo();
            ContactCenterRepo = new Mock<IInfoRepository<ContactCenterInfo>>();
            ContactCenterRepo.Setup(x => x.RetrieveAsync()).ReturnsAsync(ContactCenterInfo);
            ContactCenterRepo.Setup(x => x.UpdateAsync(It.IsAny<ContactCenterInfo>())).ReturnsAsync(ContactCenterInfo);

            UserRepo = new Mock<IUserRepository>();
            UserRepo.Setup(m => m.ProfilesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(new List<Profile>()
                {
                    new Profile() { ProfileTypeId = ProfileInfo.ProfileTypeId, InstitutionCode = InstitutionCode }
                });
            UserRepo.Setup(m => m.HasSessionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>())).ReturnsAsync(false);

            UserRepo.Setup(m => m.ServicesAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<Service>()
            {
                new Service()
                {
                    GroupId = 1,
                    HasConvention = false,
                    Id= 1,
                    RequiredAuthenticationType = "2SPI".ShortDescribe()
                }
            });

            UserRepo.Setup(m => m.IsDelegationAvailableAsync(
                It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(false);

            UserRepo.Setup(m => m.AuthorizedServicesAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(ProfileInfo.Services);

            LevelsRepo = new Mock<ILevelsRepository>();
            LevelsRepo.Setup(x => x.CompareAuthorizationAsync(It.IsAny<char>(),
                It.IsAny<char>())).ReturnsAsync(true);
            LevelsRepo.Setup(x => x.LevelsAsync()).ReturnsAsync(new List<AuthorizationLevel>() {
                    new AuthorizationLevel() { AuthenticationType = CommonAuthenticationTypes.CNS.ShortDescribe(), Priority = 10 },
                    new AuthorizationLevel() { AuthenticationType = CommonAuthenticationTypes.OTP.ShortDescribe(), Priority = 10 },
                    new AuthorizationLevel() { AuthenticationType = "1SPI".ShortDescribe(), Priority = 10 },
                    new AuthorizationLevel() { AuthenticationType = "2SPI".ShortDescribe(), Priority = 20 },
                    new AuthorizationLevel() { AuthenticationType = "3SPI".ShortDescribe(), Priority = 30 }
                });

            ConfigurationOptions = Options.Create(new ConfigurationOptions()
            {
                SessionManagementFlag = "3",
                ServiceId = 1,
            });

            ConfigurationOptions.Value.SessionManagementFlag = SessionFlags.Production.ToString();

            UserContactsService = new Mock<IPassiUserContactsService>();
            UserContacts userContacts = new()
            {
                Email = Guid.NewGuid().ToString(),
            };
            UserContactsService.Setup(s => s.UserContactsAsync()).ReturnsAsync(userContacts);
            UserContactsService.Setup(s => s.UserContactsAsync(It.IsAny<string>())).ReturnsAsync(userContacts);

            this.requiredUserTypeId = requiredUserTypeId;
        }

        internal IPassiAuthenticationService PackApiAuthService(bool nullContextAccessor = false)
        {
            return new ApiAuthenticationService(
                SessionTokenRepo.Object,
                PassiFixture.DataCypherUnderTest(),
                UrlOptions,
                ConfigurationOptions,
                PackWebAuthService(),
                HostingAppManager(),
                nullContextAccessor ? PassiFixture.NullContextAccessor() : PassiFixture.AccessorUnderTest(requiredUserTypeId?.ToString()));
        }


        internal IPassiAuthenticationService PackWebAuthService(bool nullContextAccessor = false, string? overrideRequiredUserTypeId = null)
        {
            string? requiredUserTypeIdString = requiredUserTypeId?.ToString();
            if (!string.IsNullOrWhiteSpace(overrideRequiredUserTypeId))
            {
                requiredUserTypeIdString = overrideRequiredUserTypeId;
            }
            return new AuthenticationService(
                nullContextAccessor ? PassiFixture.NullContextAccessor() : PassiFixture.AccessorUnderTest(requiredUserTypeIdString),
                ProfileRepo.Object,
                SessionRepo.Object,
                ConventionRepo.Object,
                ContactCenterRepo.Object,
                PassiFixture.DataCypherUnderTest(),
                UserRepo.Object,
                LevelsRepo.Object,
                new Mock<ILogger<AuthenticationService>>().Object,
                UrlOptions,
                ConfigurationOptions);
        }

        public IPassiService PackPassiService()
        {
            return new PassiService(
                UrlOptions,
                UserContactsService.Object,
                ContactCenterRepo.Object,
                SessionRepo.Object,
                ProfileRepo.Object,
                ConventionRepo.Object,
                LevelsRepo.Object,
                UserRepo.Object
                );
        }

        public IPassiSecureService PackPassiSecure()
        {
            return new PassiSecureService(
                PassiFixture.DataCypherUnderTest(),
                SessionRepo.Object,
                ContactCenterRepo.Object,
                ConfigurationOptions,
                PassiFixture.AccessorUnderTest(requiredUserTypeId?.ToString())
                );
        }

        IHostingAppManager HostingAppManager()
        {
            var mock = new Mock<IHostingAppManager>();
            mock.Setup(x => x.ClearExternalInfoAsync()).Returns(Task.CompletedTask);
            return mock.Object;
        }


        public IPassiConventionService PackPassiConventionService()
        {
            return new PassiConventionService(
                ConventionRepo.Object,
                ConfigurationOptions);
        }

        internal Mock<IUserRepository> UserRepo { get; private set; }

        internal Mock<IInfoRepository<ProfileInfo>> ProfileRepo { get; private set; }
        internal ProfileInfo ProfileInfo { get; private set; }

        internal Mock<IInfoRepository<SessionInfo>> SessionRepo { get; private set; }
        internal SessionInfo SessionInfo { get; private set; }

        internal Mock<IInfoRepository<UserInfo>> UserInfoRepo { get; private set; }
        internal UserInfo UserInfo { get; private set; }

        internal Mock<IInfoRepository<ConventionInfo>> ConventionRepo { get; private set; }
        internal ConventionInfo ConventionInfo { get; private set; }

        internal Mock<IInfoRepository<SessionToken>> SessionTokenRepo { get; private set; }
        internal SessionToken SessionToken { get; private set; }

        internal Mock<IInfoRepository<ContactCenterInfo>> ContactCenterRepo { get; private set; }
        internal ContactCenterInfo ContactCenterInfo { get; private set; }

        internal Mock<ILevelsRepository> LevelsRepo { get; private set; }

        internal IOptions<ConfigurationOptions> ConfigurationOptions { get; private set; }

        internal static IOptions<UrlOptions> UrlOptions => Options.Create(new UrlOptions()
        {
            ChangeContacts = new Uri("https://www.LinkChangeContacts.com"),
            ChangePin = new Uri("https://www.LinkChangePin.com"),
            ErrorPage = new Uri("https://www.LinkErrorPage.com"),
            Logout = new Uri("https://www.LinkLogout.com"),
            SwitchProfile = new Uri("https://www.LinkSwitchProfile.com"),
            PassiWeb = new Uri("https://www.LinkPassiWeb.com?uri="),
            PassiWebCns = new Uri("https://www.LinkPassiWebC.com?uri="),
            PassiWebI = new Uri("https://www.LinkPassiWebI.com?uri="),
            PassiWebOtp = new Uri("https://www.LinkPassiWebO.com?uri="),
        });


        private static ProfileInfo TestProfileInfo(int serviceId, SessionInfo info)
        {
            var now = DateTime.UtcNow;
            now = now.AddMilliseconds(-now.Millisecond);

            var profile = new ProfileInfo()
            {
                InstitutionCode = InstitutionCode,
                FiscalCode = Guid.NewGuid().ToString(),
                Closing = DateTime.UtcNow.Date.AddDays(1).AddMinutes(-1),
                Opening = DateTime.UtcNow.Date,
                LastUpdate = now,
                Id = Guid.NewGuid().ToString(),
                ProfileTypeId = info.ProfileTypeId,
                Timeout = TimeSpan.FromSeconds(180),
                OfficeCode = Guid.NewGuid().ToString(),
            };

            profile.InstitutionCode = info.InstitutionCode;
            profile.FiscalCode = info.FiscalCode;
            if (serviceId > 0)
            {
                profile.Services.Add(new Service()
                {
                    Id = serviceId,
                    HasConvention = true,
                    RequiredAuthenticationType = "1SPI".ShortDescribe()
                });
            }
            return profile;
        }

        private static SessionInfo TestSessionInfo()
        {
            var person = new Bogus.Person();

            var now = DateTime.UtcNow;
            now = now.AddMilliseconds(-now.Millisecond);

            var inputSessionInfo = new SessionInfo
            {
                AnonymousId = Guid.NewGuid().ToString(),
                AuthenticationType = "2SPI",
                BirthPlaceCode = Guid.NewGuid().ToString(),
                BirthDate = now.AddDays(-1),
                DelegateUserId = string.Empty,
                Email = person.Email,
                InstitutionDescription = Guid.NewGuid().ToString(),
                InstitutionFiscalCode = Guid.NewGuid().ToString(),
                InstitutionCode = InstitutionCode,
                FiscalCode = person.CodiceFiscale(),
                LoggedIn = now.AddMinutes(-10),
                InformationCampaign = true,
                IsFromLogin = true,
                LastAccess = now.AddDays(-1),
                LastPinUpdate = now,
                LastSessionUpdate = now,
                LastUpdated = now,
                Mobile = Guid.NewGuid().ToString(),
                Name = person.FirstName,
                Number = Guid.NewGuid().ToString(),
                PEC = Guid.NewGuid().ToString(),
                PECVerificationStatus = StringExtensions.Random<PecVerificationStatuses>(),
                Phone = Guid.NewGuid().ToString(),
                ProfileTypeId = RandomNumberGenerator.GetInt32(0, 100),
                SessionId = Guid.NewGuid().ToString(),
                SessionMaximumCachingTime = TimeSpan.FromSeconds(1800),
                SessionMaximumTime = TimeSpan.FromSeconds(1800),
                SessionTimeout = TimeSpan.FromSeconds(1800),
                Surname = person.LastName,
                UserId = person.CodiceFiscale(),
                OfficeCode = Guid.NewGuid().ToString()
            };

            return inputSessionInfo;

        }

        private static SessionToken TestSessionToken(int serviceId)
        {
            var person = new Bogus.Person();

            var now = DateTime.UtcNow;
            now = now.AddMilliseconds(-now.Millisecond);

            var inputSessionInfo = new SessionToken
            {
                InstitutionCode = InstitutionCode,
                LoggedIn = now.AddMinutes(-10),
                UserTypeId = RandomNumberGenerator.GetInt32(0, 100),
                SessionId = Guid.NewGuid().ToString(),
                UserId = person.CodiceFiscale(),
                OfficeCode = Guid.NewGuid().ToString(),
                ServiceId = serviceId,
                ServiceUri = UriExtensions.Default
            };
            return inputSessionInfo;

        }

        private static ConventionInfo TestConventionInfo(int serviceId)
        {
            var conventionInfo = new ConventionInfo();
            conventionInfo.Conventions.Add(new Convention()
            {
                ServiceId = serviceId,
                IsAvailable = true,
                Roles = new HashSet<Role>()
                {
                    new Role()
                    {
                        Value = "AAA"
                    },
                    new Role()
                    {
                        Value = "BBB"
                    }
                },
                Filters = new HashSet<Filter>()
                {
                    new Filter()
                    {
                        Value = "CCC",
                        Scope = "A",
                        Type = "R"
                    }
                }
            });
            return conventionInfo;
        }

        private static ContactCenterInfo TestContactCenterInfo()
        {
            var person = new Bogus.Person("it");
            var contactCenterInfo = new ContactCenterInfo()
            {
                BirthPlaceCode = Guid.NewGuid().ToString(),
                BirthDate = person.DateOfBirth,
                BirthProvince = person.Address.City,
                Email = person.Email,
                FiscalCode = person.CodiceFiscale().ToString(),
                Mobile = person.Phone,
                OfficeCode = Guid.NewGuid().ToString(),
                Name = person.FirstName,
                Surname = person.LastName,
                Gender = person.Gender.ToString(),
                OperatorId = "TSEDI001",
                OperatorUserClass = "2030"
            };
            contactCenterInfo.UserId = contactCenterInfo.FiscalCode;
            return contactCenterInfo;
        }

        internal Mock<IPassiUserContactsService> UserContactsService { get; private set; }
    }
}

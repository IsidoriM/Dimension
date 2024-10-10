using Bogus;
using Bogus.Extensions.Italy;
using Microsoft.Extensions.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Store.Fake.Options;

namespace Passi.Authentication.Fake.Repository
{
    class UserInfoRepository : IInfoRepository<UserInfo>
    {
        private static UserInfo? UserInfo;
        public UserInfoRepository(IOptionsMonitor<UserOptions> options)
        {
            SetUserInfo(options.CurrentValue);
        }

        public Task<UserInfo> RetrieveAsync()
        {
            return Task.FromResult(UserInfo!);
        }

        public Task<UserInfo> UpdateAsync(UserInfo item)
        {
            return Task.FromResult(item);
        }

        private static void SetUserInfo(UserOptions options)
        {
            if (UserInfo == null)
            {
                var userInfoFaker = new Faker<UserInfo>("it");
                userInfoFaker.RuleFor(p => p.UserId, f => options.UserId ?? f.Person.CodiceFiscale());
                userInfoFaker.RuleFor(p => p.FiscalCode, f => options.FiscalCode ?? f.Person.CodiceFiscale());
                userInfoFaker.RuleFor(p => p.Name, f => options.Name ?? f.Person.FirstName);
                userInfoFaker.RuleFor(p => p.Surname, f => options.Surname ?? f.Person.LastName);
                userInfoFaker.RuleFor(p => p.Gender, f => options.Gender ?? "M");
                userInfoFaker.RuleFor(p => p.BirthPlaceCode, f => f.Address.City());
                userInfoFaker.RuleFor(p => p.BirthProvince, f => f.Address.City());
                userInfoFaker.RuleFor(p => p.BirthDate, f => f.Person.DateOfBirth);
                userInfoFaker.RuleFor(p => p.Email, f => options.Email ?? f.Person.Email);
                userInfoFaker.RuleFor(p => p.PEC, f => options.PEC ?? f.Internet.Email(provider: "inpspec.it"));
                userInfoFaker.RuleFor(p => p.Phone, f => f.Person.Phone);
                userInfoFaker.RuleFor(p => p.Mobile, f => f.Phone.PhoneNumber());

                UserInfo = userInfoFaker.Generate();
            }
        }
    }
}

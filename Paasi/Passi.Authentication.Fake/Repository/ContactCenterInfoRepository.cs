using Bogus;
using Bogus.Extensions.Italy;
using Passi.Core.Application.Repositories;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities.Info;

namespace Passi.Authentication.Fake.Repository
{
    class ContactCenterInfoRepository : IInfoRepository<ContactCenterInfo>
    {
        private static ContactCenterInfo? ContactCenterInfo;
        public ContactCenterInfoRepository() { }

        public Task<ContactCenterInfo> RetrieveAsync()
        {
            SetContactCenterInfo();
            return Task.FromResult(ContactCenterInfo!);
        }

        public Task<ContactCenterInfo> UpdateAsync(ContactCenterInfo item)
        {
            return Task.FromResult(item);
        }

        private static void SetContactCenterInfo()
        {
            if (ContactCenterInfo == null)
            {
                Faker<ContactCenterInfo> faker = new Faker<ContactCenterInfo>("it");
                faker.RuleFor(r => r.BirthPlaceCode, p => p.Person.Address.City);
                faker.RuleFor(r => r.BirthDate, p => p.Person.DateOfBirth);
                faker.RuleFor(r => r.BirthProvince, p => p.Address.CityPrefix());
                faker.RuleFor(r => r.Email, p => p.Person.Email);
                faker.RuleFor(r => r.FiscalCode, p => p.Person.CodiceFiscale());
                faker.RuleFor(r => r.Mobile, p => p.Person.Phone);
                faker.RuleFor(r => r.Name, p => p.Person.FirstName);
                faker.RuleFor(r => r.OfficeCode, p => string.Empty);
                faker.RuleFor(r => r.OperatorId, p => "TSEDI001");
                faker.RuleFor(r => r.OperatorUserClass, p => "2030");
                faker.RuleFor(r => r.PEC, p => p.Person.Email);
                faker.RuleFor(r => r.Phone, p => p.Phone.PhoneNumber());
                faker.RuleFor(r => r.Pin, p => string.Empty);
                faker.RuleFor(r => r.Gender, p => p.Person.Gender.ToString());
                faker.RuleFor(r => r.Surname, p => p.Person.LastName);
                ContactCenterInfo = faker.Generate();
            }
        }
    }
}

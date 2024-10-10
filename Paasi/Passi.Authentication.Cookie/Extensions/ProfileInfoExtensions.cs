using Passi.Core.Domain.Const;

namespace Passi.Core.Domain.Entities.Info
{
    static class ProfileInfoExtensions
    {
        public static string Serialize(this ProfileInfo item)
        {
            var profileData = new List<string>
            {
                item.FiscalCode,
                item.ProfileTypeId.ToString(),
                item.InstitutionCode,
                item.OfficeCode,
                item.LastUpdate.ToMilliseconds().ToString(),
                item.Timeout.TotalSeconds.ToString(),
                item.Opening.ToString(Keys.HourFormat),
                item.Closing.ToString(Keys.HourFormat)
            };


            if (item.Services.Any())
            {
                foreach (var service in item.Services)
                {
                    var hasConvention = service.HasConvention ? "1" : "0";
                    profileData.Add($"{service.RequiredAuthenticationType}{service.Id}{hasConvention}");
                }
            }

            return string.Join(Keys.Separator, profileData);
        }
    }
}

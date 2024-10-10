using Passi.Core.Domain.Const;

namespace Passi.Core.Domain.Entities.Info
{
    static class ConventionInfoExtensions
    {
        public static string Serialize(this ConventionInfo item)
        {
            var cookiePieces = new List<string>
            {
                item.UserId,
                item.UserTypeId.ToString(),
                item.WorkOfficeCode
            };
            foreach (var convention in item.Conventions)
            {
                var conventionCookie = new List<string>();
                var isAvailable = convention.IsAvailable ? "1" : "0";
                conventionCookie.Add(isAvailable);
                conventionCookie.Add(convention.ServiceId.ToString());
                foreach (var filter in convention.Filters)
                {
                    conventionCookie.Add($"{conventionCookie}#{filter.Type}{filter.Scope}{filter.Value}");
                }
                foreach (var role in convention.Roles)
                {
                    conventionCookie.Add($"{conventionCookie}#R*{role.Value}");
                }
                cookiePieces.Add(string.Join('#', conventionCookie));
            }

            return string.Join(Keys.Separator, cookiePieces);
        }
    }
}

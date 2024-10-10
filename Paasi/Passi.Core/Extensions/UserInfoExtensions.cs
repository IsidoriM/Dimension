using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;

namespace Passi.Core.Extensions
{
    static class UserInfoExtensions
    {
        public static User MapUser(this SessionInfo userInfo)
        {
            return new User()
            {
                UserId = userInfo.UserId,
                Name = userInfo.Name,
                Surname = userInfo.Surname,
                Gender = userInfo.Gender,
                BirthPlaceCode = userInfo.BirthPlaceCode,
                BirthDate = userInfo.BirthDate.ToBirthdayFormat(),
                BirthProvince = userInfo.BirthProvince,
                MultipleProfile = userInfo.MultipleProfile,
                AuthenticationType = userInfo.AuthenticationType
            };
        }
    }
}

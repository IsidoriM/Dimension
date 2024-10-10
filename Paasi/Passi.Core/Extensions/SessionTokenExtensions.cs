using Passi.Core.Domain.Const;
using System.Security.Cryptography;

namespace Passi.Core.Domain.Entities.Info
{
    static class SessionTokenExtensions
    {
        private static string Random(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[RandomNumberGenerator.GetInt32(0, s.Length)]).ToArray());
        }

        public static string Serialize(this SessionToken token)
        {
            var result = new List<string>
            {
                Random(4),
                token.SessionId,
                token.UserId,
                token.LoggedIn.ToString(),
                token.UserTypeId.ToString(),
                token.InstitutionCode,
                token.OfficeCode,
                token.ServiceId.ToString(),
                token.ServiceUri.ToString(),
            };

            return string.Join(Keys.Separator, result);
        }
    }
}

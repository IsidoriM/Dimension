namespace Passi.Core.Domain.Const
{
    struct CommonAuthenticationTypes
    {
        public const string PIN = "PIN";
        public const string OTP = "OTP";
        public const string CNS = "CNS";
        public const string LOW = "LOW";
        public const string IPOL = "IPOL";
        public const string Undefined = "UDEF";
    }

    static class AuthenticationTypesExtensions
    {
        public static char ShortDescribe(this string item)
        {
            return item.FirstOrDefault();
        }
    }
}

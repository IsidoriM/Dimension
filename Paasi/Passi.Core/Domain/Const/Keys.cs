namespace Passi.Core.Domain.Const
{
    struct Config
    {
        public const string CONN_SICUREZZA = "passiConnSicurezza";
        public const string CONN_UTENZE = "passiConnUtenze";
        public const string CONN_DELEGAPATR = "PassiConnDelega";
        public const string CONN_LOG = "passiConnLog";
    }

    struct Keys
    {
        public const char Separator = '|';
        public const char OptionalSeparator = '&';
        public const string HourFormat = "HH:mm:ss.ffff";
        public const string Ente = "S";
        public const string ErrorMessage = "errorMsg";
        public const string RequiredUserTypeId = "srcPortal";
        public const string Uri = "uri";
        public const string Cookie = "Cookie";
        public const string Secure = "secure";
        public const string SecureTable = "st";
        public const string ServiceId = "idServizio";
        public const string SessionManagementFlag = "GestioneSessione";
        public const string Spec = "spec";
        public const string SessionToken = "SessionToken";
        public const string ReturnUrl = "returnUrl";

    }

    struct SpecialServices
    {
        public const int EveryProfileIsOK = 0;
        public const int NoProfileNeeded = -1;
    }

    struct SpecialProfiles
    {
        public const int ContactCenter = 30;
        public const int Citizen = 3;
        public const int Thousand = -1000;
    }

    struct SessionFlags
    {
        public const int Production = 3;
        public const int Debug = 1;
        public const int Test = 2;
    }

    public struct Schema
    {
        public const string Https = "https";
    }
}

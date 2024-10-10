using Passi.Core.Domain.Const;

namespace Passi.Core.Domain.Entities
{
    class AuthorizationLevel
    {
        public char AuthenticationType { get; set; } = CommonAuthenticationTypes.Undefined.ShortDescribe();
        public int Priority { get; set; } = int.MinValue;
    }
}

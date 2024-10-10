using Passi.Core.Domain.Const;

namespace Passi.Core.Domain.Entities
{
    class Service
    {
        public int Id { get; internal set; } = 0;
        public int GroupId { get; internal set; } = 0;
        public bool HasConvention { get; internal set; } = false;
        public char RequiredAuthenticationType { get; internal set; } = CommonAuthenticationTypes.Undefined.ShortDescribe();
    }
}

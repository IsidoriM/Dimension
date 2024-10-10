namespace Passi.Core.Domain.Entities.Info
{
    class ContactCenterInfo : UserInfo
    {
        public string OperatorId { get; set; } = string.Empty;
        public string OperatorUserClass { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
    }
}

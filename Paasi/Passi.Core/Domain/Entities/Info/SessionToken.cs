namespace Passi.Core.Domain.Entities.Info
{
    [Serializable]
    class SessionToken
    {

        public SessionToken() : base() { }

        public string UserId { get; set; } = string.Empty;
        public DateTime LoggedIn { get; set; } = DateTime.MinValue;
        public int UserTypeId { get; set; } = 0;
        public string InstitutionCode { get; set; } = string.Empty;
        public string OfficeCode { get; set; } = string.Empty;
        public Uri ServiceUri { get; set; } = UriExtensions.Default;
        public string SessionId { get; set; } = string.Empty;
        public int ServiceId { get; set; } = 0;

        public bool IsValid => !string.IsNullOrWhiteSpace(UserId) || !string.IsNullOrWhiteSpace(SessionId);
    }

}

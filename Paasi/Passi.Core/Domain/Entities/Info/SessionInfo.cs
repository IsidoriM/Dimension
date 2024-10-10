using Passi.Core.Domain.Const;

namespace Passi.Core.Domain.Entities.Info
{
    [Serializable]
    class SessionInfo : UserInfo
    {
        public string SessionId { get; internal set; } = string.Empty;

        public string DelegateUserId { get; internal set; } = string.Empty;

        public string AuthenticationType { get; internal set; } = CommonAuthenticationTypes.Undefined;

        /// <summary>
        /// è il tipo di profilo con cui l’utente accede al servizio. (es. 1 per patronato, 14 per CAF, 3 per cittadino).
        /// Per determinare il tipo di profilo su cui l’utente è attestato va esaminata la proprietà tipoUtente dell’oggetto SessionInfo 
        /// e non la proprietà idClasseUtente che è in dismissione.
        /// </summary>
        public int ProfileTypeId { get; internal set; } = 0;

        /// <summary>
        /// Equivale alla matricola
        /// </summary>
        public string Number { get; internal set; } = string.Empty;

        public DateTime LastAccess { get; internal set; } = DateTime.MinValue;
        public DateTime LastUpdated { get; internal set; } = DateTime.MinValue;
        public TimeSpan SessionTimeout { get; internal set; } = TimeSpan.FromMilliseconds(0);

        public DateTime LoggedIn { get; internal set; } = DateTime.MinValue;
        public TimeSpan SessionMaximumTime { get; internal set; } = TimeSpan.FromMilliseconds(0);
        public DateTime LastPinUpdate { get; internal set; } = DateTime.MinValue;
        public DateTime LastSessionUpdate { get; internal set; } = DateTime.MinValue;
        public TimeSpan SessionMaximumCachingTime { get; internal set; } = TimeSpan.FromMilliseconds(0);

        public string InstitutionCode { get; internal set; } = string.Empty;
        public string InstitutionFiscalCode { get; internal set; } = string.Empty;
        public string InstitutionDescription { get; internal set; } = string.Empty;
        public string UserClass { get; internal set; } = string.Empty; // Da documentazione, è in dismissione
        public bool IsPinUnified { get; internal set; } = false;
        public bool InformationCampaign { get; internal set; } = false;
        public bool HasSessionFlag { get; internal set; } = false;
        public string AnonymousId { get; internal set; } = string.Empty;
        public bool IsFromLogin { get; internal set; } = false;
        public bool IsInfoPrivacyAccepted { get; internal set; } = false;
        public PecVerificationStatuses PECVerificationStatus { get; internal set; } = PecVerificationStatuses.None;

        public SessionInfo() : base() { }
    }

}

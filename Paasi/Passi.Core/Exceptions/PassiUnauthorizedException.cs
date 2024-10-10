using Passi.Core.Domain.Const;

namespace Passi.Core.Exceptions
{
    enum Reason
    {
        InvalidSessionCookie,
        SessionUserIdDifferentProfileUserId,
        SessionExpiredForTimeout,
        SessionExpiredForMaximumTime,
        SessionNotFound,
        MyInpsHalfPinNeeded,
        ProfileNotFound,
        ProfileSwitchNeeded,
        LevelSwitchNeeded,
        TimeSlotClosed,
        DelegationNotFound,
        ConventionNotFound,
        ConventionExpired,
        InvalidParameter,
        ServiceUnavailable,
        SessionTokenNotFound,
        InvalidSessionToken,
        Unknown = 99
    }

    [Serializable]
    class PassiUnauthorizedException : PassiException
    {
        public Uri RedirectUrl { get; private set; }
        public Reason Reason { get; private set; }
        public bool ClearExternalInfo { get; private set; }
        public bool ClearAll { get; private set; }

        public PassiUnauthorizedException(Uri url, Reason reason, bool clean = false) : base()
        {
            this.RedirectUrl = url;
            this.Reason = reason;
            this.ClearExternalInfo = clean;
            this.ClearAll = clean;
        }

        public PassiUnauthorizedException(Uri url, ErrorCodes code, Reason reason, bool clean = false) : this(url, reason, clean)
        {
            url = url.AddToQueryString(Keys.ErrorMessage, ((int)code).ToString());
            this.RedirectUrl = url;
        }

    }
}

namespace Passi.Core.Application.Options
{
    class UrlOptions
    {
        public Uri Logout { get; set; } = UriExtensions.Default;
        public Uri ErrorPage { get; set; } = UriExtensions.Default;
        public Uri ChangePin { get; set; } = UriExtensions.Default;
        public Uri ChangeContacts { get; set; } = UriExtensions.Default;
        public Uri SwitchProfile { get; set; } = UriExtensions.Default;
        public Uri PassiWeb { get; set; } = UriExtensions.Default;
        public Uri PassiWebCns { get; set; } = UriExtensions.Default;
        public Uri PassiWebOtp { get; set; } = UriExtensions.Default;
        public Uri PassiWebI { get; set; } = UriExtensions.Default;
    }
}

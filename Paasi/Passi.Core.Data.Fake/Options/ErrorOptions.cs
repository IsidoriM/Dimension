using Passi.Core.Exceptions;

namespace Passi.Core.Store.Fake.Options
{
    class ErrorOptions
    {
        public string Url { get; set; } = string.Empty;
        public Reason Reason { get; set; } = Reason.InvalidSessionCookie;
    }
}
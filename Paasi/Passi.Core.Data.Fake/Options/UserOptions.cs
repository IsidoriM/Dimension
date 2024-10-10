using Passi.Core.Domain.Const;

namespace Passi.Core.Store.Fake.Options
{
    internal class UserOptions
    {
        public const string SectionName = "User";

        public string? UserId { get; set; }
        public string? FiscalCode { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Gender { get; set; }
        public string? Email { get; set; }
        public string? PEC { get; set; }
        public ProfileOptions Profile { get; set; } = new ProfileOptions();
    }
}

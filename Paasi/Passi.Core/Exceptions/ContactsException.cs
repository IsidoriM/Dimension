using Passi.Core.Domain.Const;

namespace Passi.Core.Exceptions
{
    class ContactsException : Exception
    {
        public ContactsException(Outcomes outcome, string fiscalCode, string? message) : base(message)
        {
            Outcome = outcome;
            Title = "Contatti personali di " + fiscalCode;
        }

        public ContactsException(Outcomes outcome, string? message) : base(message)
        {
            Outcome = outcome;
            Title = "Contatti personali";
        }

        public string Title { get; set; }

        public Outcomes Outcome { get; }
    }
}


namespace Passi.Core.Domain.Const
{
    enum PecVerificationStatuses
    {
        None = -1,
        Deleted = 0,
        ConfirmOrUpdate = 1,
        Validated = 2,
        OptionalCodeError = 9,
        OptionalCodeEvaluation = 10,
        MandatoryCodeEvaluation = 11,
        OptionalCodeErorrWriteLog = 12
    }

    static class PecVerificationStatusesExtensions
    {
        public static PecVerificationStatuses ToVerificationStatuses(this string value)
        {
            return value switch
            {
                "0" => PecVerificationStatuses.Deleted,
                "1" => PecVerificationStatuses.ConfirmOrUpdate,
                "2" => PecVerificationStatuses.Validated,
                "9" => PecVerificationStatuses.OptionalCodeError,
                "10" => PecVerificationStatuses.OptionalCodeEvaluation,
                "11" => PecVerificationStatuses.MandatoryCodeEvaluation,
                "12" => PecVerificationStatuses.OptionalCodeErorrWriteLog,
                _ => PecVerificationStatuses.None,
            };
        }

        public static string ToVerificationString(this PecVerificationStatuses value)
        {
            return value switch
            {
                PecVerificationStatuses.Deleted => "0",
                PecVerificationStatuses.ConfirmOrUpdate => "1",
                PecVerificationStatuses.Validated => "2",
                PecVerificationStatuses.OptionalCodeError => "9",
                PecVerificationStatuses.OptionalCodeEvaluation => "10",
                PecVerificationStatuses.MandatoryCodeEvaluation => "11",
                PecVerificationStatuses.OptionalCodeErorrWriteLog => "12",
                _ => "",
            };
        }
    }
}

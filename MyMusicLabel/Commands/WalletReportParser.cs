namespace MyMusicLabel.Commands;

internal static class WalletReportParser
{
    public static string? TryParseRequest(
        IReadOnlyList<string> args,
        string report,
        out DateOnly startDate,
        out DateOnly endDate
    )
    {
        startDate = default;
        endDate = default;
        if (args.Count != 5 || !args[1].Equals("total", StringComparison.OrdinalIgnoreCase)
                             || !args[2].Equals(report, StringComparison.OrdinalIgnoreCase))
            return $"Укажите период: wallet total {report} <дд.ММ.гггг> <дд.ММ.гггг>";

        return DateParsing.TryParsePeriod(args, 3, 4, out startDate, out endDate);
    }
}

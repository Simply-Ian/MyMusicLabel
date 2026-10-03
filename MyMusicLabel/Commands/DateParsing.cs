using System.Globalization;

namespace MyMusicLabel.Commands;

internal static class DateParsing
{
    public static string? TryParsePeriod(
        IReadOnlyList<string> args,
        int startArgIndex,
        int endArgIndex,
        out DateOnly startDate,
        out DateOnly endDate
    )
    {
        startDate = default;
        endDate = default;
        var culture = CultureInfo.GetCultureInfo("ru-RU");

        if (!TryParseDate(args[startArgIndex], culture, out startDate))
            return $"Не удалось прочитать дату \"{args[startArgIndex]}\". Укажите дату в формате дд.ММ.гггг";

        if (!TryParseDate(args[endArgIndex], culture, out endDate))
            return $"Не удалось прочитать дату \"{args[endArgIndex]}\". Укажите дату в формате дд.ММ.гггг";

        if (startDate > endDate)
            return "Дата начала периода не может быть позже даты окончания";

        return null;
    }

    private static bool TryParseDate(string arg, CultureInfo culture, out DateOnly date)
    {
        try
        {
            date = DateOnly.Parse(arg, culture);
            return true;
        }
        catch (FormatException)
        {
            date = default;
            return false;
        }
    }
}

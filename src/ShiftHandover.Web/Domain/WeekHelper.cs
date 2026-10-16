using System.Globalization;

namespace ShiftHandover.Web.Domain;

public static class WeekHelper
{
    private static readonly CultureInfo Swedish = new("sv-SE");

    // Monday of the week the date belongs to
    public static DateOnly StartOfWeek(DateOnly date) =>
        date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    // ISO week number, the one used in Sweden
    public static int WeekNumber(DateOnly date) =>
        ISOWeek.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue));

    // Monday to Friday of the week
    public static IEnumerable<DateOnly> Workdays(DateOnly weekStart) =>
        Enumerable.Range(0, 5).Select(weekStart.AddDays);

    // Saturday and Sunday count as the Friday before
    public static DateOnly LastWorkday(DateOnly date) => date.DayOfWeek switch
    {
        DayOfWeek.Saturday => date.AddDays(-1),
        DayOfWeek.Sunday => date.AddDays(-2),
        _ => date
    };

    // "Måndag 12 oktober"
    public static string SwedishDayName(DateOnly date)
    {
        var text = date.ToString("dddd d MMMM", Swedish);
        return char.ToUpper(text[0]) + text[1..];
    }
}
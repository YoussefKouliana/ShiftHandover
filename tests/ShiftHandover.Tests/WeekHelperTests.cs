using ShiftHandover.Web.Domain;

namespace ShiftHandover.Tests;

public class WeekHelperTests
{
    [Theory]
    [InlineData(12)] // Monday
    [InlineData(15)] // Thursday
    [InlineData(18)] // Sunday
    public void StartOfWeek_returns_the_monday(int day)
    {
        var date = new DateOnly(2026, 10, day);

        Assert.Equal(new DateOnly(2026, 10, 12), WeekHelper.StartOfWeek(date));
    }

    [Fact]
    public void WeekNumber_uses_iso_weeks()
    {
        Assert.Equal(42, WeekHelper.WeekNumber(new DateOnly(2026, 10, 12)));
    }

    [Fact]
    public void LastWorkday_of_a_saturday_is_the_friday()
    {
        Assert.Equal(new DateOnly(2026, 10, 16), WeekHelper.LastWorkday(new DateOnly(2026, 10, 17)));
    }

    [Fact]
    public void SwedishDayName_is_capitalized_swedish()
    {
        Assert.Equal("Måndag 12 oktober", WeekHelper.SwedishDayName(new DateOnly(2026, 10, 12)));
    }
}
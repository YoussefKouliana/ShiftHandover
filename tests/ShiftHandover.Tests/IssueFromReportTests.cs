using ShiftHandover.Web.Domain;

namespace ShiftHandover.Tests;

public class IssueFromReportTests
{
    private static readonly DateOnly Date = new(2026, 10, 16);

    private static ShiftReport Report(MachineStatus machine, ToolStatus tool, QualityStatus quality, string? comment = "Test") =>
        ShiftReport.Create(Date, Shift.Day,
            new Machine { Name = "Maskin 2" },
            new Tool { Number = "2202" },
            new Operator { Name = "Test" },
            machine, tool, quality, comment);

    [Fact]
    public void Normal_report_creates_no_issue()
    {
        var report = Report(MachineStatus.ReadyToRun, ToolStatus.GoodCondition, QualityStatus.Approved, null);

        Assert.Empty(Issue.FromShiftReport(report));
    }

    [Fact]
    public void Tool_crash_creates_issue_for_toolmakers()
    {
        var report = Report(MachineStatus.NotOk, ToolStatus.Crash, QualityStatus.NeedsQualityCheck, "Sprucken stans");

        var issue = Assert.Single(Issue.FromShiftReport(report));

        Assert.Equal(Team.Toolmaker, issue.ResponsibleTeam);
        Assert.Equal(IssueCategory.ToolDamage, issue.Category);
        Assert.Equal("Krasch på verktyg 2202", issue.Title);
        Assert.Equal("Sprucken stans", issue.ActionTaken);
        Assert.Same(report, issue.SourceReport);
    }

    [Fact]
    public void Blocked_quality_creates_issue_for_quality()
    {
        var report = Report(MachineStatus.ReadyToRun, ToolStatus.GoodCondition, QualityStatus.Blocked, "Repor");

        var issue = Assert.Single(Issue.FromShiftReport(report));

        Assert.Equal(Team.Quality, issue.ResponsibleTeam);
        Assert.Equal("Kvalitet spärrad på Maskin 2", issue.Title);
    }

    [Fact]
    public void Tool_and_quality_problem_creates_two_issues()
    {
        var report = Report(MachineStatus.NotOk, ToolStatus.Crash, QualityStatus.Blocked, "Krasch och spärrat");

        Assert.Equal(2, Issue.FromShiftReport(report).Count);
    }

    [Fact]
    public void Worn_tool_creates_no_issue()
    {
        var report = Report(MachineStatus.ReadyToRun, ToolStatus.Worn, QualityStatus.Approved);

        Assert.Empty(Issue.FromShiftReport(report));
    }
}
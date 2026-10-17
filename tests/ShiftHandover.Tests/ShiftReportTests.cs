using ShiftHandover.Web.Domain;

namespace ShiftHandover.Tests;

public class ShiftReportTests
{
    private static readonly DateOnly Date = new(2026, 10, 16);

    private static (Machine machine, Tool tool, Operator reporter) Setup() =>
        (new Machine { Name = "Maskin 2" },
         new Tool { Number = "V-1" },
         new Operator { Name = "Test" });

    [Fact]
    public void Tool_crash_requires_machine_not_ok()
    {
        var (machine, tool, reporter) = Setup();

        Assert.Throws<InvalidOperationException>(() => ShiftReport.Create(Date, Shift.Day, machine, tool, reporter,
            MachineStatus.ReadyToRun, ToolStatus.Crash, QualityStatus.Approved, "Krasch"));
    }

    [Fact]
    public void Not_ok_with_tool_crash_is_accepted_without_comment()
    {
        var (machine, tool, reporter) = Setup();

        var report = ShiftReport.Create(Date, Shift.Day, machine, tool, reporter,
            MachineStatus.NotOk, ToolStatus.Crash, QualityStatus.NeedsQualityCheck, null);

        Assert.Equal(MachineStatus.NotOk, report.MachineStatus);
    }

    [Fact]
    public void Not_ok_with_good_tool_requires_a_comment()
    {
        var (machine, tool, reporter) = Setup();

        Assert.Throws<InvalidOperationException>(() => ShiftReport.Create(Date, Shift.Day, machine, tool, reporter,
            MachineStatus.NotOk, ToolStatus.GoodCondition, QualityStatus.Approved, null));
    }

    [Fact]
    public void Empty_machine_cannot_have_a_tool()
    {
        var (machine, tool, reporter) = Setup();

        Assert.Throws<InvalidOperationException>(() => ShiftReport.Create(Date, Shift.Day, machine, tool, reporter,
            MachineStatus.Empty, null, null, null));
    }

    [Fact]
    public void Empty_machine_without_tool_is_accepted()
    {
        var (machine, _, reporter) = Setup();

        var report = ShiftReport.Create(Date, Shift.Day, machine, null, reporter,
            MachineStatus.Empty, null, null, null);

        Assert.Null(report.Tool);
        Assert.Null(report.ToolStatus);
    }

    [Fact]
    public void Machine_in_use_requires_a_tool()
    {
        var (machine, _, reporter) = Setup();

        Assert.Throws<InvalidOperationException>(() => ShiftReport.Create(Date, Shift.Day, machine, null, reporter,
            MachineStatus.ReadyToRun, ToolStatus.GoodCondition, QualityStatus.Approved, null));
    }

    [Theory]
    [InlineData(QualityStatus.NotOk)]
    [InlineData(QualityStatus.Blocked)]
    public void Bad_quality_requires_a_comment(QualityStatus quality)
    {
        var (machine, tool, reporter) = Setup();

        Assert.Throws<InvalidOperationException>(() => ShiftReport.Create(Date, Shift.Day, machine, tool, reporter,
            MachineStatus.ReadyToRun, ToolStatus.GoodCondition, quality, " "));
    }

    [Fact]
    public void Creating_a_report_mounts_the_tool_on_the_machine()
    {
        var (machine, tool, reporter) = Setup();

        ShiftReport.Create(Date, Shift.Day, machine, tool, reporter,
            MachineStatus.ReadyToRun, ToolStatus.GoodCondition, QualityStatus.Approved, null);

        Assert.Same(machine, tool.CurrentMachine);
    }
}
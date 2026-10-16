using ShiftHandover.Web.Domain;

namespace ShiftHandover.Tests;

public class ShiftReportTests
{
    private static (Machine machine, Tool tool, Operator reporter) Setup() =>
        (new Machine { Name = "Maskin 2" },
         new Tool { Number = "V-1", Name = "Test", Part = new Part { Number = "D-1", Name = "Test" } },
         new Operator { Name = "Test" });

    [Fact]
    public void Machine_cannot_run_with_damaged_tool()
    {
        var (machine, tool, reporter) = Setup();

        Assert.Throws<InvalidOperationException>(() => ShiftReport.Create(
            new DateOnly(2026, 10, 16), Shift.Day, machine, tool, reporter,
            MachineStatus.Running, ToolStatus.Damaged, QualityStatus.Approved, null));
    }

    [Fact]
    public void Blocked_quality_requires_a_comment()
    {
        var (machine, tool, reporter) = Setup();

        Assert.Throws<InvalidOperationException>(() => ShiftReport.Create(
            new DateOnly(2026, 10, 16), Shift.Day, machine, tool, reporter,
            MachineStatus.Stopped, ToolStatus.Ok, QualityStatus.Blocked, "  "));
    }

    [Fact]
    public void Creating_a_report_mounts_the_tool_on_the_machine()
    {
        var (machine, tool, reporter) = Setup();

        ShiftReport.Create(
            new DateOnly(2026, 10, 16), Shift.Day, machine, tool, reporter,
            MachineStatus.Running, ToolStatus.Ok, QualityStatus.Approved, null);

        Assert.Same(machine, tool.CurrentMachine);
    }
}
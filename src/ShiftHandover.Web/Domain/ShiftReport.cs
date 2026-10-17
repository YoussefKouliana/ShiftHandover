using ToolState = ShiftHandover.Web.Domain.ToolStatus;
using QualityState = ShiftHandover.Web.Domain.QualityStatus;

namespace ShiftHandover.Web.Domain;

public class ShiftReport
{
    public int Id { get; private set; }
    public DateOnly Date { get; private set; }
    public Shift Shift { get; private set; }

    public int MachineId { get; private set; }
    public Machine Machine { get; private set; } = null!;

    // Null when the machine is Tom (no tool mounted)
    public int? ToolId { get; private set; }
    public Tool? Tool { get; private set; }

    public int OperatorId { get; private set; }
    public Operator Operator { get; private set; } = null!;

    public MachineStatus MachineStatus { get; private set; }
    public ToolStatus? ToolStatus { get; private set; }
    public QualityStatus? QualityStatus { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedUtc { get; private set; }

    // Needed by EF Core
    private ShiftReport() { }

    // The only way to create a report, so the rules are always checked
    public static ShiftReport Create(
        DateOnly date, Shift shift, Machine machine, Tool? tool, Operator reporter,
        MachineStatus machineStatus, ToolState? toolStatus, QualityState? qualityStatus,
        string? comment, DateTime? nowUtc = null)
    {
        var hasComment = !string.IsNullOrWhiteSpace(comment);

        if (machineStatus == MachineStatus.Empty)
        {
            // Rule 3: Tom means no tool, so nothing to report about tool or quality
            if (tool is not null)
                throw new InvalidOperationException("En tom maskin kan inte ha ett verktyg.");

            toolStatus = null;
            qualityStatus = null;
        }
        else
        {
            // Rule 4: a machine that is not Tom needs tool, tool status and quality
            if (tool is null)
                throw new InvalidOperationException("Välj verktyg, eller sätt maskinstatus till Tom.");
            if (toolStatus is null)
                throw new InvalidOperationException("Välj verktygsstatus.");
            if (qualityStatus is null)
                throw new InvalidOperationException("Välj kvalitetsstatus.");
        }

        // Rule 1: a tool crash means the machine is not okay
        if (toolStatus == ToolState.Crash && machineStatus != MachineStatus.NotOk)
            throw new InvalidOperationException("Vid verktygskrasch måste maskinstatus vara Ej okej.");

        // Rule 2: Ej okej needs a reason, either a tool problem or a comment
        var toolHasProblem = toolStatus is not null && toolStatus != ToolState.GoodCondition;
        if (machineStatus == MachineStatus.NotOk && !toolHasProblem && !hasComment)
            throw new InvalidOperationException("Ange orsaken till Ej okej: en verktygsstatus eller en kommentar.");

        // Rule 5: bad quality needs a comment
        var badQuality = qualityStatus is QualityState.NotOk or QualityState.Blocked;
        if (badQuality && !hasComment)
            throw new InvalidOperationException("Ange en kommentar när kvaliteten är Ej okej eller Spärrad.");

        // Reporting a tool on a machine means it is mounted there now
        tool?.MountOn(machine);

        return new ShiftReport
        {
            Date = date,
            Shift = shift,
            Machine = machine,
            Tool = tool,
            Operator = reporter,
            MachineStatus = machineStatus,
            ToolStatus = toolStatus,
            QualityStatus = qualityStatus,
            Comment = hasComment ? comment!.Trim() : null,
            CreatedUtc = nowUtc ?? DateTime.UtcNow
        };
    }
}
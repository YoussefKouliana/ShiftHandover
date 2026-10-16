namespace ShiftHandover.Web.Domain;

public class ShiftReport
{
    public int Id { get; private set; }
    public DateOnly Date { get; private set; }
    public Shift Shift { get; private set; }

    public int MachineId { get; private set; }
    public Machine Machine { get; private set; } = null!;

    public int ToolId { get; private set; }
    public Tool Tool { get; private set; } = null!;

    public int OperatorId { get; private set; }
    public Operator Operator { get; private set; } = null!;

    public MachineStatus MachineStatus { get; private set; }
    public ToolStatus ToolStatus { get; private set; }
    public QualityStatus QualityStatus { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedUtc { get; private set; }

    // Needed by EF Core
    private ShiftReport() { }

    // The only way to create a report, so the rules are always checked
    public static ShiftReport Create(
        DateOnly date, Shift shift, Machine machine, Tool tool, Operator reporter,
        MachineStatus machineStatus, ToolStatus toolStatus, QualityStatus qualityStatus,
        string? comment, DateTime? nowUtc = null)
    {
        if (toolStatus == ToolStatus.Damaged && machineStatus == MachineStatus.Running)
            throw new InvalidOperationException("Maskinen kan inte köra med ett skadat verktyg.");

        if (qualityStatus == QualityStatus.Blocked && string.IsNullOrWhiteSpace(comment))
            throw new InvalidOperationException("Ange en kommentar när detaljer spärras.");

        // Reporting a tool on a machine means it is mounted there now
        tool.MountOn(machine);

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
            Comment = comment,
            CreatedUtc = nowUtc ?? DateTime.UtcNow
        };
    }
}
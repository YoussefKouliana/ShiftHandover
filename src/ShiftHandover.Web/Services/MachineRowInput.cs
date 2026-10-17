using ShiftHandover.Web.Domain;

namespace ShiftHandover.Web.Services;

// One machine's row in the shift report form
public class MachineRowInput
{
    public int MachineId { get; set; }
    public string MachineName { get; set; } = string.Empty;

    public int? ToolId { get; set; }
    public MachineStatus? MachineStatus { get; set; }
    public ToolStatus? ToolStatus { get; set; }
    public QualityStatus? QualityStatus { get; set; }
    public int? OperatorId { get; set; }
    public string? Comment { get; set; }

    // Set when this machine is already reported for the shift
    public ReportRow? Saved { get; set; }

    // Shown under the row when saving fails
    public string? Error { get; set; }

    public bool IsReported => Saved is not null;

    // The tool is prefilled, so only the other fields count as "filled in"
    public bool HasInput =>
        MachineStatus is not null
        || ToolStatus is not null
        || QualityStatus is not null
        || OperatorId is not null
        || !string.IsNullOrWhiteSpace(Comment);
}
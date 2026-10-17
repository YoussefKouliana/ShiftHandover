namespace ShiftHandover.Web.Domain;

public class Issue
{
    public int Id { get; set; }
    public int MachineId { get; set; }
    public Machine Machine { get; set; } = null!;
    public IssueCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ActionTaken { get; set; }
    public string? FollowUp { get; set; }
    public Team ResponsibleTeam { get; set; }
    public IssueStatus Status { get; private set; } = IssueStatus.Open;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; private set; }
    public List<StatusChange> History { get; } = new();
        // Optional: which tool the issue concerns
    public int? ToolId { get; set; }
    public Tool? Tool { get; set; }
        // Set when the issue was created automatically from a shift report
    public int? SourceReportId { get; set; }
    public ShiftReport? SourceReport { get; set; }

    public bool IsClosed => Status == IssueStatus.Verified;

    // All status changes go through this method, so the rules live in one place
    // and every change is recorded in the history.
    public void ChangeStatus(IssueStatus newStatus, string? comment = null, DateTime? nowUtc = null)
    {
        if (newStatus == Status)
            throw new InvalidOperationException("Ärendet har redan den statusen.");

        if (newStatus == IssueStatus.Verified && Status != IssueStatus.Fixed)
            throw new InvalidOperationException("Endast åtgärdade ärenden kan verifieras.");

        if (IsClosed && newStatus != IssueStatus.Open)
            throw new InvalidOperationException("Ett verifierat ärende kan bara öppnas igen.");

        var now = nowUtc ?? DateTime.UtcNow;

        History.Add(new StatusChange
        {
            FromStatus = Status,
            ToStatus = newStatus,
            ChangedUtc = now,
            Comment = comment
        });

        Status = newStatus;
        UpdatedUtc = now;
    }
        // Problems in a shift report become issues on the board.
    // Tool problems go to the toolmakers, bad quality goes to quality.
    public static List<Issue> FromShiftReport(ShiftReport report)
    {
        var issues = new List<Issue>();
        var origin = $"Skapad automatiskt från {report.Shift.ToSwedish().ToLowerInvariant()} {report.Date:yyyy-MM-dd}.";

        if (report.ToolStatus is ToolStatus.Crash or ToolStatus.ToolProblem && report.Tool is not null)
        {
            issues.Add(new Issue
            {
                Machine = report.Machine,
                Tool = report.Tool,
                SourceReport = report,
                Category = IssueCategory.ToolDamage,
                ResponsibleTeam = Team.Toolmaker,
                Title = $"{report.ToolStatus.Value.ToSwedish()} på verktyg {report.Tool.Number}",
                ActionTaken = report.Comment,
                FollowUp = origin,
                CreatedUtc = report.CreatedUtc
            });
        }

        if (report.QualityStatus is QualityStatus.NotOk or QualityStatus.Blocked)
        {
            issues.Add(new Issue
            {
                Machine = report.Machine,
                Tool = report.Tool,
                SourceReport = report,
                Category = IssueCategory.QualityDeviation,
                ResponsibleTeam = Team.Quality,
                Title = $"Kvalitet {report.QualityStatus.Value.ToSwedish().ToLowerInvariant()} på {report.Machine.Name}",
                ActionTaken = report.Comment,
                FollowUp = origin,
                CreatedUtc = report.CreatedUtc
            });
        }

        return issues;
    }
}
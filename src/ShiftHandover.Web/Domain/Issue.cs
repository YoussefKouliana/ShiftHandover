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
}
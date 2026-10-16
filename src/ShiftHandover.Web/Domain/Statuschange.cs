namespace ShiftHandover.Web.Domain;

public class StatusChange
{
    public int Id { get; set; }
    public int IssueId { get; set; }
    public IssueStatus FromStatus { get; set; }
    public IssueStatus ToStatus { get; set; }
    public DateTime ChangedUtc { get; set; }
    public string? Comment { get; set; }
}
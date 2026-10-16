using ShiftHandover.Web.Domain;

namespace ShiftHandover.Tests;

public class IssueTests
{
    [Fact]
    public void New_issue_starts_as_open()
    {
        var issue = new Issue();

        Assert.Equal(IssueStatus.Open, issue.Status);
        Assert.Empty(issue.History);
    }

    [Fact]
    public void ChangeStatus_records_history()
    {
        var issue = new Issue();
        var now = new DateTime(2026, 10, 16, 6, 0, 0, DateTimeKind.Utc);

        issue.ChangeStatus(IssueStatus.InProgress, "VM tittar på det", now);

        Assert.Equal(IssueStatus.InProgress, issue.Status);
        Assert.Single(issue.History);
        Assert.Equal(IssueStatus.Open, issue.History[0].FromStatus);
        Assert.Equal(IssueStatus.InProgress, issue.History[0].ToStatus);
        Assert.Equal(now, issue.UpdatedUtc);
    }

    [Fact]
    public void Cannot_verify_issue_that_is_not_fixed()
    {
        var issue = new Issue();

        Assert.Throws<InvalidOperationException>(() => issue.ChangeStatus(IssueStatus.Verified));
    }

    [Fact]
    public void Fixed_issue_can_be_verified()
    {
        var issue = new Issue();
        issue.ChangeStatus(IssueStatus.Fixed);

        issue.ChangeStatus(IssueStatus.Verified);

        Assert.True(issue.IsClosed);
    }

    [Fact]
    public void Verified_issue_can_only_be_reopened()
    {
        var issue = new Issue();
        issue.ChangeStatus(IssueStatus.Fixed);
        issue.ChangeStatus(IssueStatus.Verified);

        Assert.Throws<InvalidOperationException>(() => issue.ChangeStatus(IssueStatus.InProgress));

        issue.ChangeStatus(IssueStatus.Open, "Problemet kom tillbaka");
        Assert.Equal(IssueStatus.Open, issue.Status);
    }

    [Fact]
    public void Changing_to_same_status_is_rejected()
    {
        var issue = new Issue();

        Assert.Throws<InvalidOperationException>(() => issue.ChangeStatus(IssueStatus.Open));
    }
}
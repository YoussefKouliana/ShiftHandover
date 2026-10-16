 using Microsoft.EntityFrameworkCore;
using ShiftHandover.Web.Data;
using ShiftHandover.Web.Domain;

namespace ShiftHandover.Web.Services;

// What the board needs to show for one issue, nothing more
public record IssueCard(
    int Id,
    string MachineName,
    string Title,
    IssueCategory Category,
    Team ResponsibleTeam,
    IssueStatus Status,
    DateTime CreatedUtc,
    bool IsRecurring);

public class HandoverService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<List<IssueCard>> GetActiveIssuesAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.Issues
            .Where(i => i.Status != IssueStatus.Verified)
            .OrderBy(i => i.CreatedUtc)
            .Select(i => new IssueCard(
                i.Id,
                i.Machine.Name,
                i.Title,
                i.Category,
                i.ResponsibleTeam,
                i.Status,
                i.CreatedUtc,
                // Recurring = it was verified once, and then reopened
                i.History.Any(h => h.FromStatus == IssueStatus.Verified)))
            .ToListAsync();
    }

    public async Task<int> CountVerifiedAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Issues.CountAsync(i => i.Status == IssueStatus.Verified);
    }
}
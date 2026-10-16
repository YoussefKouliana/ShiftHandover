using Microsoft.EntityFrameworkCore;
using ShiftHandover.Web.Data;
using ShiftHandover.Web.Domain;

namespace ShiftHandover.Web.Services;

public record MachineItem(int Id, string Name);

public record ReportRow(
    DateOnly Date,
    Shift Shift,
    int MachineId,
    string ToolText,
    string PartText,
    MachineStatus MachineStatus,
    ToolStatus ToolStatus,
    QualityStatus QualityStatus,
    string OperatorName,
    string? Comment);

public class ShiftReportService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<List<MachineItem>> GetMachinesAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.Machines
            .OrderBy(m => m.Id)
            .Select(m => new MachineItem(m.Id, m.Name))
            .ToListAsync();
    }

    public async Task<List<ReportRow>> GetReportsAsync(DateOnly from, DateOnly to)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.ShiftReports
            .Where(r => r.Date >= from && r.Date <= to)
            .Select(r => new ReportRow(
                r.Date,
                r.Shift,
                r.MachineId,
                r.Tool.Number + " " + r.Tool.Name,
                r.Tool.Part.Number + " " + r.Tool.Part.Name,
                r.MachineStatus,
                r.ToolStatus,
                r.QualityStatus,
                r.Operator.Name,
                r.Comment))
            .ToListAsync();
    }
}
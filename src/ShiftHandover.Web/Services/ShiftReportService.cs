using Microsoft.EntityFrameworkCore;
using ShiftHandover.Web.Data;
using ShiftHandover.Web.Domain;

namespace ShiftHandover.Web.Services;

public record MachineItem(int Id, string Name);

public record ReportRow(
    DateOnly Date,
    Shift Shift,
    int MachineId,
    string? ToolNumber,
    MachineStatus MachineStatus,
    ToolStatus? ToolStatus,
    QualityStatus? QualityStatus,
    string OperatorName,
    string? Comment);

// Used by the shift report form (Part 2)
public record ToolOption(int Id, string Number, int? CurrentMachineId, string? CurrentMachineName);

public record OperatorOption(int Id, string Name);

public record FormOptions(List<MachineItem> Machines, List<ToolOption> Tools, List<OperatorOption> Operators);

public record ShiftSaveResult(bool Success, int SavedCount, string? Error);
public record SaveResult(bool Success, string? Error);

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
                r.Tool != null ? r.Tool.Number : null,
                r.MachineStatus,
                r.ToolStatus,
                r.QualityStatus,
                r.Operator.Name,
                r.Comment))
            .ToListAsync();
    }

    public async Task<FormOptions> GetFormOptionsAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var machines = await db.Machines
            .OrderBy(m => m.Id)
            .Select(m => new MachineItem(m.Id, m.Name))
            .ToListAsync();

        var tools = await db.Tools
            .OrderBy(t => t.Number)
            .Select(t => new ToolOption(
                t.Id,
                t.Number,
                t.CurrentMachineId,
                t.CurrentMachine != null ? t.CurrentMachine.Name : null))
            .ToListAsync();

        // Only active operators can be chosen
        var operators = await db.Operators
            .Where(o => o.IsActive)
            .OrderBy(o => o.Name)
            .Select(o => new OperatorOption(o.Id, o.Name))
            .ToListAsync();

        return new FormOptions(machines, tools, operators);
    }
      public async Task<List<MachineRowInput>> GetShiftRowsAsync(DateOnly date, Shift shift, FormOptions options)
    {
        var reports = await GetReportsAsync(date, date);

        return options.Machines
            .Select(m => new MachineRowInput
            {
                MachineId = m.Id,
                MachineName = m.Name,
                // Start with the tool that is mounted in this machine now
                ToolId = options.Tools.FirstOrDefault(t => t.CurrentMachineId == m.Id)?.Id,
                Saved = reports.FirstOrDefault(r => r.Shift == shift && r.MachineId == m.Id)
            })
            .ToList();
    }

    public async Task<ShiftSaveResult> SaveShiftAsync(DateOnly date, Shift shift, List<MachineRowInput> rows)
    {
        foreach (var row in rows)
            row.Error = null;

        // Rows nobody touched are skipped
        var filled = rows.Where(r => !r.IsReported && r.HasInput).ToList();
        if (filled.Count == 0)
            return new ShiftSaveResult(false, 0, "Fyll i minst en maskin.");

        // The same tool cannot be in two machines at once
        var sameTool = filled
            .Where(r => r.ToolId is not null)
            .GroupBy(r => r.ToolId)
            .FirstOrDefault(g => g.Count() > 1);
        if (sameTool is not null)
        {
            foreach (var row in sameTool)
                row.Error = "Samma verktyg är valt för flera maskiner.";
            return new ShiftSaveResult(false, 0, "Inget sparades. Rätta de markerade raderna.");
        }

        await using var db = await dbFactory.CreateDbContextAsync();

        var alreadyReported = await db.ShiftReports
            .Where(r => r.Date == date && r.Shift == shift)
            .Select(r => r.MachineId)
            .ToListAsync();
        var machines = await db.Machines.ToDictionaryAsync(m => m.Id);
        var tools = await db.Tools.ToListAsync(); // few tools, so loading all is fine
        var operators = await db.Operators.ToDictionaryAsync(o => o.Id);

        var reports = new List<ShiftReport>();

        foreach (var row in filled)
        {
            if (alreadyReported.Contains(row.MachineId))
            {
                row.Error = "Maskinen är redan rapporterad för det här skiftet.";
                continue;
            }
            if (row.OperatorId is null)
            {
                row.Error = "Välj operatör.";
                continue;
            }
            if (row.MachineStatus is null)
            {
                row.Error = "Välj maskinstatus.";
                continue;
            }

            var machine = machines[row.MachineId];
            var tool = row.ToolId is null ? null : tools.FirstOrDefault(t => t.Id == row.ToolId);

            try
            {
                // A machine holds one tool at a time: other tools in it go back to the tool room
                foreach (var other in tools.Where(t => t.CurrentMachine == machine && t != tool))
                    other.Unmount();

                reports.Add(ShiftReport.Create(
                    date, shift, machine, tool, operators[row.OperatorId.Value],
                    row.MachineStatus.Value, row.ToolStatus, row.QualityStatus, row.Comment));
            }
            catch (InvalidOperationException ex)
            {
                // A business rule was broken: show its message on this row
                row.Error = ex.Message;
            }
        }

        // All or nothing: one bad row means nothing is saved
        if (filled.Any(r => r.Error is not null))
            return new ShiftSaveResult(false, 0, "Inget sparades. Rätta de markerade raderna.");

        db.ShiftReports.AddRange(reports);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return new ShiftSaveResult(false, 0,
                "Skiftet kunde inte sparas. Någon kan ha rapporterat samtidigt, ladda om sidan.");
        }

        return new ShiftSaveResult(true, reports.Count, null);
    }
     }

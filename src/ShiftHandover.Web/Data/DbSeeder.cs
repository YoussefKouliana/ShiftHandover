using Microsoft.EntityFrameworkCore;
using ShiftHandover.Web.Domain;

namespace ShiftHandover.Web.Data;

// Synthetic demo data. Inspired by typical shift handover patterns,
// but contains no real production data, part numbers or names.
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, DateTime? nowUtc = null)
    {
        // Only seed an empty database
        if (await db.Machines.AnyAsync())
            return;

        var now = nowUtc ?? DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        DateTime DaysAgo(double days) => now.AddDays(-days);

        // Machines
        var machines = Enumerable.Range(1, 6)
            .Select(n => new Machine { Name = $"Maskin {n}" })
            .ToList();
        db.Machines.AddRange(machines);
        Machine M(int number) => machines[number - 1];

                // Tools, identified only by number
        var tools = new List<Tool>();
        Tool T(string number)
        {
            var tool = new Tool { Number = number };
            tools.Add(tool);
            return tool;
        }

        var v1101 = T("1101");
        T("1102");
        T("2201");
        var v2202 = T("2202");
        var v3301 = T("3301");
        var v4401 = T("4401");
        var v5501 = T("5501");
        var v6601 = T("6601");
        T("6602");
        db.Tools.AddRange(tools);
        // Operators (made-up names)
        var anna = new Operator { Name = "Anna Lindqvist" };
        var erik = new Operator { Name = "Erik Johansson" };
        var sara = new Operator { Name = "Sara Nilsson" };
        var mikael = new Operator { Name = "Mikael Berg" };
        var johan = new Operator { Name = "Johan Ek", IsActive = false };
        db.Operators.AddRange(anna, erik, sara, mikael, johan);

        // Shift reports: generated history for last week and this week,
        // plus hand-written reports for the latest workday's morning shift
        var lastWorkday = WeekHelper.LastWorkday(today);
        var mountedTools = new Dictionary<int, Tool>
        {
            [1] = v1101,
            [2] = v2202,
            [3] = v3301,
            [4] = v4401,
            [5] = v5501,
            [6] = v6601
        };
        var activeOperators = new[] { anna, erik, sara, mikael };
        var random = new Random(42); // fixed seed: same demo data every time

        for (var date = WeekHelper.StartOfWeek(lastWorkday).AddDays(-7); date < lastWorkday; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;

            foreach (var shift in Enum.GetValues<Shift>())
            {
                foreach (var (number, tool) in mountedTools)
                {
                    var reporter = activeOperators[random.Next(activeOperators.Length)];
                    db.ShiftReports.Add(RandomReport(random, date, shift, M(number), tool, reporter));
                }
            }
        }

        // One example of each important situation
        db.ShiftReports.AddRange(
            ShiftReport.Create(lastWorkday, Shift.Day, M(1), v1101, anna,
                MachineStatus.ReadyToRun, ToolStatus.Worn, QualityStatus.NeedsQualityCheck,
                "Grad vid klippkant, verktygsmakare informerad.", DaysAgo(0.1)),
            ShiftReport.Create(lastWorkday, Shift.Day, M(2), v2202, sara,
                MachineStatus.NotOk, ToolStatus.Crash, QualityStatus.Blocked,
                "Verktygskrasch, sprucken stans. Detaljerna spärrade.", DaysAgo(0.1)),
            ShiftReport.Create(lastWorkday, Shift.Day, M(3), v3301, mikael,
                MachineStatus.ReadyToRun, ToolStatus.GoodCondition, QualityStatus.Approved, null, DaysAgo(0.1)),
            ShiftReport.Create(lastWorkday, Shift.Day, M(4), null, anna,
                MachineStatus.Empty, null, null,
                "Verktyget hos verktygsmakare.", DaysAgo(0.1)),
            ShiftReport.Create(lastWorkday, Shift.Day, M(5), v5501, erik,
                MachineStatus.HalfService, ToolStatus.WaitingForToolmaker, QualityStatus.MeasurementInProgress,
                "Väntar på verktygsmakare efter halvservice.", DaysAgo(0.1)),
            ShiftReport.Create(lastWorkday, Shift.Day, M(6), v6601, sara,
                MachineStatus.ReadyToRun, ToolStatus.GoodCondition, QualityStatus.Approved, null, DaysAgo(0.1)));

        // Maskin 4 is Tom, so its tool is back in the tool room
        v4401.Unmount();

        // Issues
        var issues = new List<Issue>();
        Issue Add(int machine, IssueCategory category, Team team, string title,
            string? action, string? followUp, double daysAgo, Tool? tool = null)
        {
            var issue = new Issue
            {
                Machine = M(machine),
                Tool = tool,
                Category = category,
                ResponsibleTeam = team,
                Title = title,
                ActionTaken = action,
                FollowUp = followUp,
                CreatedUtc = DaysAgo(daysAgo)
            };
            issues.Add(issue);
            return issue;
        }

        // Open
        Add(1, IssueCategory.Burr, Team.Toolmaker, "Grad vid klippkant på kavitet 2",
            "Graden noterad, berörda detaljer sorterade.",
            "VM bör åtgärda innan graden växer.", 2, v1101);
        Add(2, IssueCategory.Burr, Team.Toolmaker, "Grad på hålkanter, flera kaviteter",
            null, "Kontrollera verktyget och åtgärda graden.", 9, v2202);
        Add(6, IssueCategory.Mark, Team.Quality, "Repor på ytan efter trumling",
            "Detaljerna spärrade för bedömning.",
            "Kvalitet bedömer om detaljerna kan godkännas.", 2, v6601);

        // In progress
        Add(1, IssueCategory.Measurement, Team.Quality, "Radie på vänster kavitet nära toleransgränsen",
            "Ommätt med annan fixtur, godkänd men nära gränsen.",
            "Kvalitet följer upp nästa batch.", 3, v1101)
            .ChangeStatus(IssueStatus.InProgress, "Kvalitet mäter extra detaljer", DaysAgo(2));
        Add(3, IssueCategory.Measurement, Team.Quality, "Nytt mätupplägg under testperiod",
            "Detaljer körda och mätta, protokoll lämnat.",
            "Utvärdera efter testperioden.", 10, v3301)
            .ChangeStatus(IssueStatus.InProgress, "Resultaten ser lovande ut", DaysAgo(8));
        Add(5, IssueCategory.MachineFault, Team.Maintenance, "Planerad service av smörjsystemet",
            "Service påbörjad.", null, 5)
            .ChangeStatus(IssueStatus.InProgress, "UH har påbörjat servicen", DaysAgo(4));
        Add(2, IssueCategory.ToolDamage, Team.Toolmaker, "Sprucken stans upptäckt vid kontroll",
            "Maskinen stoppad.", "VM byter till ny stans.", 0.2, v2202)
            .ChangeStatus(IssueStatus.InProgress, "VM har hämtat verktyget", DaysAgo(0.1));

        // Waiting for another team
        Add(2, IssueCategory.MachineFault, Team.Toolmaker, "Givare för matning ger ingen signal",
            "Ingen åtgärd ännu.", "VM kontrollerar givaren och kablaget.", 4)
            .ChangeStatus(IssueStatus.WaitingForTeam, "Väntar på VM", DaysAgo(3));
        Add(4, IssueCategory.MachineFault, Team.Maintenance, "Hydraulläckage vid pressbordet",
            "Absorbent utlagd.", "UH byter tätning.", 1)
            .ChangeStatus(IssueStatus.WaitingForTeam, "Väntar på reservdel", DaysAgo(0.8));

        // Fixed, waiting for quality
        Add(6, IssueCategory.Measurement, Team.Production, "Planhet utanför tolerans på några detaljer",
            "Justerat, nya detaljer mäts OK.", "Kvalitet verifierar nästa batch.", 7, v6601)
            .ChangeStatus(IssueStatus.Fixed, "Justerat och kontrollmätt", DaysAgo(6));

        // Verified and closed
        var pressMark = Add(1, IssueCategory.ToolDamage, Team.Toolmaker, "Tryckmärke på ovansidan, höger kavitet",
            "Slitna delar i verktyget bytta.", null, 6, v1101);
        pressMark.ChangeStatus(IssueStatus.Fixed, "Delar bytta av VM", DaysAgo(5));
        pressMark.ChangeStatus(IssueStatus.Verified, "Kvalitet godkände detaljerna", DaysAgo(4));

        // Recurring: verified, then reopened when the problem came back
        var recurring = Add(1, IssueCategory.Mark, Team.Toolmaker, "Märke intill hål på höger kavitet",
            "Verktyget polerat.", "Bevaka om märket kommer tillbaka.", 12, v1101);
        recurring.ChangeStatus(IssueStatus.Fixed, "Polerat av VM", DaysAgo(10));
        recurring.ChangeStatus(IssueStatus.Verified, "Godkänt av kvalitet", DaysAgo(9));
        recurring.ChangeStatus(IssueStatus.Open, "Märket har kommit tillbaka", DaysAgo(1));

        db.Issues.AddRange(issues);
        await db.SaveChangesAsync();
    }

    // Comments for reports where something is not normal
    private static readonly string[] ProblemNotes =
    [
        "Grad noterad, verktygsmakare informerad.",
        "Mätning nära toleransgränsen, kvalitet kontaktad.",
        "Kort stopp för justering.",
        "Extra kontroll genomförd.",
        "Uppföljning behövs vid nästa skift."
    ];

    private static ShiftReport RandomReport(Random rng, DateOnly date, Shift shift,
        Machine machine, Tool tool, Operator reporter)
    {
        // Mostly normal, sometimes a problem, like real production
        var toolStatus = rng.Next(100) switch
        {
            < 78 => ToolStatus.GoodCondition,
            < 88 => ToolStatus.Worn,
            < 93 => ToolStatus.ToolProblem,
            < 96 => ToolStatus.WaitingForToolmaker,
            < 98 => ToolStatus.ToolChangeInProgress,
            _ => ToolStatus.Crash
        };

        // Rule 1: a crash always means Ej okej
        var machineStatus = toolStatus == ToolStatus.Crash
            ? MachineStatus.NotOk
            : rng.Next(100) switch
            {
                < 85 => MachineStatus.ReadyToRun,
                < 89 => MachineStatus.Test,
                < 93 => MachineStatus.HalfService,
                < 96 => MachineStatus.ServiceInProgress,
                _ => MachineStatus.NotOk
            };

        var qualityStatus = rng.Next(100) switch
        {
            < 82 => QualityStatus.Approved,
            < 90 => QualityStatus.NeedsQualityCheck,
            < 95 => QualityStatus.MeasurementInProgress,
            < 98 => QualityStatus.NotOk,
            _ => QualityStatus.Blocked
        };

        // Rules 2 and 5: anything not normal gets a comment
        var allNormal = toolStatus == ToolStatus.GoodCondition
            && machineStatus == MachineStatus.ReadyToRun
            && qualityStatus == QualityStatus.Approved;
        var comment = allNormal ? null : ProblemNotes[rng.Next(ProblemNotes.Length)];

        // Reports are written during the shift: 06-14, 14-22, 22-06
        var shiftStart = shift switch { Shift.Day => 6, Shift.Evening => 14, _ => 22 };
        var created = date.ToDateTime(new TimeOnly(shiftStart, 0), DateTimeKind.Utc)
            .AddMinutes(rng.Next(480));

        return ShiftReport.Create(date, shift, machine, tool, reporter,
            machineStatus, toolStatus, qualityStatus, comment, created);
    }
}
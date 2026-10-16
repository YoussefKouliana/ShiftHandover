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

        var machines = new[] { "Maskin 1", "Maskin 2", "Maskin 3", "Maskin 4", "Maskin 5", "Maskin 6", "Riktverk" }
            .Select(name => new Machine { Name = name })
            .ToList();
        db.Machines.AddRange(machines);

        var issues = new List<Issue>();

        Machine M(string name) => machines.Single(m => m.Name == name);
        DateTime DaysAgo(double days) => now.AddDays(-days);

        Issue Add(string machine, IssueCategory category, Team team, string title,
            string? action, string? followUp, double daysAgo)
        {
            var issue = new Issue
            {
                Machine = M(machine),
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

        // Open issues
        Add("Maskin 1", IssueCategory.Burr, Team.Toolmaker,
            "Grad vid klippkant på kavitet 2",
            "Graden noterad, berörda detaljer sorterade.",
            "VM bör åtgärda innan graden växer och blir svår att trumla bort.", 2);

        Add("Maskin 2", IssueCategory.Burr, Team.Toolmaker,
            "Grad på hålkanter, flera kaviteter",
            null,
            "Kontrollera verktyget och åtgärda graden.", 9);

        Add("Maskin 6", IssueCategory.Mark, Team.Quality,
            "Repor på ytan efter trumling",
            "Detaljerna spärrade för bedömning.",
            "Kvalitet bedömer om detaljerna kan godkännas.", 2);

        // In progress
        var radius = Add("Maskin 1", IssueCategory.Measurement, Team.Quality,
            "Radie på vänster kavitet nära toleransgränsen",
            "Ommätt med annan fixtur, mätningen godkänd men nära gränsen.",
            "Kvalitet följer upp nästa batch.", 3);
        radius.ChangeStatus(IssueStatus.InProgress, "Kvalitet mäter extra detaljer", DaysAgo(2));

        var testSetup = Add("Maskin 3", IssueCategory.Measurement, Team.Quality,
            "Nytt mätupplägg under testperiod",
            "Detaljer körda och mätta, protokoll lämnat till ansvarig.",
            "Utvärdera resultatet efter testperioden.", 10);
        testSetup.ChangeStatus(IssueStatus.InProgress, "Resultaten ser lovande ut", DaysAgo(8));

        var service = Add("Maskin 5", IssueCategory.MachineFault, Team.Maintenance,
            "Planerad service av smörjsystemet",
            "Service påbörjad.",
            null, 5);
        service.ChangeStatus(IssueStatus.InProgress, "UH har påbörjat servicen", DaysAgo(4));

        var crackedPunch = Add("Maskin 2", IssueCategory.ToolDamage, Team.Toolmaker,
            "Sprucken stans upptäckt vid kontroll",
            "Maskinen stoppad, reservstans monterad.",
            "VM byter till ny stans.", 0.2);
        crackedPunch.ChangeStatus(IssueStatus.InProgress, "Reservstans monterad, körs vidare", DaysAgo(0.1));

        // Waiting for another team
        var feedSensor = Add("Maskin 2", IssueCategory.MachineFault, Team.Toolmaker,
            "Givare för matning ger ingen signal",
            "Ingen åtgärd ännu.",
            "VM kontrollerar givaren och kablaget.", 4);
        feedSensor.ChangeStatus(IssueStatus.WaitingForTeam, "Väntar på VM", DaysAgo(3));

        var leak = Add("Maskin 4", IssueCategory.MachineFault, Team.Maintenance,
            "Hydraulläckage vid pressbordet",
            "Absorbent utlagd, maskinen körs med tillsyn.",
            "UH byter tätning vid nästa stopp.", 1);
        leak.ChangeStatus(IssueStatus.WaitingForTeam, "Väntar på UH och reservdel", DaysAgo(0.8));

        // Fixed, waiting for quality to verify
        var flatness = Add("Riktverk", IssueCategory.Measurement, Team.Production,
            "Planhet utanför tolerans på några detaljer",
            "Riktverket justerat, nya detaljer mäts OK.",
            "Kvalitet verifierar nästa batch.", 7);
        flatness.ChangeStatus(IssueStatus.Fixed, "Justerat och kontrollmätt", DaysAgo(6));

        // Verified and closed
        var pressMark = Add("Maskin 1", IssueCategory.ToolDamage, Team.Toolmaker,
            "Tryckmärke på ovansidan, höger kavitet",
            "Slitna delar i verktyget bytta.",
            null, 6);
        pressMark.ChangeStatus(IssueStatus.Fixed, "Delar bytta av VM", DaysAgo(5));
        pressMark.ChangeStatus(IssueStatus.Verified, "Kvalitet godkände detaljerna", DaysAgo(4));

        var instruction = Add("Maskin 3", IssueCategory.Other, Team.Production,
            "Ny instruktion för uppstart efter verktygsbyte",
            "Instruktionen uppsatt vid maskinen.",
            null, 14);
        instruction.ChangeStatus(IssueStatus.Fixed, null, DaysAgo(13));
        instruction.ChangeStatus(IssueStatus.Verified, null, DaysAgo(12));

        // Recurring: verified, then reopened when the problem came back
        var recurring = Add("Maskin 1", IssueCategory.Mark, Team.Toolmaker,
            "Märke intill hål på höger kavitet",
            "Verktyget polerat.",
            "Bevaka om märket kommer tillbaka.", 12);
        recurring.ChangeStatus(IssueStatus.Fixed, "Polerat av VM", DaysAgo(10));
        recurring.ChangeStatus(IssueStatus.Verified, "Godkänt av kvalitet", DaysAgo(9));
        recurring.ChangeStatus(IssueStatus.Open, "Märket har kommit tillbaka", DaysAgo(1));

        db.Issues.AddRange(issues);
        await db.SaveChangesAsync();
    }
}
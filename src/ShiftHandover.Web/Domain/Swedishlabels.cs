namespace ShiftHandover.Web.Domain;

// Code is in English, the user interface is in Swedish.
// All translations live here so the UI never shows raw enum names.
public static class SwedishLabels
{
    public static string ToSwedish(this IssueStatus status) => status switch
    {
        IssueStatus.Open => "Öppet",
        IssueStatus.InProgress => "Pågår",
        IssueStatus.WaitingForTeam => "Väntar på annan avdelning",
        IssueStatus.Fixed => "Åtgärdat",
        IssueStatus.Verified => "Verifierat",
        _ => status.ToString()
    };

    public static string ToSwedish(this Team team) => team switch
    {
        Team.Production => "Produktion",
        Team.Toolmaker => "Verktygsmakare (VM)",
        Team.Maintenance => "Underhåll (UH)",
        Team.Quality => "Kvalitet",
        _ => team.ToString()
    };

    public static string ToSwedish(this IssueCategory category) => category switch
    {
        IssueCategory.Burr => "Grad",
        IssueCategory.Mark => "Märke/repa",
        IssueCategory.Measurement => "Mätning",
        IssueCategory.ToolDamage => "Verktygsskada",
        IssueCategory.MachineFault => "Maskinfel",
        IssueCategory.Other => "Övrigt",
        _ => category.ToString()
    };
      public static string ToSwedish(this Shift shift) => shift switch
    {
        Shift.Day => "Morgonskift",
        Shift.Evening => "Kvällsskift",
        Shift.Night => "Nattskift",
        _ => shift.ToString()
    };

    public static string ToSwedish(this MachineStatus status) => status switch
    {
        MachineStatus.Running => "Kör",
        MachineStatus.Stopped => "Stoppad",
        MachineStatus.Service => "Service",
        _ => status.ToString()
    };

    public static string ToSwedish(this ToolStatus status) => status switch
    {
        ToolStatus.Ok => "OK",
        ToolStatus.Worn => "Slitage",
        ToolStatus.Damaged => "Skadat",
        ToolStatus.NeedsChange => "Byte behövs",
        _ => status.ToString()
    };

    public static string ToSwedish(this QualityStatus status) => status switch
    {
        QualityStatus.Approved => "Godkänt",
        QualityStatus.CheckRequested => "Kontroll begärd",
        QualityStatus.Blocked => "Spärrat",
        _ => status.ToString()
    };
}
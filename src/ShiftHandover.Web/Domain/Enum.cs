namespace ShiftHandover.Web.Domain;

public enum IssueStatus
{
    Open,
    InProgress,
    WaitingForTeam,
    Fixed,
    Verified
}

public enum Team
{
    Production,
    Toolmaker,
    Maintenance,
    Quality
}

public enum IssueCategory
{
    Burr,
    Mark,
    Measurement,
    ToolDamage,
    MachineFault,
    Other
}
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

public enum Shift
{
    Day,
    Evening,
    Night
}

public enum MachineStatus
{
    ReadyToRun,
    NotOk,
    Empty,
    HalfService,
    ServiceInProgress,
    Test
}

public enum ToolStatus
{
    GoodCondition,
    Crash,
    ToolProblem,
    WaitingForToolmaker,
    Worn,
    ToolChangeInProgress
}

public enum QualityStatus
{
    Approved,
    NotOk,
    MeasurementInProgress,
    NeedsQualityCheck,
    Blocked
}
namespace ShiftHandover.Web.Domain;

public class Part
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;   // e.g. "D-118"
    public string Name { get; set; } = string.Empty;     // e.g. "Fäste vänster"
}